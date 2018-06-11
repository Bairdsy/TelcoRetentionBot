using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Microsoft.Bot.Connector;

using MultiDialogsBot.Database;
using MultiDialogsBot.Dialogs;

namespace MultiDialogsBot.Helper
{
    public class TopFeatures
    {
        readonly Dictionary<NodeLUISPhoneDialog.EIntents, string> englishDescriptions;
        readonly Dictionary<string, NodeLUISPhoneDialog.EIntents> english2Intent;  

        IntentDecoder theDecoder;
        List<Tuple<string, string>> ranking;

        public IntentDecoder AssociatedDecoder
        {
            get
            {
                return theDecoder;
            }
         }

        public TopFeatures(IntentDecoder decoder)
        {
            englishDescriptions = new Dictionary<NodeLUISPhoneDialog.EIntents, string>()
            {
                { NodeLUISPhoneDialog.EIntents.BandWidth,"BandWidth"},
                { NodeLUISPhoneDialog.EIntents.FMRadio, "FM Radio"},
                { NodeLUISPhoneDialog.EIntents.DualCamera, "DualCamera" },
                { NodeLUISPhoneDialog.EIntents.DualSIM, "DualSIM"},
                { NodeLUISPhoneDialog.EIntents.ExpandableMemory, "ExpandableMemory" },
                { NodeLUISPhoneDialog.EIntents.FaceID, "FaceId" },
                { NodeLUISPhoneDialog.EIntents.GPS,"GPS" },
                { NodeLUISPhoneDialog.EIntents.WiFi, "WiFi" },
                { NodeLUISPhoneDialog.EIntents.HDVoice, "HDVoice" },
                { NodeLUISPhoneDialog.EIntents.SecondaryCamera,"SecondaryCamera" },
                { NodeLUISPhoneDialog.EIntents.WaterResist,"WaterResist" },
                { NodeLUISPhoneDialog.EIntents.BatteryLife,"BatteryLife"},
                { NodeLUISPhoneDialog.EIntents.Camera,"Camera" },
                { NodeLUISPhoneDialog.EIntents.HighResDisplay,"DisplayResolution" },
                { NodeLUISPhoneDialog.EIntents.LargeStorage,"StorageMB"  },
                { NodeLUISPhoneDialog.EIntents.ScreenSize, "ScreenSize" },
                { NodeLUISPhoneDialog.EIntents.Cheap, "Price" },
                { NodeLUISPhoneDialog.EIntents.Small, "BodySize" },
                { NodeLUISPhoneDialog.EIntents.Weight, "Weight" },
                { NodeLUISPhoneDialog.EIntents.Color, "Colors"},
                { NodeLUISPhoneDialog.EIntents.OS, "OS" },
                { NodeLUISPhoneDialog.EIntents.Brand, "Brand" },
                { NodeLUISPhoneDialog.EIntents.Newest, "ReleaseDate"  }
            };
            english2Intent = new Dictionary<string, NodeLUISPhoneDialog.EIntents>();
            foreach (var key in englishDescriptions.Keys)
                english2Intent.Add(englishDescriptions[key], key);

            theDecoder = decoder;
            ranking = MongoDBAccess.Instance.GetFeatureRanking();
        }

        public int numberOfFeatures()
        {
            return ranking.Count;
        }
        public SuggestedActions GetTop4Buttons(System.Text.StringBuilder debug)
        {
            List<CardAction> actions = new List<CardAction>();
            List<int> indexes4Removal = new List<int>();
            List<NodeLUISPhoneDialog.EIntents> intents2Exclude;
            int max;


            debug.Append("buttons : ");
            for (int i = 0; i < ranking.Count; ++i)
            {
                debug.Append(this.ranking[i].Item1 + " ==> ");
                if (!theDecoder.KnocksSomeButNotAll(english2Intent[ranking[i].Item1]))
                {     
                    indexes4Removal.Add(i);
                    debug.Append("No");
                }
                else
                    debug.Append("Yes");
                
                debug.Append("\r\n");
                debug.Append($"name: {ranking[i].Item1}, description : {ranking[i].Item2}");
            }
            intents2Exclude = theDecoder.Exclude;
            ranking = new List<Tuple<string, string>>(ranking.Where((tuple, i) => (!indexes4Removal.Contains(i) && !intents2Exclude.Contains(english2Intent[tuple.Item1]))));
            max = Math.Min(ranking.Count, BotConstants.TOP_INTENTS);
            for (int i = 0; i < max; ++i)
                actions.Add(new CardAction() { Title= ranking[i].Item1, Type = ActionTypes.ImBack, Value = ranking[i].Item2 });
            return new SuggestedActions() { Actions = actions };
        }


    }
}