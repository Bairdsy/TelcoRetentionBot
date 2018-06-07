namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class Node9 : IDialog<object>
    {
        private const string Yes = "Yes";
        private const string No = "No";
        private const string yes = "yes";
        private const string no = "no";
        string HandsetMaker;
        string HandsetModel;
        string Handset;
        string HandsetImage;

        public async Task StartAsync(IDialogContext context)
        {
            var message = context.MakeMessage();

            context.ConversationData.TryGetValue("HandsetMakerKey", out HandsetMaker);
            context.ConversationData.TryGetValue("HandsetModelKey", out HandsetModel);
            context.ConversationData.TryGetValue("Handset", out Handset);
            context.ConversationData.TryGetValue("Handset_Image", out HandsetImage);

           // await context.PostAsync($"Image link is {HandsetImage}");
            var attachment = GetHandsetCard(Handset,HandsetImage);
            message.Attachments.Add(attachment);

            await context.PostAsync(message);
            context.Wait(this.PhoneSelection);

        }

        private static Attachment GetHandsetCard(string HSET, string IMAGE)
        {
            var heroCard = new HeroCard
            {
                Title = HSET,
                Subtitle = "Is this the handset you have right now?",
                Text = "My records show you currently have a " + HSET + ".  Is that correct?",
                Images = new List<CardImage> { new CardImage(IMAGE) },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.ImBack, "Yes", value: "Yes") , new CardAction(ActionTypes.ImBack, "No", value: "No") }
            };

            return heroCard.ToAttachment();
        }

        public virtual async Task PhoneSelection(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            var optionSelected = message.Text;
            try
            {
                switch (optionSelected)
                {
                    case Yes:
                        context.ConversationData.SetValue("HSet_Brand_Show", HandsetMaker);
                        context.Call(new Node12(), this.ResumeAfterOptionDialog);
                        break;

                    case No:
                        context.Call(new Node13(), this.ResumeAfterOptionDialog);
                        break;

                    case yes:
                        context.ConversationData.SetValue("HSet_Brand_Show", HandsetMaker);
                        context.Call(new Node12(), this.ResumeAfterOptionDialog);
                        break;

                    case no:
                        context.Call(new Node13(), this.ResumeAfterOptionDialog);
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
                context.Done(9);
            }
        }
    }
}


