using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;


 

namespace MultiDialogsBot.Dialogs      
{
    [Serializable]        
    public class ColorsNode : CommonDialog    
    {
        string chosenModel;

          
        public ColorsNode(string model)
        {
            chosenModel = model;
        }
              
        public override async Task StartAsync(IDialogContext context) 
        {
            List<string> colors;
            

            if (debugMessages) await context.PostAsync($"DEBUG : StartAsync() method in ColorsNode object, I received {chosenModel} model to present");

            colors = GetColors(chosenModel);
            if (colors.Count != 1)   // This way works well because context.Wait(MessageReceivedAsync) is commented out
                PromptDialog.Choice(context, 
                                  ColorSelectionReceivedAsync,
                                  colors,
                                  "OK. There are a few different options for you to pick for that phone from below:",
                                  "Sorry, not a valid option",
                                  4);
            else
            {
                await CongratulateSubsAsync(context);
                // context.Wait(MessageReceivedAsync);   <----------- Needs to be commented out as well
            }
        }
         
      

        public async Task MessageReceivedAsync(IDialogContext context,IAwaitable<object> awaitable)
        {
            if (debugMessages) await context.PostAsync("DEBUG : ColorsNode object - End of dialog");
            await context.PostAsync("0 OK, 0:1");
            // context.Wait(MessageReceivedAsync);
            context.Done(0);
        }

        private async Task CongratulateSubsAsync(IDialogContext context )
        {
            string phoneMatchMsg = "The phone match message will be inserted here";

            await context.PostAsync($"Great Choice - The {chosenModel} is perfect for you because **{phoneMatchMsg}** . Now we need to work out what plan you should be on");
            context.ConversationData.SetValue("SelectedPhone", chosenModel);
            //Ryans flow kicks in
            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : I'm going to call Ryan's node");
            try
            {
                context.Call(new PlanNode(),PlanFlowDone);
            }
            catch (Exception xception)   
            {
                await context.PostAsync("Error...xception message = " + xception.ToString());
            }
        }

        private async Task PlanFlowDone(IDialogContext context, IAwaitable<object> result)
        {
            await context.PostAsync("End of plan Flow - enter something"); 
            context.Wait(MessageReceivedAsync);
        }

        private async Task ColorSelectionReceivedAsync(IDialogContext context,IAwaitable<string> awaitable)
        {
            await CongratulateSubsAsync(context);
        }
    }

}