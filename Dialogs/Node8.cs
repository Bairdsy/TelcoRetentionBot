namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class Node8 :  IDialog<object>
    {
        private const string NewPhone       = "I want a new phone.";
        private const string CurrentPhone   = "I'm happy with my current phone.";
        private const string NotSure        = "I'm not sure.";

        public  async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync($"OK.  You have been successfully authenticated so lets get down to business.  :)");
            await context.PostAsync("So how can I help you today?");
            context.Call(new NodeLUISBegin( ), EnterLuis);
           // PromptDialog.Choice(context, this.PhoneSelection, new List<string>() { NewPhone, CurrentPhone, NotSure }, "Have you thought about whether you want to get a new phone or if you are happy with your current phone?", "Not a valid option", 3);
        }

        private async Task EnterLuis(IDialogContext context,IAwaitable<object> result)
        {  
            var ret = await result;

            await context.PostAsync("DEBUG : NodeLuisBegin returned : " + ret.ToString());
            if (((Tuple<string,NodeLUISBegin.EIntent>)ret).Item2 == NodeLUISBegin.EIntent.HandSet )
                context.Call(new NodePhoneFlow(((Tuple<string, NodeLUISBegin.EIntent>)ret).Item1),this.upgradeEquipmentFlow);
            else
                PromptDialog.Choice(context, this.PhoneSelection, new List<string>() { NewPhone, CurrentPhone, NotSure }, "Have you thought about whether you want to get a new phone or if you are happy with your current phone?", "Not a valid option", 3);
        }

        private async Task upgradeEquipmentFlow(IDialogContext context,IAwaitable<object> result)
        {
            PromptDialog.Choice(context, this.PhoneSelection, new List<string>() { NewPhone, CurrentPhone, NotSure }, "Have you thought about whether you want to get a new phone or if you are happy with your current phone?", "Not a valid option", 3);
        }


        private async Task PhoneSelection(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                
                string optionSelected =   await result;

                switch (optionSelected)
                {
                    case NewPhone:
                        context.Call(new Node9(), this.ResumeAfterOptionDialog);
                        break;

                    case CurrentPhone:
                        context.Call(new Node32(), this.ResumeAfterOptionDialog);
                        break;

                    case NotSure:
                        context.Call(new Node11(), this.ResumeAfterOptionDialog);
                        break;
                }
            }
            catch (TooManyAttemptsException ex)
            {
                await context.PostAsync($"Ooops! Too many attemps :(. But don't worry, I'm handling that exception and you can try again!");

                context.Done(0);
            }
        }

        private async Task ResumeAfterOptionDialog(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                var message = await result;
            }
            catch (Exception ex)
            {
                await context.PostAsync($"Failed with message: {ex.Message}");
            }
            finally
            {
                context.Done(0);
            }
        }
    }
}