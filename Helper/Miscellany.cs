using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MultiDialogsBot.Helper
{
    public static class Miscellany
    {
        public static string Capitalize(string str2Capitalize)
        {
            string allLower = str2Capitalize.ToLower();

            if (allLower.StartsWith("iphone"))
                return "iP" + str2Capitalize.Substring(2);
            else if (allLower.StartsWith("htc"))
                return allLower.ToUpper();
            else
                return str2Capitalize.First().ToString().ToUpper() + str2Capitalize.Substring(1);
        }

        public static double Product(IEnumerable<double> vector)
        {
            double returnValue = 1;

            foreach (var factor in vector)
                returnValue *= factor;
            return returnValue;
        }
    }
}