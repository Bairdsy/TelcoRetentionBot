namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class RootDialogCopy : IDialog<object>
    {
        private const string YesOption = "Yes - lets do it.";
        private const string BusyOption = "No - Im busy right now.  Lets do it later.";
        private const string NoOption = "No - Im not interested.";
        private const string CallBackOption = "No - Id rather talk to a person about it.";


        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            //(message.Text.All(Char.IsDigit)
            if (message.Text.ToLower().Contains("help") || message.Text.ToLower().Contains("support") || message.Text.ToLower().Contains("problem"))
            {
                await context.Forward(new SupportDialog(), this.ResumeAfterSupportDialog, message, CancellationToken.None);
            }
            else
            {
                this.ShowOptions(context);
            }
        }

        private void ShowOptions(IDialogContext context)
        {
            PromptDialog.Choice(context, this.OnOptionSelected, new List<string>() { YesOption, BusyOption, NoOption, CallBackOption }, "Hi there!  I'm Vicky from Vodafone, the Vodafone help bot.  I just thought I'd let you know that you are now eligible for an upgrade to a new phone.  We have some great new offers available that Ive personalised just for you.  Are you able to go through this with me now?", "Not a valid option", 3);
        }

        private async Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                switch (optionSelected)
                {
                    case YesOption:
                        context.Call(new Node2(), this.ResumeAfterOptionDialog);
                        break;

                    case BusyOption:
                        context.Call(new Node3(), this.ResumeAfterOptionDialog);
                        break;

                    case NoOption:
                        context.Call(new Node4(), this.ResumeAfterOptionDialog);
                        break;

                    case CallBackOption:
                        context.Call(new Node5(), this.ResumeAfterOptionDialog);
                        break;

                }
            }
            catch (TooManyAttemptsException ex)
            {
                await context.PostAsync($"Ooops! Too many attemps :(. But don't worry, I'm handling that exception and you can try again!");

                context.Wait(this.MessageReceivedAsync);
            }
        }

        private async Task ResumeAfterSupportDialog(IDialogContext context, IAwaitable<int> result)
        {
            var ticketNumber = await result;

            await context.PostAsync($"Thanks for contacting our support team. Your ticket number is {ticketNumber}.");
            context.Wait(this.MessageReceivedAsync);
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
                context.Wait(this.MessageReceivedAsync);
            }
        }
    }
}