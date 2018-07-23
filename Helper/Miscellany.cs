﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;

using MultiDialogsBot.Database;

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

        public static string QueryCompare(string query, string alteredQuery, StringBuilder sb = null)
        {
            string[] alteredWords,returnValue ;
            List<Tuple<string, int>> alteredWordIndexes = new List<Tuple<string, int>>();
            List<string> words;
            int index, altListIndex, counter = 0 ;
            string aux;

            words = new List<string>(query.Split(' '));
            alteredWords = alteredQuery.Split(' ');
            returnValue = new string[alteredWords.Length];
            for (index = 0; index < alteredWords.Length; index++)
                alteredWordIndexes.Add(new Tuple<string, int>(alteredWords[index], index));
            alteredWordIndexes.Sort((a, b) => (a.Item1.CompareTo(b.Item1)));  
            words.Sort();
            index = altListIndex = 0;

            do
            {
                if (sb != null)
                    sb.Append($"it = {counter}, index = {index}, contents = {words[index]}, alteredIndex= {altListIndex},contents = {alteredWordIndexes[altListIndex].Item1}\r\n");
                while ((index != words.Count) && (0 > words[index].ToLower().CompareTo(alteredWordIndexes[altListIndex].Item1.ToLower())))
                    index++;
                 
                while ((altListIndex != alteredWords.Length) && 
                       ((index == words.Count) || ((0 > alteredWordIndexes[altListIndex].Item1.ToLower().CompareTo(words[index].ToLower())))))
                {
                    aux = alteredWordIndexes[altListIndex].Item1;
                    returnValue[alteredWordIndexes[altListIndex].Item2] = aux.Length != 0 ? string.Concat("**", aux, "**") : aux;
                    ++altListIndex;
                }

                while ((altListIndex != alteredWords.Length) && (index != words.Count) && (alteredWordIndexes[altListIndex].Item1.ToLower() == words[index].ToLower()))
                { 
                    returnValue[alteredWordIndexes[altListIndex].Item2] = alteredWordIndexes[altListIndex].Item1;
                    ++altListIndex;
                    ++index;
                }
                if (++counter >= 100)
                    break;
            }
            while (altListIndex != alteredWords.Length);
            return string.Join(" ", returnValue);
        }

        public static void SortCarousel(List<Tuple<HeroCard,HandSetFeatures>> unsortedCarousel)
        {
            Comparison<Tuple<HeroCard,HandSetFeatures>> comparer;

            comparer = new Comparison<Tuple<HeroCard,HandSetFeatures>>(Comparer);
            unsortedCarousel.Sort(comparer);
        }
           
        public static async Task InsertDelayAsync(IDialogContext context )
        {
            ConnectorClient connectorClient; 
            ITypingActivity typingActivity;
            Activity msg = (Activity)context.Activity;
               
            connectorClient = new ConnectorClient(new Uri(msg.ServiceUrl));
            typingActivity = (msg).CreateReply();
            typingActivity.Type = ActivityTypes.Typing;
            connectorClient.Conversations.SendToConversationAsync((Activity)typingActivity);
            Thread.Sleep(2200);  // 2200 b4
        }

        public static string BuildBrandString(List<string> brands)
        {  
            int len = brands.Count;
            StringBuilder sb = new StringBuilder();

            sb.Append($"{brands[0]}");
            if (len > 1)
            {
                for (int x = 1; x < (len - 1); ++x)
                    sb.Append($",{brands[x]}");
                sb.Append($" and {brands[len - 1]}");
            }    
            return sb.ToString();
        }

        private static int Comparer(Tuple<HeroCard,HandSetFeatures> hs1,Tuple<HeroCard,HandSetFeatures> hs2)
        {
            int brandCmpResult = hs1.Item2.Brand.CompareTo(hs2.Item2.Brand),dateCmpResult;

            if (brandCmpResult == 0)
            {
                dateCmpResult = hs2.Item2.ReleaseDate.CompareTo(hs1.Item2.ReleaseDate);

                if (dateCmpResult == 0)
                    return hs2.Item2.Model.CompareTo(hs1.Item2.Model);
                else
                    return dateCmpResult;
            }
            else
                return brandCmpResult;
        }
    }
}