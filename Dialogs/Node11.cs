namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class Node11 : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            PromptDialog.Choice(context, this.OptionSelected, new List<string>() { "Sure.", "No - I'll just keep my current phone." }, "That's fine.  Why don't we look at what I can offer you with a new phone and if you don’t like that then I can show you the alternatives if you keep your current phone.  Does that sound OK?", "Not a valid option", 2);
        }

        private async Task OptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                switch (optionSelected)
                {
                    case "Sure":
                    case "OK":
                    case "okay":
                    case "Okay":
                    case "Ok":
                    case "ok":
                    case "Yes":
                    case "YES":
                    case "yes":
                    case "Sure.":
                        context.Call(new Node9(), this.ResumeAfterOptionDialog);
                        break;

                    case "NO":
                    case "No":
                    case "no":
                    case "No - I'll just keep my current phone.":
                        context.Call(new Node32(), this.ResumeAfterOptionDialog);
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