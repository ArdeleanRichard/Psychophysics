using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Psychophysics
{
    public class QuestHandler : StairHandler
    {
        public double startValStd;
        public double stopInterval;
        public double range;

        public double pThreshold;
        public double xThreshold;
        public double beta;
        public double delta;
        public double gamma;
        public double grain;

        public int dim;

        public bool updatePDF = true;
        public bool warnPDF = true;
        public bool normalizePDF = true;

        public double tGuess = 1.0;
        public double tGuessSd = 0.3;

        public LinkedList<double> i = new LinkedList<double>();
        public LinkedList<double> x = new LinkedList<double>();
        public LinkedList<double> i2 = new LinkedList<double>();
        public LinkedList<double> pdf = new LinkedList<double>();
        public LinkedList<double> x2 = new LinkedList<double>();
        public LinkedList<double> p2_init = new LinkedList<double>();
        public LinkedList<double> p2 = new LinkedList<double>();

        public double quantileOrder;

        public QuestHandler(double startVal, double startValStd, double pThreshold, double gamma, STEP_TYPE step_type, List<int> step_sizes) : base(startVal, step_type, step_sizes)
        {
            this.startVal = startVal;
            this.startValStd = startValStd;
            this.pThreshold = pThreshold;

            this.beta = 3.5;
            this.delta = 0.01;
            this.gamma = 0.01;
            this.grain = 0.01;

            this.minValue = 0f;
            this.maxValue = 1.0f;
            this.numberTrials = 20;

            if (this.range == 0)
                this.dim = 500;
            else
            {
                dim = 2 * (int)Math.Ceiling((this.range / this.grain) / 2.0);
            }
            recompute();

        }

        public QuestHandler(double startVal, double startValStd, double pThreshold, double gamma) : base(startVal)
        {
            this.startVal = startVal;
            this.startValStd = startValStd;
            this.pThreshold = pThreshold;

            this.beta = 3.5;
            this.delta = 0.01;
            this.gamma = 0.01;
            this.grain = 0.01;

            this.minValue = 0f;
            this.maxValue = 1.0f;
            this.numberTrials = 20;

            if (this.range == 0)
                this.dim = 500;
            else
            {
                dim = 2 * (int)Math.Ceiling((this.range / this.grain) / 2.0);
            }
            recompute();

        }

        private static LinkedList<double> normalize_divide_by_sum(LinkedList<double> pdf, double pdf_sum)
        {
            for (LinkedListNode<double> node = pdf.First; node != null; node = node.Next)
            {
                node.Value = node.Value / pdf_sum;
            }
            return pdf;
        }

        public void recompute()
        {
            if (updatePDF == false)
                return;
            if (this.gamma > this.pThreshold)
                this.gamma = 0.5;

            for (double value = -this.dim / 2; value < this.dim / 2 + 1; value++)
                i.AddLast(value);

            for (double value = -this.dim; value < this.dim + 1; value++)
                i2.AddLast(value);

            double pdf_sum = 0;
            for (LinkedListNode<double> node = i.First; node != null; node = node.Next)
            {
                double x_value = node.Value * grain;
                x.AddLast(x_value);
                double pdf_value = Math.Exp(-0.5 * Math.Pow(x_value / this.tGuessSd, 2));
                pdf.AddLast(pdf_value);
                pdf_sum += pdf_value;
            }

            pdf = normalize_divide_by_sum(pdf, pdf_sum);


            for (LinkedListNode<double> node = i2.First; node != null; node = node.Next)
            {
                double x2_value = node.Value * grain;
                x2.AddLast(x2_value);

                double exp = Math.Exp((double)-1 * Math.Pow(10, this.beta * x2_value));
                double p2_value = this.delta * this.gamma + (1f - this.delta) * (1f - (1f - this.gamma) * exp);

                p2_init.AddLast(p2_value);
            }



            List<int> indexes = new List<int>();
            int index = 0;
            for (LinkedListNode<double> node = p2_init.First; node.Next != null; node = node.Next)
            {
                if (node.Next.Value - node.Value != 0f)
                {
                    indexes.Add(index);
                }
                index += 1;
            }

            //create sorted list for interpolation of p2[indexes] and x2[indexes]
            var knownPairsXY = new SortedList<double, double>();
            foreach (var id in indexes)
            {
                try
                {
                    knownPairsXY.Add(p2_init.ElementAt(id), x2.ElementAt(id));
                }
                catch (System.ArgumentException argumentException)
                {

                }


            }

            for (int i = 0; i < knownPairsXY.Count; i++)
            {
                if (knownPairsXY.ElementAt(i).Key > pThreshold)
                {
                    this.xThreshold = knownPairsXY.ElementAt(i - 1).Value + (pThreshold - knownPairsXY.ElementAt(i - 1).Key) * (knownPairsXY.ElementAt(i).Value - knownPairsXY.ElementAt(i - 1).Value) / (knownPairsXY.ElementAt(i).Key - knownPairsXY.ElementAt(i - 1).Key);
                    // y0 + (x - x0) * (y1 - y0) / (x1 - x0)

                    break;
                }

            }


            for (LinkedListNode<double> node = x2.First; node != null; node = node.Next)
            {
                double p2_value = this.delta * this.gamma + (1 - this.delta) * (1 - (1 - this.gamma) * Math.Exp((double)-1f * Math.Pow(10, this.beta * (node.Value + this.xThreshold))));
                p2.AddLast(p2_value);
            }

            double eps = 1e-14;

            double pL = p2.First.Value;
            double pH = p2.Last.Value;

            double pE = pH * Math.Log(pH + eps) - Math.Log(pL + eps) + (1 - pH + eps) * Math.Log(1 - pH + eps) - (1 - pL + eps) * Math.Log(1 - pL + eps);
            pE = 1 / (1 + Math.Exp(pE / (pL - pH)));
            this.quantileOrder = (pE - pL) / (pH - pL);

            for (int j = 0; j < intensities.Count; j++)
            {
                double inten = Math.Max(-1e10, Math.Min(1e10, intensities[j]));

                List<double> ii = new List<double>();
                for (LinkedListNode<double> node = this.i.First; node != null; node = node.Next)
                    ii.Add((double)pdf.Count + node.Value - Math.Round((inten - this.tGuess) / this.grain) - 1);

                if (ii[0] < 0)
                {
                    for (int k = 0; k < ii.Count; k++)
                        ii[k] = ii[k] - ii[0];
                }

                List<int> iii = new List<int>();
                for (int k = 0; k < ii.Count; k++)
                    iii.Add((int)ii[k]);

                if (normalizePDF)
                {
                    pdf = normalize_divide_by_sum(pdf, pdf_sum);
                }

            }
            if (normalizePDF)
            {
                pdf = normalize_divide_by_sum(pdf, pdf_sum);
            }


        }

        public double next()
        {
            if (finished == false)
            {
                intensities.Add(nextIntensity);
                return nextIntensity;
            }
            else
                return -1f;
        }

        public double addResponse(RESPONSE response)
        {
            updateQuest(response, nextIntensity);

            return nextIntensity;
        }

        private void updateQuest(RESPONSE response, double intensity)
        {
            if (updatePDF == true)
            {
                double inten = Math.Max(-1e10, Math.Min(1e10, intensity));

                List<double> ii = new List<double>();
                for (LinkedListNode<double> node = this.i.First; node != null; node = node.Next)
                    ii.Add((double)pdf.Count + node.Value - Math.Round((inten - this.tGuess) / this.grain) - 1);

                if (ii[0] < 0)
                {
                    for (int k = 0; k < ii.Count; k++)
                        ii[k] = ii[k] - ii[0];
                }

                List<int> iii = new List<int>();
                for (int k = 0; k < ii.Count; k++)
                    iii.Add((int)ii[k]);

                if (normalizePDF)
                {
                    double pdf_sum = 0;
                    for (LinkedListNode<double> node = pdf.First; node != null; node = node.Next)
                    {
                        pdf_sum += node.Value;
                    }
                    pdf = normalize_divide_by_sum(pdf, pdf_sum);
                }
            }

            responses.Add(response);
            intensities.Add(intensity);
            calculateNextIntensity();
        }

    }

}
