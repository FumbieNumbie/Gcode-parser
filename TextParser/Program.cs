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

			bool abs = false;
			if (argCount > 0)
			{
				if (Array.IndexOf(args, "-abs") != -1)
				{
					abs = true;
				}
				if (Array.IndexOf(args, "-help") != -1)
				{
					Console.WriteLine("  -abs -- the program will run in absolute values mode.");
					Console.WriteLine();
					Console.WriteLine("  Without parameters the program will compare the score to the baseline which is Ender3 test dog (0.4 mm extruder and 1.75 mm filament).");
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

			ratio = GetRatio(refinedList, abs);

			Console.WriteLine("Ratio: " + Math.Round(ratio,2));
			Console.WriteLine("Elapsed time: " + Math.Round((DateTime.Now - now).TotalSeconds,1)+ " sec");
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
			string path = Directory.GetCurrentDirectory();
			string[] files = Directory.GetFiles(path);
			Regex pattern = new Regex(fileName + @"\.");
			foreach (string file in files)
			{
				Match match = pattern.Match(file);
				if (match.Success)
				{
					fileName = file;
				}
			}

			return fileName;
		}

		/// <summary>
		/// Cleans the initial array from strings that have nothing to do with calculations.
		/// </summary>
		/// <param name="lines">An array of strings.</param>
		private static List<string> RefineList(string[] lines)
		{
			List<string> list = new List<string>();
			foreach (string item in lines)
			{
				Regex pattern1 = new Regex(@"X\d+\.*\d*");
				Regex pattern2 = new Regex(@"\sY\d+\.*\d*");
				Regex pattern3 = new Regex(@"E\d+\.*\d*");
				Match match1 = pattern1.Match(item);
				Match match2 = pattern2.Match(item);
				Match match3 = pattern3.Match(item);
				string line = "";
				// Create a line containing only coordinates and extrusion
				if (match1.Success)
				{
					line += match1.Value;
				}
				if (match2.Success)
				{
					line += match2.Value;

				}
				if (match3.Success)
				{
					line += " " + match3.Value;
				}
				if (match1.Success)
				{
					list.Add(line);
				}

			}
			return list;
		}
		/// <summary>
		/// The result of division of line's length by extrusion length.
		/// </summary>
		/// <param name="refList">Refined list of input elements.</param>
		private static double GetRatio(List<string> refList, bool absolute = false)
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
				if (i == refList.Count-1)
				{
					Regex anyE = new Regex(@"(E\d+\.*\d*)");
					Match anyMatch = anyE.Match(refList[i]);
					if (anyMatch.Success)
					{
						totalE += float.Parse(anyMatch.Value.Substring(1));
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
			if (absolute == true)
			{
				return totalLength / totalE;
			}
			else
			{
				return 40.52 / totalLength * totalE;
			}
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
