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
    [LuisModel("bd88c74a-7bf4-4253-8abd-e808f0a83f19", "a3a20fb04cad4cfcaf7b821bd1eb9a19",LuisApiVersion.V2,null,SpellCheck = true,Verbose =true)]
    [Serializable]
    public class NodeLuisSubsNeedscs : LuisDialog<object>
    {
        bool debugMessages;

        [LuisIntent("None")]
        public async Task None(IDialogContext context,LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("Not understood");
        }

        [LuisIntent("ComfortableToHold")]
        public async Task ComfortableToHold(IDialogContext context,LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("ComfortableToHold");
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
            await context.PostAsync("CommunicateInWritting");
        }

        [LuisIntent("PlayGames")]
        public async Task PlayGames(IDialogContext context,LuisResult result)
        {

        }

        private async Task ShowDebugInfoAsync(IDialogContext context, LuisResult luisResult)
        {
            IntentRecommendation topIntent;


            topIntent = luisResult.TopScoringIntent;
            if (debugMessages) await context.PostAsync($"DEBUG : The most scored intent is {topIntent.Intent} with skore = {topIntent.Score}");
            if (debugMessages) await context.PostAsync(GetEntityScores(luisResult));
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