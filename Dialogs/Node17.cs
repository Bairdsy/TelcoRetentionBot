namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class Node17 : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            PromptDialog.Choice(context, this.OptionSelected, new List<string>() { "It costs too much.", "I dont need that much stuff included.", "I need more stuff included.", "Everything about it.", "Oops - Actually that is fine."  }, "Okay - what are you not happy with?", "Not a valid option - please click on one of the response buttons above.", 5);
        }



        private async Task OptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                switch (optionSelected)
                {
                    case "It costs too much.":
                        context.Call(new Node22(), this.ResumeAfterOptionDialog);
                        break;

                    case "I dont need that much stuff included.":
                        context.Call(new Node23(), this.ResumeAfterOptionDialog);
                        break;

                    case "I need more stuff included.":
                        context.Call(new Node24(), this.ResumeAfterOptionDialog);
                        break;

                    case "Everything about it.":
                        context.Call(new Node25(), this.ResumeAfterOptionDialog);
                        break;

                    case "Oops - Actually that is fine.":
                        context.Call(new Node16(), this.ResumeAfterOptionDialog);
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