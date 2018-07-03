using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace MultiDialogsBot.Helper
{
    public static class Miscellany
    {
        public static string Capitalize(string str2Capitalize)
        {
            string temp, allLower = str2Capitalize.ToLower();
            string[] tokens;
            int beginIndex = 1,unitsIndex;
            StringBuilder returnValue = new StringBuilder();

            tokens = allLower.Split(' ');


            if (tokens[0].StartsWith("iphone"))     
                returnValue.Append("iP" + tokens[0].Substring(2));
            else if (tokens[0].StartsWith("htc") || tokens[0].StartsWith("nokia"))
                returnValue.Append(tokens[0].ToUpper());
            else
                beginIndex = 0;
            for (int x = beginIndex; x < tokens.Length; ++x)
            {
                if (tokens[x].Length == 0)
                    continue;
                if (-1 != (unitsIndex = tokens[x].IndexOf("gb", 1)) && char.IsDigit(tokens[x][unitsIndex - 1]))
                    temp = tokens[x].Replace("gb", "GB");
                else
                    temp = tokens[x];
                returnValue.Append((x == 0 ? string.Empty : " ") + temp.TrimStart(' ').First().ToString().ToUpper() + temp.Substring(1));    
                                          //  str2Capitalize.TrimStart(' ').First().ToString().ToUpper() + str2Capitalize.Substring(1); b4
            }
            return returnValue.ToString();
        }

        public static double Product(IEnumerable<double> vector)
        {
            double returnValue = 1;

            foreach (var factor in vector)
                returnValue *= factor;
            return returnValue;
        }

        public static string RemoveSpaces(string strWithSpaces)
        { 
            return string.Concat(strWithSpaces.ToLower().Split(' '));
        }
    }
}