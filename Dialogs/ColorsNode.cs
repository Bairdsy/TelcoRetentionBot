using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;

using MultiDialogsBot.Helper;
 

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
            string modelCapitalized = Miscellany.Capitalize(chosenModel);

            if (debugMessages) await context.PostAsync($"DEBUG : StartAsync() method in ColorsNode object, I received {chosenModel} model to present");

            colors = GetColors(chosenModel);
            if (colors.Count != 1)   
                PromptDialog.Choice(context, 
                                  ColorSelectionReceivedAsync,
                                  colors,
                                  $"OK. There are a few different options for you to pick for {modelCapitalized} from below:",
                                  "Sorry, not a valid option",
                                  4);
            else
            {
                await CongratulateSubsAsync(context);
            }
        }
         
      

        public async Task MessageReceivedAsync(IDialogContext context,IAwaitable<object> awaitable)
        {
            if (debugMessages) await context.PostAsync("DEBUG : ColorsNode object - End of dialog");
            await context.PostAsync("0 OK, 0:1");
            context.Done(0);
        } 

        private async Task CongratulateSubsAsync(IDialogContext context )
        {
            string phoneMatchMsg = "The phone match message will be inserted here";

            await context.PostAsync($"Excellent selection - The {chosenModel} is perfect for you because **{phoneMatchMsg}** . The next step is to work out what plan is the best for you");
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
           await context.PostAsync("O.K.  Now I need to take you through the terms and conditions to finalise your order."); 
             context.Wait(MessageReceivedAsync);
        }

        private async Task ColorSelectionReceivedAsync(IDialogContext context,IAwaitable<string> awaitable)
        {
            await CongratulateSubsAsync(context);
        }
    }

}