using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Gameplay.Script.Tools
{
    public class ExcelReader
    {
        public static List<string[]> ReadCSV(TextAsset csvFile)
        {
            List<string[]> data = new List<string[]>();
            string[] lines = csvFile.text.Split('\n');
        
            foreach (string line in lines)
            {
                string[] row = line.Split(',');
                data.Add(row);
            }
            return data;
        }

        public static List<string[]> Parse(string csvText)
        {
            List<string[]> parsedData = new List<string[]>();
            
            string pattern = @"(((?<x>(?=[,\r\n]+)))|(""(?<x>([^""]|"""")+)"")|((?<x>[^,\r\n]+))),?";
            Regex regex = new Regex(pattern, RegexOptions.ExplicitCapture);
        
            string[] lines = csvText.Split('\n');
        
            foreach(string line in lines)
            {
                if(string.IsNullOrWhiteSpace(line)) continue;
            
                List<string> fields = new List<string>();
                MatchCollection matches = regex.Matches(line);
            
                foreach(Match match in matches)
                {
                    fields.Add(match.Groups["x"].Value.Replace("\"\"", "\""));
                }
            
                parsedData.Add(fields.ToArray());
            }
        
            return parsedData;
        }
        
    }
}