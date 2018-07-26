namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class Terms1 : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            PromptDialog.Choice(context, this.OptionSelected, new List<string>() { "Accept", "Reject"}, "Please follow the link above and read the Terms & Conditions for your plan, then come back here and click Accept.  ", "Not a valid option", 3);
            await context.PostAsync($"http://madcalm.com/beyou-terms-conditions/"); 
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
                        var Card = new HeroCard
                        {
                            Title = "That's OK.",
                            Text = "If you have changed your mind then I will save this order so you can access it later, or just start again next time you want to chat.",
                            Images = new List<CardImage> { new CardImage("http://www.madcalm.com/wp-content/uploads/2018/06/MADCALM-PROCESSING.png") },
                        };
                        var msg = context.MakeMessage();
                        msg.Attachments.Add(Card.ToAttachment());

                        await context.PostAsync(msg);
                        context.Done(2);
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