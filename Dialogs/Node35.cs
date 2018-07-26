namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class Node35 : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            PromptDialog.Choice(context, this.OptionSelected, new List<string>() { "Accept", "Reject", "I need help"}, "Please follow the link above and read the Terms & Conditions for your plan, then come back here and click Accept.  ", "Not a valid option", 3);
            await context.PostAsync($"http://www.vodafone.ie/terms/bill-pay/index.jsp#redconnect"); 
        }

        private async Task OptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;
                switch (optionSelected)
                {
                    case "Accept":
                        context.Call(new Node37(), this.ResumeAfterOptionDialog);
                        break;

                    case "Reject":
                        context.Call(new Node38(), this.ResumeAfterOptionDialog);
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
                context.Done(2);
            }
        }
    }
}