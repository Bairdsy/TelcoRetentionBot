namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class Node41 : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            PromptDialog.Choice(context, this.OptionSelected, new List<string>() { "Confirm", "Reject" }, $"At the end of the term, your Agreement will continue to run on a month to month basis, unless you, the Customer, provide notice of your desire to terminate this agreement. ", "Not a valid option", 3);
        }

        private async Task OptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;
                switch (optionSelected)
                {
                    case "Confirm":
                        context.Call(new Node39(), this.ResumeAfterOptionDialog);
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

                    case "I need help":
                        context.Call(new Node5(), this.ResumeAfterOptionDialog);
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
            context.Done(2);
        }
    }
}