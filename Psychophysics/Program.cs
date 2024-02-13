using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Psychophysics
{
    public enum STATUS { HIT, MISS, TBD } // to be determined
    public enum STEP_TYPE { DB, LOG, LIN }

    public enum RESPONSE { CORRECT, INCORRECT }

    public enum DIRECTION { START, UP, DOWN }

    public enum GAME { PRESTART, ONGOING, FINISHED }


    class Program
    {
        static void RunStaircase()
        {
            List<int> step_sizes = new List<int>() { 8, 4, 4, 2 };

            StairHandler sh = new StairHandler(1.0f, STEP_TYPE.DB, step_sizes);

            bool finished = true;
            // insert 30 responses
            // - while the computed intensity values are above 0.3 insert correct responses
            // - when they get below 0.3 insert incorrect responses
            // - we expect the found value to be around 0.3
            for (int index = 0; index < 30; index++)
            {
                if (sh.intensities.Count > 0 && sh.intensities[sh.intensities.Count - 1] > 0.3)
                {
                    double intensity = sh.addResponse(RESPONSE.CORRECT);
                    sh.intensities.Add(intensity);
                }
                else
                {
                    double intensity = sh.addResponse(RESPONSE.INCORRECT);
                    sh.intensities.Add(intensity);
                }
                if (sh.finished == true && finished == true)
                {
                    Console.WriteLine($"The algorithm has actually stopped already at step {sh.intensities.Count} with intensity {sh.intensities[sh.intensities.Count-1]}");
                    finished = false;
                }
            }

            Console.WriteLine($"Number of responses: {sh.intensities.Count}");
            foreach (var intensity in sh.intensities)
            {
                Console.WriteLine($"--> Intensity value: {intensity}");
            }
        }

        static void RunQuest()
        {

            //List<int> step_sizes = new List<int>() { 8, 4, 4, 2 };
            //QuestHandler qh = new QuestHandler(1.0f, 0.3f, 0.63f, 0.01f, STEP_TYPE.DB, step_sizes);
            QuestHandler qh = new QuestHandler(1.0f, 0.3f, 0.63f, 0.01f);

            bool finished = true;
            // insert 30 responses
            // - while the computed intensity values are above 0.3 insert correct responses
            // - when they get below 0.3 insert incorrect responses
            // - we expect the found value to be around 0.3
            for (int index = 0; index < 30; index++)
            {
                if (qh.intensities.Count > 0 && qh.intensities[qh.intensities.Count - 1] > 0.3)
                {
                    double intensity = qh.addResponse(RESPONSE.CORRECT);
                    qh.intensities.Add(intensity);
                }
                else
                {
                    double intensity = qh.addResponse(RESPONSE.INCORRECT);
                    qh.intensities.Add(intensity);
                }
                if (qh.finished == true && finished == true)
                {
                    Console.WriteLine($"The algorithm has actually stopped already at step {qh.intensities.Count} with intensity {qh.intensities[qh.intensities.Count - 1]}");
                    finished = false;
                }
            }

            Console.WriteLine($"Number of responses: {qh.intensities.Count}");
            foreach (var intensity in qh.intensities)
            {
                Console.WriteLine($"--> Intensity value: {intensity}");
            }
        }

        static void Main(string[] args)
        {
            RunStaircase();
            //RunQuest();
        }
    }
}
