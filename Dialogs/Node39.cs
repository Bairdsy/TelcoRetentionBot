namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class Node39 : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            string phoneName;
            if (context.ConversationData.TryGetValue("SelectedPhone", out phoneName))
            {
                PromptDialog.Choice(context, this.OptionSelected, new List<string>() { "Confirm", "Reject" }, $"Your new {phoneName} will be sent to your current registered address.  The up-front cost will be charged to your account on your next BeYou bill day.", "Not a valid option", 3);
            }
            else
            {
                context.Call(new Node42(), this.ResumeAfterOptionDialog);
            }
        }

        private async Task OptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;
                switch (optionSelected)
                {
                    case "Confirm":
                        context.Call(new Node42(), this.ResumeAfterOptionDialog);
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
            context.Done(2);
        }
    }
}