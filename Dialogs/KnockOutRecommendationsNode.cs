using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;

using MultiDialogsBot.Helper;

namespace MultiDialogsBot.Dialogs
{
 /*   [Serializable]
    public class KnockOutRecommendationsNode : CommonDialog
    {
        readonly Dictionary<NodeLUISPhoneDialog.EIntents, string> englishDescriptions;
        List<FeatureIntent> features;
            
        enum EFeatureType
        {
            boolean = 0,
            numericDouble,
            enumerated,
            releaseDate,
            numericDouble2D,
            numericDouble3D
        }

        [Serializable]
        struct FeatureIntent : IComparable    
        {
            public NodeLUISPhoneDialog.EIntents intent;
            public string desc;
            public EFeatureType type;

            int IComparable.CompareTo(object obj)
            {
                if (obj is FeatureIntent)
                {
                    FeatureIntent other = (FeatureIntent)obj;
                    return desc.CompareTo(other.desc);
                }
                else
                    throw new Exception("Error...Comparing FeatureIntent against an incompatible object type");
            }
            public FeatureIntent(NodeLUISPhoneDialog.EIntents i,string s,EFeatureType t) :this(i,t)
            {
                desc = s;
            }

            public FeatureIntent(NodeLUISPhoneDialog.EIntents i,EFeatureType t)
            {
                desc = null;
                intent = i;
                type = t;
            }
        }

        IntentDecoder decoder;


        public KnockOutRecommendationsNode(IntentDecoder dec)
        {
            FeatureIntent temp;

            decoder = dec;
            handSets = dec.PhonesLeft;
            features = new List<FeatureIntent>
            {
                new FeatureIntent(NodeLUISPhoneDialog.EIntents.BandWidth,EFeatureType.boolean),
                new FeatureIntent ( NodeLUISPhoneDialog.EIntents.FMRadio,EFeatureType.boolean),
                new FeatureIntent( NodeLUISPhoneDialog.EIntents.DualCamera, EFeatureType.boolean),
                new FeatureIntent( NodeLUISPhoneDialog.EIntents.DualSIM,EFeatureType.boolean),
                new FeatureIntent( NodeLUISPhoneDialog.EIntents.ExpandableMemory,EFeatureType.boolean),
                new FeatureIntent( NodeLUISPhoneDialog.EIntents.FaceID,EFeatureType.boolean),
                new FeatureIntent( NodeLUISPhoneDialog.EIntents.GPS,EFeatureType.boolean),
                new FeatureIntent( NodeLUISPhoneDialog.EIntents.WiFi,EFeatureType.boolean),
                new FeatureIntent( NodeLUISPhoneDialog.EIntents.HDVoice,EFeatureType.boolean),
                new FeatureIntent( NodeLUISPhoneDialog.EIntents.SecondaryCamera,EFeatureType.boolean),
                new FeatureIntent( NodeLUISPhoneDialog.EIntents.WaterResist,EFeatureType.boolean),
                new FeatureIntent( NodeLUISPhoneDialog.EIntents.BatteryLife,EFeatureType.boolean),
                new FeatureIntent( NodeLUISPhoneDialog.EIntents.Camera,EFeatureType.numericDouble),
                new FeatureIntent( NodeLUISPhoneDialog.EIntents.HighResDisplay,EFeatureType.numericDouble2D),
                new FeatureIntent( NodeLUISPhoneDialog.EIntents.LargeStorage,EFeatureType.numericDouble),
                new FeatureIntent( NodeLUISPhoneDialog.EIntents.ScreenSize,EFeatureType.numericDouble),
                new FeatureIntent( NodeLUISPhoneDialog.EIntents.Cheap,EFeatureType.numericDouble),
                new FeatureIntent( NodeLUISPhoneDialog.EIntents.Small,EFeatureType.numericDouble3D),
                new FeatureIntent( NodeLUISPhoneDialog.EIntents.Weight,EFeatureType.numericDouble),
                new FeatureIntent( NodeLUISPhoneDialog.EIntents.Color,EFeatureType.enumerated),
                new FeatureIntent( NodeLUISPhoneDialog.EIntents.OS,EFeatureType.enumerated),
                new FeatureIntent( NodeLUISPhoneDialog.EIntents.Brand,EFeatureType.enumerated),
                new FeatureIntent( NodeLUISPhoneDialog.EIntents.Newest,EFeatureType.releaseDate)

            };
            englishDescriptions = new Dictionary<NodeLUISPhoneDialog.EIntents,string>()
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
                { NodeLUISPhoneDialog.EIntents.LargeStorage,"MemoryMB"  },
                { NodeLUISPhoneDialog.EIntents.ScreenSize, "ScreenSize" },
                { NodeLUISPhoneDialog.EIntents.Cheap, "Price" },
                { NodeLUISPhoneDialog.EIntents.Small, "BodySize" },
                { NodeLUISPhoneDialog.EIntents.Weight, "Weight" },
                { NodeLUISPhoneDialog.EIntents.Color, "Colors"},
                { NodeLUISPhoneDialog.EIntents.OS, "OS" },
                { NodeLUISPhoneDialog.EIntents.Brand, "Brand" },
                { NodeLUISPhoneDialog.EIntents.Newest, "ReleaseDate"  }
            };

            for (int i = 0; i < features.Count; ++i)
            {
                temp = features[i];
                temp.desc = englishDescriptions[features[i].intent];
                features[i] = temp;
            }
            features.Sort();
        }

        public override async Task StartAsync(IDialogContext context)
        {
            int bagCount;

            if (debugMessages) await context.PostAsync($"DEBUG : StartAsync() method in KnockOutRecommendations");

            bagCount = handSets.BagCount();
            await context.PostAsync($"I still have {bagCount} phones available and I'm trying to work out the right one for you.");
            await ShowOptionsAsync(context);
        }

        public async Task MessageReceivedAsync(IDialogContext context,IAwaitable<object> awaitable)
        {
            if (debugMessages) await context.PostAsync("DEBUG : KnockOutRecommendationNode::MessageReceivedAsync() ");
            await context.PostAsync("0 OK, 0:1");

            context.Wait(MessageReceivedAsync);
        }

        private async Task ChoiceReceivedAsync(IDialogContext context,IAwaitable<string> awaitable)
        {
            string ans = await awaitable;
            FeatureIntent aux = new FeatureIntent();
            int index,numberRemoved;

            if (debugMessages) await context.PostAsync("DEBUG : He picked " + ans);

            aux.desc = ans;
            index = features.BinarySearch(aux);
            if (debugMessages) await context.PostAsync("DEBUG : index = " + index);
            if (debugMessages) await context.PostAsync("DEBUG : Phrase is " + features[index].desc);
            if (index < 0)
                throw new Exception("Error...String was not found");
            aux = features[index];
            if (features[index].type != EFeatureType.enumerated)
            {
                numberRemoved = decoder.DecodeIntent(aux.intent,null,false);
                if (debugMessages) await context.PostAsync($"DEBUG : Narrowed it down to {numberRemoved}");
            }
            if (debugMessages) await context.PostAsync("DEBUG : Bag string representation : " + decoder.PhonesLeft.BuildStrRep());
            if (decoder.PhonesLeft.BagCount() <= 4)
            {
                context.Done(decoder);
            }
            else
                context.Wait(MessageReceivedAsync);
        }

        private async Task ShowOptionsAsync(IDialogContext context)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            string msg = "What of the following would you say it's the most important for your decision?";  
            List<string> options = new List<string>();
            List<NodeLUISPhoneDialog.EIntents> top4 = decoder.IntentsRanking(sb);

            foreach (var feature in top4)
                options.Add(englishDescriptions[feature]);
            PromptDialog.Choice(context, ChoiceReceivedAsync, options, msg, "Not understood, please try again", 4);
        }
    }*/
}