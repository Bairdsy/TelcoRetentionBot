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
        public enum ENeeds
        {
            PictureLover = 0,
            MovieWatcher ,
            CommunicateInWritting,
            ComfortableToHold
        };

        double topEntitySkore;

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
            if (CommonDialog.debugMessages) await context.PostAsync("PictureLover Intent detected");
            context.Done(new Tuple<ENeeds,double>(ENeeds.PictureLover,topEntitySkore));
        }

        [LuisIntent("MovieWatcher")]
        public async Task MovieWatcher(IDialogContext context,LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            if (CommonDialog.debugMessages) await context.PostAsync("MovieWatcher intent detected");

            context.Done(new Tuple<ENeeds, double>(ENeeds.MovieWatcher, topEntitySkore));
        }

        [LuisIntent("ComfortableToHold")]
        public async Task ComfortableToHold(IDialogContext context,LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            if (CommonDialog.debugMessages) await context.PostAsync("Comfortable to hold intent detected");
            await context.PostAsync("ComfortableToHold");
            context.Done(new Tuple<ENeeds, double>(ENeeds.ComfortableToHold, topEntitySkore));
        }

        [LuisIntent("BrowseWeb")]
        public async Task BrowseWeb(IDialogContext context,LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("BrowseWeb");
        }

        [LuisIntent("CommunicateInWritting")]
        public async Task CommunicateInWritting(IDialogContext context,LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            if (CommonDialog.debugMessages) await context.PostAsync("Communicate in writting intent detected");
            await context.PostAsync("CommunicateInWritting");
            context.Done(new Tuple<ENeeds, double>(ENeeds.CommunicateInWritting, topEntitySkore));
        }

        [LuisIntent("PlayGames")]
        public async Task PlayGames(IDialogContext context,LuisResult result)
        {

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