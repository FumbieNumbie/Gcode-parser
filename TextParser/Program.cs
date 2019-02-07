using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace TextParser
{
    class Program
    {
        static void Main(string[] args)
        {
            int argCount = args.Length;
            string fileContents = "";
            bool lengths = false;
            bool abs = false;
            if (argCount > 0)
            {
                if (Array.IndexOf(args, "-l") != -1)
                {
                    lengths = true;
                }
                if (Array.IndexOf(args, "-abs") != -1)
                {
                    abs = true;
                }
                if (Array.IndexOf(args, "-help") != -1)
                {
                    Console.WriteLine("  -abs -- the program will run in absolute values mode.");
                    Console.WriteLine("  -l   -- the program will display total line length and total extrusion length.");
                    Console.WriteLine();
                    Console.WriteLine("  Without parameters the program will compare the score to several baselines with following parameters:");
                    Console.WriteLine("Filament diameter is set to 1.75 mm, line width is 0.4 mm and line heights are 0.1, 0.15, 0.2, 0.3 mm.");
                    Console.Read();
                    Environment.Exit(0);
                }
            }
            string fileName = "test";
        Mark:
            Console.Write("Enter a file name: ");
            fileName = Console.ReadLine();
            fileName = GetFullName(fileName);
            DateTime now = DateTime.Now;
            try
            {
                using (StreamReader reader = new StreamReader(fileName, true))
                {
                    fileContents = reader.ReadToEnd();
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Wrong file name. Try again.");
                goto Mark;
            }

            double ratio = 0;

            string[] lines = fileContents.Split('\n');
            List<string> refinedList = RefineList(lines);
            //foreach (string item in refinedList)
            //{
            //	Console.WriteLine(item);
            //}

            ratio = GetRatio(refinedList, lengths);
            if (abs)
            {
                Console.WriteLine("Ratio: " + ratio);
            }
            else
            {
                Console.WriteLine("Comparison of line density.");
                Console.WriteLine("0.10mm layer height: " + Math.Round(ratio / 16.446, 2));
                Console.WriteLine("0.15mm layer height: " + Math.Round(ratio / 24.67, 2));
                Console.WriteLine("0.20mm layer height: " + Math.Round(ratio / 37.005, 2));
                Console.WriteLine("0.30mm layer height: " + Math.Round(ratio / 49.34, 2));

            }
            Console.WriteLine("Elapsed time: " + Math.Round((DateTime.Now - now).TotalSeconds, 1) + " sec");
            Console.WriteLine("Would you like to parse another file?");
            Console.WriteLine("Y/N (default is 'Y')");
            string answer = Console.ReadLine();
            if (answer == "n")
            {
                Environment.Exit(0);
            }
            else
            {
                goto Mark;
            }
        }
        private static string GetFullName(string fileName)
        {
            int forwardSIndex = fileName.LastIndexOf(@"/");
            int backwardSIndex = fileName.LastIndexOf(@"\");
            if (forwardSIndex != -1)
            {
                fileName = fileName.Substring(0, forwardSIndex) + @"\" + fileName.Substring(forwardSIndex + 1) + @".gcode";
            }
            else if (backwardSIndex != -1)
            {
                fileName = fileName.Substring(0, backwardSIndex) + @"\" + fileName.Substring(backwardSIndex + 1) + @".gcode";
            }
            else
            {
                fileName = Directory.GetCurrentDirectory() + @"\" + fileName + @".gcode";
            }


            return fileName;
        }
        /*
        private static string GetFullName(string fileName)
        {
            string shortName = fileName;
            string path = Directory.GetCurrentDirectory();
            int lastB = fileName.LastIndexOf(@"\");
            int lastF = fileName.LastIndexOf(@"/");


            if (lastF != -1)
            {
                path = fileName.Substring(0, lastF);
                shortName = fileName.Substring(lastF + 1);
            }
            if (lastB != -1)
            {
                path = fileName.Substring(0, lastB);
                shortName = fileName.Substring(lastB + 1);
            }
            string patternSt = "^" + path + shortName + @".gcode$";
            Console.WriteLine(patternSt);
            string[] files = new string[0];
            try
            {
                files = Directory.GetFiles(path);
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("There is no such directory. The program will be closed.");
                Console.Read();
                Environment.Exit(0);
            }
            Regex pattern = new Regex(patternSt, RegexOptions.IgnoreCase);
            foreach (string file in files)
            {
                //Match match = pattern.Match(file);
                //if (match.Success)
                //{
                //    fileName = file;
                //}
                Match match = pattern.Match(file);
                if (match.Success)
                {
                    if (match.Value[0] == shortName[0])
                    {
                        fileName = file;
                        break;
                    }
                }
            }

            Console.WriteLine("Parsing file " + fileName + ", please, wait.");
            return fileName;
        }
        */
        /// <summary>
        /// Cleans the initial array from strings that have nothing to do with calculations.
        /// </summary>
        /// <param name="lines">An array of strings.</param>
        private static List<string> RefineList(string[] lines)
        {
            List<string> list = new List<string>();
            foreach (string item in lines)
            {
                Regex patternM = new Regex("M");

                if (patternM.Match(item).Success == false)
                {

                    Regex patternX = new Regex(@"X\d+\.*\d*");
                    Regex patternY = new Regex(@"\sY\d+\.*\d*");
                    Regex patternE = new Regex(@"E\d+\.*\d*");
                    Match matchX = patternX.Match(item);
                    Match matchY = patternY.Match(item);
                    Match matchE = patternE.Match(item);
                    string line = "";
                    // Create a line containing only coordinates and extrusion
                    if (matchX.Success)
                    {
                        line += matchX.Value;
                    }
                    if (matchY.Success)
                    {
                        line += matchY.Value;

                    }
                    if (matchE.Success)
                    {
                        line += " " + matchE.Value;
                    }
                    if (matchX.Success)
                    {
                        list.Add(line);
                    }
                }

            }
            return list;
        }
        /// <summary>
        /// The result of division of line's length by extrusion length.
        /// </summary>
        /// <param name="refList">Refined list of input elements.</param>
        private static double GetRatio(List<string> refList, bool lengths = false)
        {
            double totalLength = 0;
            float currentE = 0;
            float lastE = 0;
            float totalE = 0;
            for (int i = 1; i < refList.Count; i++)
            {
                Regex pattern = new Regex(@"(E\d+\.*\d*)");
                Match match = pattern.Match(refList[i]);
                if (match.Success)
                {
                    currentE = float.Parse(match.Groups[1].Value.Substring(1));

                    if (lastE > currentE)
                    {
                        totalE += lastE;
                    }
                    lastE = currentE;
                }
                if (i == refList.Count - 1)
                {
                    Regex anyE = new Regex(@"(E\d+\.*\d*)");
                    for (int k = i; k > 0; k--)
                    {
                        Match anyMatch = anyE.Match(refList[k]);
                        if (anyMatch.Success)
                        {
                            totalE += float.Parse(anyMatch.Value.Substring(1));
                            break;
                        }
                    }
                }

                bool hop = CheckHops(refList[i - 1], refList[i]);
                if (!hop) //if these two lines are not a hop
                {
                    Vector start = GetSegmentPoint(refList, i - 1);
                    Vector end = GetSegmentPoint(refList, i);
                    Vector segment = end - start;
                    totalLength += segment.Length();
                }

            }
            if (lengths)
            {
                Console.WriteLine("Total line length: " + totalLength);
                Console.WriteLine("Total extrusion length: " + totalE);
            }

            return totalE / totalLength * 1000;

        }
        /// <summary>
        /// Creates one of the points of a line segment.
        /// </summary>
        /// <param name="refList">A refined list of coordinates and extrusions.</param>
        /// <param name="i">An index of an element in the list.</param>
        private static Vector GetSegmentPoint(List<string> refList, int i)
        {
            string[] line = refList[i].Split(' ');
            float x = float.Parse(line[0].Substring(1));
            float y = float.Parse(line[1].Substring(1));
            Vector point = new Vector(x, y);
            return point;
        }

        /// <summary>
        /// Checks if there is no extrusion in a segment.
        /// </summary>
        /// <param name="st1">A string corresponding to the segment's start.</param>
        /// <param name="st2">A string corresponding to the segment's end.</param>
        /// <returns></returns>
        private static bool CheckHops(string st1, string st2)
        {
            Regex pattern1 = new Regex(@"X\d+\.*\d*");
            Regex pattern2 = new Regex(@"\sY\d+\.*\d*");
            Regex pattern3 = new Regex(@"\sE\d+\.*\d*");
            bool x = pattern1.Match(st2).Success;
            bool y = pattern2.Match(st2).Success;
            bool e = pattern3.Match(st2).Success;
            if (!pattern1.Match(st1).Success)
            {
                return true;
            }
            if (x && y && e)
            {
                return false;
            }
            else return true;
        }
    }
}
