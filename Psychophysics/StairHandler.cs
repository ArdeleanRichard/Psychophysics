using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Psychophysics
{
    public class StairHandler
    {

        public double maxValue;
        public double minValue;
        public List<RESPONSE> responses = new List<RESPONSE>();
        public List<double> reversalIntensities = new List<double>();
        public List<double> intensities = new List<double>();
        public int correctCounter = 0;
        public bool reversal;
        public bool applyInitialRule = true;
        public bool initialRule = true;
        public bool finished = false;
        public bool variableStep = false;

        public List<int> reversalPoints = new List<int>();
        public int currentTrialNumber;

        public List<int> step_sizes;
        public int currentStepSize;

        public DIRECTION currentDirection = DIRECTION.START;
        public STEP_TYPE step_type;

        public double startVal;
        public int numberTrials;
        public int numberUp;
        public int numberDown; // correct responses before stim goes down
        public int numberReversals = 0;
        public double nextIntensity;

        public double threshold = -1.0;
        public int maximumTrials = 30;

        public StairHandler(double startVal)
        {
            this.startVal = startVal;
            this.step_type = STEP_TYPE.DB;
            this.step_sizes = new List<int> { 4 };
            this.nextIntensity = startVal;
            this.numberTrials = 1;
            this.numberUp = 1;
            this.numberDown = 3;

            this.minValue = 0f;
            this.maxValue = 1.0f;

            if (step_sizes.Count > 1)
                variableStep = true;

            currentStepSize = step_sizes[0];
            intensities.Add(nextIntensity);

            if (numberReversals == 0)
            {
                numberReversals = step_sizes.Count;
            }
        }

        public StairHandler(double startVal, STEP_TYPE step_type, List<int> step_sizes)
        {
            this.startVal = startVal;
            this.step_type = step_type;
            this.step_sizes = step_sizes;
            this.nextIntensity = startVal;
            this.numberTrials = 1;
            this.numberUp = 1;
            this.numberDown = 3;

            this.minValue = 0f;
            this.maxValue = 1.0f;

            if (step_sizes.Count > 1)
                variableStep = true;

            currentStepSize = step_sizes[0];
            intensities.Add(nextIntensity);

            if (numberReversals == 0)
            {
                numberReversals = step_sizes.Count;
            }
        }

        public double next()
        {
            if (finished == false)
            {
                currentTrialNumber += 1;
                intensities.Add(nextIntensity);
                return nextIntensity;
            }
            else
                return -1f;
        }

        public double addResponse(RESPONSE response)
        {
            responses.Add(response);

            if (response == RESPONSE.CORRECT)
                if (responses.Count > 1 && responses[responses.Count - 2] == response)
                    correctCounter += 1;
                else
                    correctCounter = 1;
            else
                if (responses.Count > 1 && responses[responses.Count - 2] == response)
                correctCounter -= 1;
            else
                correctCounter = -1;

            calculateNextIntensity();

            return nextIntensity;

        }

        public void calculateNextIntensity()
        {
            if (reversalIntensities.Count == 0 && applyInitialRule == true)
            {
                if (responses[responses.Count - 1] == RESPONSE.CORRECT)
                {
                    if (currentDirection == DIRECTION.UP)
                        reversal = true;
                    else
                        reversal = false;
                    currentDirection = DIRECTION.DOWN;
                }
                else
                {
                    if (currentDirection == DIRECTION.DOWN)
                        reversal = true;
                    else
                        reversal = false;
                    currentDirection = DIRECTION.UP;
                }
            }
            else
            {
                if (correctCounter >= numberDown)
                {
                    // n right, time to go down!
                    if (currentDirection == DIRECTION.UP)
                        reversal = true;
                    else
                        reversal = false;
                }
                else
                {
                    if (correctCounter <= -numberUp)
                    {
                        // n wrong, time to go up!
                        // note current direction
                        if (currentDirection == DIRECTION.DOWN)
                            reversal = true;
                        else
                            reversal = false;
                        currentDirection = DIRECTION.DOWN;
                    }
                    else
                        // same as previous trial
                        reversal = false;
                    currentDirection = DIRECTION.UP;
                }


            }

            // add reversal info
            if (reversal == true)
            {
                reversalPoints.Add(currentTrialNumber);
                if (reversalIntensities.Count == 0 && applyInitialRule == true)
                    initialRule = true;
                reversalIntensities.Add(intensities[intensities.Count - 1]);
            }

            // test if we're done
            if (reversalIntensities.Count >= numberReversals && intensities.Count >= numberTrials)
            {
                finished = true;

                if (threshold == -1.0)
                {

                }
            }
            else
            {
                if (intensities.Count >= maximumTrials)
                {
                    finished = true;
                }
            }

            // new step size if necessary
            if (reversal == true && variableStep == true)
            {
                if (reversalIntensities.Count >= step_sizes.Count)
                    // we've gone beyond the list of step sizes
                    // so just use the last one
                    currentStepSize = step_sizes[step_sizes.Count - 1];
                else
                {
                    int size = reversalIntensities.Count;
                    currentStepSize = step_sizes[size];
                }
            }

            // apply new step size
            if ((reversalIntensities.Count == 0 || initialRule == true) && applyInitialRule == true)
            {
                initialRule = false;
                if (responses[responses.Count - 1] == RESPONSE.CORRECT)
                    intensityDecrement();
                else
                    intensityIncrement();
            }
            else
            {
                if (correctCounter >= numberDown)
                    // n right, so going down
                    intensityDecrement();
                else
                {
                    if (correctCounter <= -numberUp)
                        // n wrong, so going up
                        intensityIncrement();
                }
            }
        }


        public double intensityDecrement()
        {
            if (this.step_type == STEP_TYPE.DB)
            {
                nextIntensity /= Math.Pow(10f, (currentStepSize / 20f));
            }
            if (this.step_type == STEP_TYPE.LOG)
            {
                nextIntensity /= Math.Pow(10, currentStepSize);
            }
            if (this.step_type == STEP_TYPE.LIN)
            {
                nextIntensity -= currentStepSize;
            }

            correctCounter = 0;

            if (nextIntensity < minValue)
                nextIntensity = minValue;

            return nextIntensity;
        }

        public double intensityIncrement()
        {
            if (this.step_type == STEP_TYPE.DB)
            {
                nextIntensity *= Math.Pow(10, (currentStepSize / 20f));
            }
            if (this.step_type == STEP_TYPE.LOG)
            {
                nextIntensity *= Math.Pow(10, currentStepSize);
            }
            if (this.step_type == STEP_TYPE.LIN)
            {
                nextIntensity += currentStepSize;
            }

            if (nextIntensity > maxValue)
                nextIntensity = maxValue;

            return nextIntensity;
        }
    }
}
