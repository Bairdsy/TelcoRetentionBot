using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Text;

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
        List<Tuple<string, string,int>> ranking;
        Dictionary<string, int> numberOfHits;

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
                { NodeLUISPhoneDialog.EIntents.BatteryLife,"BatteryLife"},  
                { NodeLUISPhoneDialog.EIntents.Camera,"Camera" },
                { NodeLUISPhoneDialog.EIntents.ScreenSize, "Screen" },
                { NodeLUISPhoneDialog.EIntents.Cheap, "Price" },
                { NodeLUISPhoneDialog.EIntents.Small, "Size of Phone" }, 
                { NodeLUISPhoneDialog.EIntents.Weight, "Weight" },
                { NodeLUISPhoneDialog.EIntents.OS, "Operating System" },
                { NodeLUISPhoneDialog.EIntents.Brand, "Brand" },
                { NodeLUISPhoneDialog.EIntents.Newest, "Recent Phones"  }
            };
            english2Intent = new Dictionary<string, NodeLUISPhoneDialog.EIntents>();
            foreach (var key in englishDescriptions.Keys)
                english2Intent.Add(englishDescriptions[key], key);

            theDecoder = decoder;
            ranking = MongoDBAccess.Instance.GetFeatureRanking();
            numberOfHits = new Dictionary<string, int>();
            foreach (var tuple in ranking)
                numberOfHits.Add(tuple.Item1, tuple.Item3);
        }

        public int numberOfFeatures()
        {
            return ranking.Count;
        }
        public SuggestedActions GetTop4Buttons( StringBuilder debug)
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
            ranking = new List<Tuple<string, string,int>>(ranking.Where((tuple, i) => (!indexes4Removal.Contains(i) && !intents2Exclude.Contains(english2Intent[tuple.Item1]))));
            
            for (int i = 0; i < ranking.Count; ++i)
            {
                debug.Append(this.ranking[i].Item1 + " ==> ");
                if (!theDecoder.KnocksSomeButNotAll(english2Intent[ranking[i].Item1]))
                {
                    debug.Append("No");
                }
                else
                    debug.Append("Yes");
                  
                debug.Append("\r\n");
                debug.Append($"name: {ranking[i].Item1}, description : {ranking[i].Item2}");
            }
            max = Math.Min(ranking.Count, BotConstants.TOP_INTENTS);
            actions.Add(new CardAction() { Title = "Show me all", Type = ActionTypes.ImBack, Value = "Show me all" });
            for (int i = 0; i < max; ++i)
                actions.Add(new CardAction() { Title= ranking[i].Item1, Type = ActionTypes.ImBack, Value = ranking[i].Item2 });

            return new SuggestedActions() { Actions = actions };
        }

        public void SetNewFreq(NodeLUISPhoneDialog.EIntents feature, StringBuilder debug)
        {   
            string englishDesc; 
            int freq;   

            for (int i = 0; i < ranking.Count; ++i)       
            { 
                debug.Append(this.ranking[i].Item1 + " ==> ");
                debug.Append($"name: {ranking[i].Item1}, description : {ranking[i].Item2}");
                debug.Append("\r\n");
            }
            if (!englishDescriptions.TryGetValue(feature,out englishDesc))
                return;
            englishDesc = englishDescriptions[feature];
            freq = numberOfHits[englishDesc];

            numberOfHits[englishDesc] = ++freq;
            MongoDBAccess.Instance.SetFeatureFrequency(englishDescriptions[feature], freq );
            ranking.Sort((x, y) => -Math.Sign(x.Item3 - y.Item3));
        }
    }
}