﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

using MultiDialogsBot.Helper;
using MultiDialogsBot.Database;


namespace MultiDialogsBot.Dialogs
{
    [LuisModel("f245439f-5379-464a-8481-e68985e4504b", "99127c285bd3420aa9d9f460091b7683", LuisApiVersion.V2, null, SpellCheck = true, Verbose = true)]
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
            Newest,
            RAM
        }

        string brandDesired;
        DateTime? ReleaseDateCurrentModel;
        IntentDecoder decoder;
        HandSets handSetsBag;
        TopFeatures topButtons;
        int numberOfIterations = 1;

        EIntents desiredFeature;
        double desiredFeatureScore;
        LuisResult res;

        public NodeLUISPhoneDialog(TopFeatures mostDemanded,HandSets handSets, string brand, DateTime? currentModelReleaseDate,List<string> narrowedListOfModels) : base()
        {
            handSetsBag = handSets;
            brandDesired = brand;
            ReleaseDateCurrentModel = currentModelReleaseDate;
            decoder = mostDemanded.AssociatedDecoder; //new IntentDecoder(handSets, brand, currentModelReleaseDate,narrowedListOfModels);
            topButtons = mostDemanded;
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
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that you want a phone with access to internet and with wide bandwidth");
            desiredFeature = EIntents.BandWidth;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("BatteryLife")]
        public async Task BatteryLife(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand you want a big battery life");
            desiredFeature = EIntents.BatteryLife;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("Brand")]
        public async Task Brand(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you is brand");
            desiredFeature = EIntents.Brand;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("Camera")]
        public async Task Camera(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you is the presence of a camera");
            desiredFeature = EIntents.Camera;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("Cheap")]
        public async Task Cheap(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
        }

        [LuisIntent("DualCamera")]
        public async Task DualCamera(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that you want a phone with Dual Camera");
            desiredFeature = EIntents.DualCamera;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("DualSIM")]
        public async Task DualSIM(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I undertand that you would like a phone with DualSIM");
            desiredFeature = EIntents.DualSIM;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("ExpandableMemory")]
        public async Task ExpandableMemory(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that an expandable memory is the most important thing for you");
            desiredFeature = EIntents.ExpandableMemory;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("FMRadio")]
        public async Task FMRadio(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you is the presence of an FM Radio Antenna");
            desiredFeature = EIntents.FMRadio;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("FaceID")]
        public async Task FaceID(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you is the presence of Face ID recognition");
            desiredFeature = EIntents.FaceID;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("GPS")]
        public async Task GPS(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you is the presence of GPS");
            desiredFeature = EIntents.GPS;
            await ProcessNeedOrFeatureAsync(context, result);
        }


        [LuisIntent("HDVoice")]
        public async Task HDVoice(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you is the presence of High Definition voice");
            desiredFeature = EIntents.HDVoice;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("HighResDisplay")]
        public async Task HighResDisplay(IDialogContext context, LuisResult result)
        { 
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you is a High resolution display");
            desiredFeature = EIntents.HighResDisplay;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("LargeStorage")]
        public async Task LargeStorage(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you is a phone with a large storage capability");
            desiredFeature = EIntents.LargeStorage;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("OS")]
        public async Task OS(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you in a phone is the operating System");
            desiredFeature = EIntents.OS;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("ScreenSize")]
        public async Task ScreenSize(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you on a phone is the screen size");
            desiredFeature = EIntents.ScreenSize;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("SecondaryCamera")]
        public async Task SecondaryCamera(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you is the presence of a secondary camera");
            desiredFeature = EIntents.SecondaryCamera;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("Small")]
        public async Task Small(IDialogContext context, LuisResult result)         
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you are the dimensions of your new phone");
            desiredFeature = EIntents.Small;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("WaterResist")]
        public async Task WaterResist(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you is that your phone should be water resistant");
            desiredFeature = EIntents.WaterResist;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("Weight")]
        public async Task Weight(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you is the weight of your phone");
            desiredFeature = EIntents.Weight;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("WiFi")]
        public async Task WiFi(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you is the presence of WiFi");
            desiredFeature = EIntents.WiFi;
            await ProcessNeedOrFeatureAsync(context, result);
        }
        /************* Daqui para baixo nao foi testado ******************/
        [LuisIntent("Color")]
        public async Task Color(IDialogContext context,LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you is the color");
            desiredFeature = EIntents.Color;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("Newest")]
        public async Task Newest(IDialogContext context , LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understant that you want a recent model");
            desiredFeature = EIntents.Newest;
            await ProcessNeedOrFeatureAsync(context, result);
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
            desiredFeatureScore = topIntent.Score ?? 0;
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

        private async Task ProcessNeedOrFeatureAsync(IDialogContext context, LuisResult luisResult)
        {
            var msg = context.MakeMessage();
            string text = luisResult.AlteredQuery != null ? luisResult.AlteredQuery : luisResult.Query;

            res = luisResult;
            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : Beginning of ProcessNeedOrFeatureAsync() method");
            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : Text received = " + text);
            msg.Text = text;
            await context.Forward(new NodeLuisSubsNeeds(), ProcessLuisNeedsResult, msg, CancellationToken.None);
        }

        private async Task UpdateUserAsync(IDialogContext context,int handSetsLeft,int handSetsB4)
        {
            StringBuilder sb = new StringBuilder("-->");
            List<EIntents> ranking = this.decoder.IntentsRanking(sb);
            var reply = ((Activity)context.Activity).CreateReply("What else is important for you on a mobile?");
          
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
            {
                sb = new StringBuilder("");
                reply.SuggestedActions = topButtons.GetTop4Buttons(sb);
                await context.PostAsync(reply);
            }
            else
            {
                if (CommonDialog.debugMessages) await context.PostAsync($"DEBUG : Number of phones on bag : {handSetsBag.BagCount()}");
                context.Done(decoder);
            }
        }

        public async Task ProcessLuisNeedsResult(IDialogContext context,IAwaitable<object> awaitable)
        {
            Tuple<NodeLuisSubsNeeds.ENeeds, double> result = (Tuple<NodeLuisSubsNeeds.ENeeds,double>) await awaitable;
            double needsScore = result.Item2;
            NodeLuisSubsNeeds.ENeeds needsIntent = result.Item1;
            int handSetsLeft,handSetsNow = decoder.CurrentNumberofHandsetsLeft();

            if (needsScore > desiredFeatureScore)  // WE have a need 
            {
                if (CommonDialog.debugMessages) await context.PostAsync("It's a need, namely " + needsIntent.ToString());
                if (needsIntent == NodeLuisSubsNeeds.ENeeds.MovieWatcher)
                {
                    await context.PostAsync("Scores : \r\n" + Scores());
                }
            }
            else
            {
                try
                {
                    handSetsLeft = decoder.DecodeIntent(desiredFeature, res);
                    if (CommonDialog.debugMessages) await context.PostAsync($"DEBUG : I have here {handSetsLeft} equipments left, bag contents = {handSetsBag.BuildStrRep()}, bag count = {handSetsBag.BagCount()}");
                    await UpdateUserAsync(context, handSetsLeft, handSetsNow);
                }
                catch (ArgumentException)
                {
                    await context.PostAsync("Argument xception");
                }
                catch (Exception xception)
                {
                    await context.PostAsync($"Error...Exception Message = {xception.Message}");
                }
            }
        }

        private string Scores()
        {
            StringBuilder sb = new StringBuilder();
            List<string> basket;
            ScoreFuncs funcs = new ScoreFuncs(handSetsBag);

            basket = handSetsBag.GetBagModels();
            foreach (var model in basket)
                sb.Append($"{model} ==> score = {funcs.PictureLover(model)}\r\n");
            return sb.ToString();
        }

    }
}