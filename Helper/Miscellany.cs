using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;

using MultiDialogsBot.Database;
using MultiDialogsBot.Dialogs;

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
           
        public static async Task InsertDelayAsync(IDialogContext context, bool noDelay = false )
        {
            ConnectorClient connectorClient; 
            ITypingActivity typingActivity;
            Activity msg = (Activity)context.Activity;
               
            connectorClient = new ConnectorClient(new Uri(msg.ServiceUrl));
            typingActivity = (msg).CreateReply();
            typingActivity.Type = ActivityTypes.Typing;
            connectorClient.Conversations.SendToConversationAsync((Activity)typingActivity);
            if (!noDelay)
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

        public static bool IsANo(string ans)
        {
            if (ans.ToLower() == "n")
                return true;

            return (ans.ToLower().StartsWith("no") && ((ans.Length == 2) || !char.IsLetter(ans[2])));
        }

        public static string GetCorrectCongratsMessage(IDialogContext context,HandSetFeatures chosenModel)
        {
            NodeLUISPhoneDialog.EIntents feature;
            NodeLuisSubsNeeds.ENeeds need ;
            string ramSize,scrResolution, prefix = "Excellent selection - ", suffix;
            

            suffix = chosenModel.DualCamera == 0 ? String.Empty : " and has dual cammera";
            if (!context.ConversationData.TryGetValue<NodeLUISPhoneDialog.EIntents>(BotConstants.LAST_FEATURE_KEY, out feature))
            {
                if (!context.ConversationData.TryGetValue<NodeLuisSubsNeeds.ENeeds>(BotConstants.LAST_NEED_KEY, out need))
                    return $"Now let's work what is the best plan for your {Capitalize(chosenModel.Model)}";
                else
                {
                    if (chosenModel.RamSize >= 1024)
                        ramSize = ((int)chosenModel.RamSize / 1024) + " GB";
                    else
                        ramSize = (int)chosenModel.RamSize + " MB";
                    scrResolution = string.Format("{0} x {1}", chosenModel.DisplayResolution[0], chosenModel.DisplayResolution[1]);
                    switch (need)
                    {
                        case NodeLuisSubsNeeds.ENeeds.PictureLover:
                            return prefix + $"The {Capitalize(chosenModel.Model)} has {chosenModel.Camera} MegaPixels {suffix}, the best feature to take pictures.";
                        case NodeLuisSubsNeeds.ENeeds.MovieWatcher:
                            return prefix + $"The {Capitalize(chosenModel.Model)} is highly regarded because of its screen and battery performance to watch your favourite shows";
                        case NodeLuisSubsNeeds.ENeeds.GamesAddict:
                            return prefix + $"The {Capitalize(chosenModel.Model)} has {ramSize} and a dislplay resolution of {scrResolution}";
                        default:
                            return $"Now let's work what is the best plan for your {Capitalize(chosenModel.Model)}";
                    }
                }
            }
            else
            {
                switch (feature)
                {
                    case NodeLUISPhoneDialog.EIntents.FeaturePhone:
                        return prefix + $"The {Capitalize(chosenModel.Model)} is a great simple phone";
                    case NodeLUISPhoneDialog.EIntents.Cheap:
                        return prefix + $"The {Capitalize(chosenModel.Model)} is one of the best offers that we have right now";
                    case NodeLUISPhoneDialog.EIntents.OS:
                        return prefix + $"The {Capitalize(chosenModel.OS)} operating system is a very reliable choice";
                    case NodeLUISPhoneDialog.EIntents.Camera:
                        return prefix + $"The {Capitalize(chosenModel.Model)} has {chosenModel.Camera} MegaPixels {suffix}.";
                    case NodeLUISPhoneDialog.EIntents.BatteryLife:
                        return prefix + $"The {Capitalize(chosenModel.Model)} has a long battery life ({chosenModel.BatteryLife} hours).";
                    case NodeLUISPhoneDialog.EIntents.Small:
                        return prefix + $"The {Capitalize(chosenModel.Model)} is popular for its size";
                    case NodeLUISPhoneDialog.EIntents.ScreenSize:
                        return prefix + $"The {Capitalize(chosenModel.Model)} is highly regarded by its screen performance";
                    case NodeLUISPhoneDialog.EIntents.Newest:
                        return prefix + $"The {Capitalize(chosenModel.Model)} is a very new model";
                    case NodeLUISPhoneDialog.EIntents.Weight:
                        return prefix + $"The {Capitalize(chosenModel.Model)} is a very popular phone due to its light weight";
                    case NodeLUISPhoneDialog.EIntents.Brand:
                        return prefix + $"The {Capitalize(chosenModel.Brand)} is a very reliable choice";
                    default:
                        return $"Now let's work what is the best plan for your {Capitalize(chosenModel.Model)}";
                }
            }
        }
    }
}