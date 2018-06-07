using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Text;
using System.Threading.Tasks;


using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

using MultiDialogsBot.Helper;
using MultiDialogsBot.Database;


namespace MultiDialogsBot.Dialogs
{
    [LuisModel("f245439f-5379-464a-8481-e68985e4504b", "a3a20fb04cad4cfcaf7b821bd1eb9a19", LuisApiVersion.V2, null, SpellCheck = true, Verbose = true)]
    [Serializable]
    public class NodeLUISPhoneDialog : LuisDialog<object>
    {
        public enum EIntents
        {
            None = 0,
            BandWidth,
            BatteryLife,
            Brand,
            Camera,
            Cheap,
            Color,
            DualCamera,
            DualSIM,
            ExpandableMemory,   
            FMRadio,
            FaceID,
            GPS,
            HDVoice,
            HighResDisplay,
            LargeStorage,
            OS,
            ScreenSize,
            SecondaryCamera,
            Small,
            WaterResist,
            Weight,
            WiFi,
            Newest
        }

        string brandDesired;
        DateTime? ReleaseDateCurrentModel;
        IntentDecoder decoder;
        HandSets handSetsBag;
        int numberOfIterations = 1;

        public NodeLUISPhoneDialog(HandSets handSets, string brand, DateTime? currentModelReleaseDate,List<string> narrowedListOfModels) : base()
        {
            handSetsBag = handSets;
            brandDesired = brand;
            ReleaseDateCurrentModel = currentModelReleaseDate;
            decoder = new IntentDecoder(handSets, brand, currentModelReleaseDate,narrowedListOfModels);
        }

        [LuisIntent("None")]
        public async Task None(IDialogContext context,LuisResult result)
        {
            await context.PostAsync("Not understood");
            await ShowDebugInfoAsync(context, result);
        }

        [LuisIntent("BandWidth")]
        public async Task BandWidth(IDialogContext context, LuisResult result)
        {
            int handsetsLeft , handsetsNow = decoder.CurrentNumberofHandsetsLeft();

            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that you want a phone with access to internet and with wide bandwidth");
            handsetsLeft = decoder.DecodeIntent(EIntents.BandWidth,result);

            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : Bag contents = " + handSetsBag.BuildStrRep());

            await UpdateUserAsync(context, handsetsLeft,handsetsNow);
        }


        [LuisIntent("BatteryLife")]
        public async Task BatteryLife(IDialogContext context, LuisResult result)
        {
            int handsetsLeft,handSetsNow = decoder.CurrentNumberofHandsetsLeft();

            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand you want a big battery life");
            try
            {
                handsetsLeft = decoder.DecodeIntent(EIntents.BatteryLife,result);
                if (CommonDialog.debugMessages) await context.PostAsync($"DEBUG : I have here {handsetsLeft} equipments left, bag contents = {handSetsBag.BuildStrRep()}, bag count = {handSetsBag.BagCount()}");
                await UpdateUserAsync(context, handsetsLeft,handSetsNow);
            }
            catch (ArgumentException)
            {
                await context.PostAsync("Argument xception");
            }
            catch (InvalidOperationException)
            {
                await context.PostAsync("Error ... Invalid operation xception");
            }
            catch (Exception xception)
            {
                await context.PostAsync($"Error...Exception Message = {xception.Message}");
            }
        }

        [LuisIntent("Brand")]
        public async Task Brand(IDialogContext context, LuisResult result)
        {
            int handSetsNow = handSetsBag.BagCount();
            int handSetsLeft = 0;

            await ShowDebugInfoAsync(context, result);
           
            await context.PostAsync("I understand that the most important thing for you is brand");
            /*
            if (brandsRequired.Count == 0)
            {
                if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : He didn't specify, Needs to call the node to specifically pick a brand");hfhth
            }*/
            handSetsLeft = decoder.DecodeIntent(EIntents.Brand, result);
            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : Bag contents : " + handSetsBag.BuildStrRep());
            await UpdateUserAsync(context, handSetsLeft, handSetsNow);
        }

        [LuisIntent("Camera")]
        public async Task Camera(IDialogContext context, LuisResult result)
        {
            int handSetsLeft, handSetsNow = decoder.CurrentNumberofHandsetsLeft();
            StringBuilder debugStr = new StringBuilder("DEBUG : DecodeIntent : ");

            await ShowDebugInfoAsync(context, result);

            await context.PostAsync("I understand that the most important thing for you is the presence of a camera");

            handSetsLeft = decoder.DecodeIntent(EIntents.Camera, result,debugStr);
            await context.PostAsync(debugStr.ToString());
            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : Bag contents : " + handSetsBag.BuildStrRep());
            await UpdateUserAsync(context, handSetsLeft, handSetsNow);
        }

        [LuisIntent("Cheap")]
        public async Task Cheap(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
        }

        [LuisIntent("DualCamera")]
        public async Task DualCamera(IDialogContext context, LuisResult result)
        {
            int handSetsLeft, handSetsNow = decoder.CurrentNumberofHandsetsLeft();

            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that you want a phone with Dual Camera");
            handSetsLeft = decoder.DecodeIntent(EIntents.DualCamera, result);
            if (CommonDialog.debugMessages) await context.PostAsync("Bag contents : " + handSetsBag.BuildStrRep());
            await UpdateUserAsync(context,handSetsLeft,handSetsNow);
        }

        [LuisIntent("DualSIM")]
        public async Task DualSIM(IDialogContext context, LuisResult result)
        {
            int handSetsLeft, handSetsNow = decoder.CurrentNumberofHandsetsLeft();

            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I undertand that you would like a phone with DualSIM");
            handSetsLeft = decoder.DecodeIntent(EIntents.DualSIM,result);
            if (CommonDialog.debugMessages) await context.PostAsync("Bag contents : " + handSetsBag.BuildStrRep());
            await UpdateUserAsync(context, handSetsLeft, handSetsNow);
        }

        [LuisIntent("ExpandableMemory")]
        public async Task ExpandableMemory(IDialogContext context, LuisResult result)
        {
            int handSetsLeft, handSetsNow = decoder.CurrentNumberofHandsetsLeft();

            await ShowDebugInfoAsync(context, result);

            await context.PostAsync("I understand that an expandable memory is the most important thing for you");

            handSetsLeft = decoder.DecodeIntent(EIntents.ExpandableMemory,result);
            if (CommonDialog.debugMessages) await context.PostAsync("Bag contents : " + handSetsBag.BuildStrRep());
            await UpdateUserAsync(context, handSetsLeft, handSetsNow);
        }

        [LuisIntent("FMRadio")]
        public async Task FMRadio(IDialogContext context, LuisResult result)
        {
            int handSetsLeft, handSetsNow = decoder.CurrentNumberofHandsetsLeft();

            await ShowDebugInfoAsync(context, result);

            await context.PostAsync("I understand that the most important thing for you is the presence of an FM Radio Antenna");
            handSetsLeft = decoder.DecodeIntent(EIntents.FMRadio, result);
            if (CommonDialog.debugMessages) await context.PostAsync("Bag contents : " + handSetsBag.BuildStrRep());
            await UpdateUserAsync(context, handSetsLeft, handSetsNow);
        }

        [LuisIntent("FaceID")]
        public async Task FaceID(IDialogContext context, LuisResult result)
        {
            int handSetsLeft, handSetsNow = decoder.CurrentNumberofHandsetsLeft();

            await ShowDebugInfoAsync(context, result);

            await context.PostAsync("I understand that the most important thing for you is the presence of Face ID recognition");

            handSetsLeft = decoder.DecodeIntent(EIntents.FaceID,result);
            if (CommonDialog.debugMessages) await context.PostAsync("Bag contents : " + handSetsBag.BuildStrRep());
            await UpdateUserAsync(context, handSetsLeft, handSetsNow);
        }

        [LuisIntent("GPS")]
        public async Task GPS(IDialogContext context, LuisResult result)
        {
            int handSetsLeft, handSetsNow = decoder.CurrentNumberofHandsetsLeft();

            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you is the presence of GPS");

            handSetsLeft = decoder.DecodeIntent(EIntents.GPS,result);
            if (CommonDialog.debugMessages) await context.PostAsync("Bag contents : " + handSetsBag.BuildStrRep());
            await UpdateUserAsync(context, handSetsLeft, handSetsNow);
        }


        [LuisIntent("HDVoice")]
        public async Task HDVoice(IDialogContext context, LuisResult result)
        {
            int handSetsLeft, handSetsNow = decoder.CurrentNumberofHandsetsLeft();

            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you is the presence of High Definition voice");
            handSetsLeft = decoder.DecodeIntent(EIntents.HDVoice,result);
            if (CommonDialog.debugMessages) await context.PostAsync("Bag contents : " + handSetsBag.BuildStrRep());
            await UpdateUserAsync(context, handSetsLeft, handSetsNow);
        }

        [LuisIntent("HighResDisplay")]
        public async Task HighResDisplay(IDialogContext context, LuisResult result)
        {
            int handSetsLeft, handSetsNow = decoder.CurrentNumberofHandsetsLeft();
            StringBuilder debug = new StringBuilder();

            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you is a High resolution display");
            handSetsLeft = decoder.DecodeIntent(EIntents.HighResDisplay, result,debug);
            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : DecodeIntent() output = " + debug.ToString());
            if (CommonDialog.debugMessages) await context.PostAsync("Bag contents : " + handSetsBag.BuildStrRep());
            await UpdateUserAsync(context, handSetsLeft, handSetsNow);
        }

        [LuisIntent("LargeStorage")]
        public async Task LargeStorage(IDialogContext context, LuisResult result)
        {
            int handSetsLeft, handSetsNow = decoder.CurrentNumberofHandsetsLeft();


            await ShowDebugInfoAsync(context, result);

            await context.PostAsync("I understand that the most important thing for you is a phone with a large storage capability");
            handSetsLeft = decoder.DecodeIntent(EIntents.LargeStorage, result);

            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : Bag contents: " + handSetsBag.BuildStrRep());

            await UpdateUserAsync(context, handSetsLeft, handSetsNow);
        }

        [LuisIntent("OS")]
        public async Task OS(IDialogContext context, LuisResult result)
        {
            int handSetsLeft, handsetsNow = decoder.CurrentNumberofHandsetsLeft();

            await ShowDebugInfoAsync(context, result);

            await context.PostAsync("I understand that the most important thing for you in a phone is the operating System");
            handSetsLeft = decoder.DecodeIntent(EIntents.OS, result);

            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : Bag contents : " + handSetsBag.BuildStrRep());

            await UpdateUserAsync(context, handSetsLeft, handsetsNow);
        }

        [LuisIntent("ScreenSize")]
        public async Task ScreenSize(IDialogContext context, LuisResult result)
        {
            int handSetsLeft, handSetsNow = decoder.CurrentNumberofHandsetsLeft();

            await ShowDebugInfoAsync(context, result);

            await context.PostAsync("I understand that the most important thing for you on a phone is the screen size");
            handSetsLeft = decoder.DecodeIntent(EIntents.ScreenSize, result);

            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : Bag ContentS : " + handSetsBag.BuildStrRep());

            await UpdateUserAsync(context, handSetsLeft, handSetsNow);
        }

        [LuisIntent("SecondaryCamera")]
        public async Task SecondaryCamera(IDialogContext context, LuisResult result)
        {
            int handSetsLeft, handSetsNow = decoder.CurrentNumberofHandsetsLeft();

            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you is the presence of a secondary camera");
            handSetsLeft = decoder.DecodeIntent(EIntents.SecondaryCamera,result);

            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : Bag contents : " + handSetsBag.BuildStrRep());     

            await UpdateUserAsync(context, handSetsLeft, handSetsNow);
        }

        [LuisIntent("Small")]
        public async Task Small(IDialogContext context, LuisResult result)         
        {
            int handSetsLeft, handSetsNow = decoder.CurrentNumberofHandsetsLeft();

            await ShowDebugInfoAsync(context, result); 

            await context.PostAsync("I understand that the most important thing for you are the dimensions of your new phone");
            handSetsLeft = decoder.DecodeIntent(EIntents.Small, result);

            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : Bag contents : " + handSetsBag.BuildStrRep());

            await UpdateUserAsync(context, handSetsLeft, handSetsNow);
        }

        [LuisIntent("WaterResist")]
        public async Task WaterResist(IDialogContext context, LuisResult result)
        {
            int handSetsLeft, handSetsNow = decoder.CurrentNumberofHandsetsLeft();
      
            await ShowDebugInfoAsync(context, result);

            await context.PostAsync("I understand that the most important thing for you is that your phone should be water resistant");
            handSetsLeft = decoder.DecodeIntent(EIntents.WaterResist,result);

            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : Bag contents : " + handSetsBag.BuildStrRep());

            await UpdateUserAsync(context, handSetsLeft, handSetsNow);
        }

        [LuisIntent("Weight")]
        public async Task Weight(IDialogContext context, LuisResult result)
        {
            int handSetsLeft = -1,handSetsNow = handSetsBag.BagCount();

            await ShowDebugInfoAsync(context, result);

            await context.PostAsync("I understand that the most important thing for you is the weight of your phone");
            handSetsLeft = decoder.DecodeIntent(EIntents.Weight, result);

            if (CommonDialog.debugMessages) await context.PostAsync($"DEBUG : I have here {handSetsLeft} equipments left, bag contents = {handSetsBag.BuildStrRep()}, bag count = {handSetsBag.BagCount()}");

            await UpdateUserAsync(context, handSetsLeft, handSetsNow);
        }

        [LuisIntent("WiFi")]
        public async Task WiFi(IDialogContext context, LuisResult result)
        {
            int handSetsLeft, handSetsNow = decoder.CurrentNumberofHandsetsLeft();

            await ShowDebugInfoAsync(context, result);

            await context.PostAsync("I understand that the most important thing for you is the presence of WiFi");
            handSetsLeft = decoder.DecodeIntent(EIntents.WiFi, result);

            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : Bag contents : " + handSetsBag.BuildStrRep());

            await UpdateUserAsync(context, handSetsLeft, handSetsNow);
        }

        [LuisIntent("Color")]
        public async Task Color(IDialogContext context,LuisResult result)
        {
            int handSetsLeft, handSetsNow = decoder.CurrentNumberofHandsetsLeft();

            await ShowDebugInfoAsync(context, result);

            await context.PostAsync("I understand that the most important thing for you is the color");
            handSetsLeft = decoder.DecodeIntent(EIntents.Color, result);

            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : Bag contents : " + handSetsBag.BuildStrRep());

            await UpdateUserAsync(context, handSetsLeft, handSetsNow);
        }

        [LuisIntent("Newest")]
        public async Task Newest(IDialogContext context , LuisResult result)
        {
            int handSetsLeft, handSetsNow = decoder.CurrentNumberofHandsetsLeft();

            await ShowDebugInfoAsync(context, result);

            await context.PostAsync("I understant that you want a recent model");
            handSetsLeft = decoder.DecodeIntent(EIntents.Newest, result);

            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : Bag contents : " + handSetsBag.BuildStrRep());

            await UpdateUserAsync(context, handSetsLeft, handSetsNow);
        }

        [LuisIntent("")]
        public async Task NoneAtAll(IDialogContext context,LuisResult result )
        {
            await ShowDebugInfoAsync(context, result);
        }

        string TyposWarning(LuisResult result)
        {
            string returnValue = null;

            if (result.AlteredQuery != null )
            {
                returnValue = $"You typed {result.Query}, did you mean {result.AlteredQuery} ?";
            }
            return returnValue;
        }



        private async Task ShowDebugInfoAsync (IDialogContext context,LuisResult luisResult)
        {
            IntentRecommendation topIntent;

            
            topIntent = luisResult.TopScoringIntent;
            if (CommonDialog.debugMessages) await context.PostAsync($"DEBUG : The most scored intent is {topIntent.Intent} with skore = {topIntent.Score}");
            if (CommonDialog.debugMessages) await context.PostAsync(GetEntityScores(luisResult));
        }

        private string GetEntityScores(LuisResult result)
        {
            StringBuilder sb = new StringBuilder("DEBUG: Entities detected:\r\n");

            foreach(var entity in result.Entities)
            {
                sb.Append($"Type = {entity.Type}\r\nEntity = {entity.Entity}\r\nSkore = {entity.Score}\r\n");
                sb.Append("Next one :\r\n");
            }
            sb.Append("No next one\r\n");
            return sb.ToString();
        }

        private async Task UpdateUserAsync(IDialogContext context,int handSetsLeft,int handSetsB4)
        {
            StringBuilder sb = new StringBuilder("-->");
            List<EIntents> ranking = this.decoder.IntentsRanking(sb);
        
            foreach (var intent in ranking)
                sb.Append(intent.ToString() + "\r\n");
            if (CommonDialog.debugMessages)
            {
                await context.PostAsync("DEBUG : Ranking : \r\n");
                await context.PostAsync(sb.ToString() + "\r\n");
            }
            if (handSetsLeft == handSetsB4)
                await context.PostAsync("Unfortunately, that doesn't help in narrowing the list down");
            else if (handSetsLeft == 0)
            {
                await context.PostAsync("I'm afraid that's a very high standard, I don't have any equipment that fulfills it.");
                handSetsLeft = handSetsB4;
            }
            else
                await context.PostAsync($"I narrowed it down to {handSetsLeft} handsets that fulfill your requirements");
            if ((numberOfIterations++ == 1) && (handSetsLeft > BotConstants.MAX_CAROUSEL_CARDS))
                await context.PostAsync("What else is important for you on a mobile?");
            else
            {
                if (CommonDialog.debugMessages) await context.PostAsync($"DEBUG : Number of phones on bag : {handSetsBag.BagCount()}");
                context.Done(decoder);
            }
        }
    }
}