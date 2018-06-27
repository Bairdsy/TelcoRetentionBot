using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Threading.Tasks;
using System.Text;

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;


namespace MultiDialogsBot.Dialogs
{
    [LuisModel("bd88c74a-7bf4-4253-8abd-e808f0a83f19", "99127c285bd3420aa9d9f460091b7683", LuisApiVersion.V2,null,SpellCheck = true,Verbose =true)]
    [Serializable]
    public class NodeLuisSubsNeeds : LuisDialog<object>
    {
        static readonly Dictionary<ENeeds,string> responses;

        public enum ENeeds
        {
            PictureLover = 0,
            MovieWatcher ,
            ShowOff,
            GamesAddict,
            Camera /* This one is part of the other LUIS APP */
        };

        double topEntitySkore;

        static   NodeLuisSubsNeeds()
        {
            responses = new Dictionary<ENeeds, string>()
            {
                { ENeeds.PictureLover,"As you are a photo lover, you can upload your photos to the Cloud, like Google Photos or iCloud."},
                { ENeeds.MovieWatcher,"As you enjoy using your phone as TV, " },
                { ENeeds.ShowOff, "As you love to have the latest," },
                { ENeeds.GamesAddict,"As you are fond of playing with your phone," }
            };
        }

        public static string GetNeedIntentDesc(ENeeds needIntent)
        {
            return responses[needIntent];
        }

        [LuisIntent("None")]
        public async Task None(IDialogContext context,LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("Not understood");
        }

        [LuisIntent("PictureLover")]
        public async Task PictureLover(IDialogContext context,LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : PictureLover Intent detected");
            context.Done(new Tuple<ENeeds,double>(ENeeds.PictureLover,topEntitySkore));
        }

        [LuisIntent("MovieWatcher")]
        public async Task MovieWatcher(IDialogContext context,LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : MovieWatcher intent detected");

            context.Done(new Tuple<ENeeds, double>(ENeeds.MovieWatcher, topEntitySkore));
        }

        [LuisIntent("ShowOff")]
        public async Task ShowOff(IDialogContext context,LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : ShowOff intent detected");

            context.Done(new Tuple<ENeeds, double>(ENeeds.ShowOff, topEntitySkore));
        }
          
        [LuisIntent("GamesAddict")]
        public async Task GamesAddict(IDialogContext context,LuisResult result)
        {
            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : Beginning of GamesAddict() handler method");
            await ShowDebugInfoAsync(context, result);
            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : GamesAddict intent detected");

            context.Done(new Tuple<ENeeds, double>(ENeeds.GamesAddict, topEntitySkore));
        }

        private async Task ShowDebugInfoAsync(IDialogContext context, LuisResult luisResult)
        {
            IntentRecommendation topIntent;

            topEntitySkore = luisResult.TopScoringIntent.Score ?? 0;
            topIntent = luisResult.TopScoringIntent;
            if (CommonDialog.debugMessages) await context.PostAsync($"DEBUG : The most scored intent is {topIntent.Intent} with skore = {topIntent.Score}");
            if (CommonDialog.debugMessages) await context.PostAsync(GetEntityScores(luisResult));
        }

        private string GetEntityScores(LuisResult result)
        {
            StringBuilder sb = new StringBuilder("DEBUG: Entities detected:\r\n");

            foreach (var entity in result.Entities)
            {
                sb.Append($"Type = {entity.Type}\r\nEntity = {entity.Entity}\r\nSkore = {entity.Score}\r\n");
                sb.Append("Next one :\r\n");
            }
            sb.Append("No next one\r\n");
            return sb.ToString();
        }
    }
}