namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class Node117 : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            PromptDialog.Choice(context, this.OptionSelected, new List<string>() { "The cost of the phone.", "The monthly cost of the plan.", "All of it.", "I've changed my mind.  I want a different phone." }, "Okay - what part are you not happy with?", "Not a valid option - please click on one of the response buttons above.", 4);
        }

        private async Task OptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                switch (optionSelected)
                {
                    case "The cost of the phone.":
                        context.Call(new Node18(), this.ResumeAfterOptionDialog);
                        break;

                    case "The monthly cost of the plan.":
                        context.Call(new Node31(), this.ResumeAfterOptionDialog);
                        break;

                    case "All of it.":
                        context.Call(new Node31(), this.ResumeAfterOptionDialog);
                        break;

                    case "I've changed my mind.  I want a different phone.":
                        context.Call(new Node31(), this.ResumeAfterOptionDialog);
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