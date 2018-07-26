namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class Node42 : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            
            var heroCard = new HeroCard
            {
                Title = "And we're done!",
                Subtitle = "Your new phone will be in the mail to your address today.",
                Text = "I hope you enjoy your new phone and have a wonderful rest of your day today.  Feel free to contact me with any issues or queries you might have.  Until then, thanks for your time.  Bye!",
                Images = new List<CardImage> { new CardImage("http://www.madcalm.com/wp-content/uploads/2018/06/MADCALM-QUESTION.png") },
                Buttons = new List<CardAction> { }
            };

            var message = context.MakeMessage();
            message.Attachments.Add(heroCard.ToAttachment());

            await context.PostAsync(message);
            context.Done(42);
        }
    }
}