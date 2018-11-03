using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace TextParser
{
	class Program
	{
		static void Main(string[] args)
		{
			string fileContents;
			using (StreamReader reader = new StreamReader(@"C:\users\aalex\desktop\file.txt", Encoding.UTF8))
			{
				fileContents = reader.ReadToEnd();
			}

			string[] lines = fileContents.Split('\n');
			List<string> refinedList = RefineList(lines);
			foreach (var item in refinedList)
			{
				System.Console.WriteLine(item);
			}
			float raito = GetRatio(refinedList);
			System.Console.WriteLine("Ratio: " + raito);
		}

		private static List<string> RefineList(string[] lines)
		{
			List<string> list = new List<string>();
			foreach (string item in lines)
			{
				Regex pattern1 = new Regex(@"X\d+\.*\d*");
				Regex pattern2 = new Regex(@"\sY\d+\.*\d*");
				Regex pattern3 = new Regex(@"\sE\d+\.*\d*");
				Match match1 = pattern1.Match(item);
				Match match2 = pattern2.Match(item);
				Match match3 = pattern3.Match(item);
				string line = "";
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
					line += match3.Value;
				}
				list.Add(line);

			}

			return list;
		}

		private static float GetRatio(List<string> refList)
		{
			float totalLength = 0;
			float lastE = 0, currentE = 0;
			float deltaE = 0;
			float totalE = 0;
			for (int i = 1; i < refList.Count; i++)
			{
				Regex pattern = new Regex(@"(E\d+\.*\d*)");
				Match match = pattern.Match(refList[i]);
				if (match.Success)
				{
					currentE = float.Parse(match.Groups[1].Value.Substring(1));
					deltaE = currentE - lastE;
					if (deltaE != currentE)
					{
						totalE += deltaE;
					}
					lastE = currentE;
				}
				string[] line = refList[i].Split(' ');
				bool hop = CheckHops(refList[i - 1], refList[i]);
				if (!hop) //if these two lines are not a hop
				{

					Vector2 start = GetSegmentPoint(refList, i - 1);
					Vector2 end = GetSegmentPoint(refList, i);
					Vector2 segment = end - start;
					totalLength += segment.Length();
				}
			}
			return totalLength / totalE;
		}

		private static Vector2 GetSegmentPoint(List<string> refList, int i)
		{
			string[] line = refList[i].Split(' ');
			float x = float.Parse(line[0].Substring(1));
			float y = float.Parse(line[1].Substring(1));
			Vector2 point = new Vector2(x, y);
			return point;
		}

		private static bool CheckHops(string st1, string st2)
		{
			Regex pattern = new Regex(@"E\d+\.*\d*");
			Match match1 = pattern.Match(st1);
			Match match2 = pattern.Match(st2);
			if (!match1.Success && match2.Success)
			{
				return false;
			}
			else if (match1.Success && match2.Success)
			{
				return false;
			}
			else return true;

		}
	}
}
