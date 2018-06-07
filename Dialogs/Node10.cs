namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class Node10 : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync($"Based on your usage patterns, we'd recommend you take the following SIM Only plan:");

            var message = context.MakeMessage();
            var attachment = GetOfferCard(context);
            message.Attachments.Add(attachment);

            await context.PostAsync(message);
            context.Wait(this.OfferSelection);
        }


        private static Attachment GetOfferCard(IDialogContext context)
        {
            string offer_plan;
            string offer_rental;
            string offer_voice;
            string offer_data;
            string offer_text;

            context.ConversationData.TryGetValue("12M_OfferPlan", out offer_plan);
            context.ConversationData.TryGetValue("12M_OfferRent", out offer_rental);
            context.ConversationData.TryGetValue("12M_OfferVoice", out offer_voice);
            context.ConversationData.TryGetValue("12M_OfferData", out offer_data);
            context.ConversationData.TryGetValue("12M_OfferText", out offer_text);

            string offer_name = offer_plan;

            var items = new List<ReceiptItem> { };
            switch (offer_plan)
            {
                case "Red Connect SIM":
                    items.Add(new ReceiptItem("10GB Data"));
                    items.Add(new ReceiptItem("Unlimited calls and texts to any network"));
                    items.Add(new ReceiptItem("100 international minutes"));
                    items.Add(new ReceiptItem("100 international texts"));
                    items.Add(new ReceiptItem("Superfast 4G", image: new CardImage(url: "http://www.vodafone.co.uk/cs/groups/public/documents/webcontent/img_150x140_4g-icon.png")));
                    items.Add(new ReceiptItem("Roaming across Europe included"));
                    break;
                case "Red Connect SIM (Promo)":
                    items.Add(new ReceiptItem("10GB Data"));
                    items.Add(new ReceiptItem("Unlimited calls and texts to any network"));
                    items.Add(new ReceiptItem("100 international minutes"));
                    items.Add(new ReceiptItem("100 international texts"));
                    items.Add(new ReceiptItem("Superfast 4G", image: new CardImage(url: "http://www.vodafone.co.uk/cs/groups/public/documents/webcontent/img_150x140_4g-icon.png")));
                    items.Add(new ReceiptItem("Roaming across Europe included"));
                    break;
            }
            if (offer_voice.Length > 1)
            {
                offer_name = offer_name + " + " + offer_voice;
                items.Add(new ReceiptItem(offer_voice));
            }
            if (offer_data.Length > 1)
            {
                offer_name = offer_name + " + " + offer_data;
                items.Add(new ReceiptItem(offer_data));
            }
            if (offer_text.Length > 1)
            {
                offer_name = offer_name + " + " + offer_text;
                items.Add(new ReceiptItem(offer_text));
            }



            var offerCard = new ReceiptCard
            {
                Title = offer_name,
                Facts = new List<Fact> { new Fact("Monthly Rental", "€" + offer_rental), new Fact("Contract Length", "24 Months") },
                Items = items,
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.ImBack, "Great.  I'd like this plan.", value: "Accept Offer"),
                    new CardAction(ActionTypes.ImBack, "No thanks.  Please show me something else.", value: "Reject Offer")
                }
            };

            return offerCard.ToAttachment();
        }

        public virtual async Task OfferSelection(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            var optionSelected = message.Text;
            try
            {
                switch (optionSelected)
                {
                    case "Accept Offer":
                        context.Call(new Node16(), this.ResumeAfterOptionDialog);
                        break;

                    case "Reject Offer":
                        context.Call(new Node17(), this.ResumeAfterOptionDialog);
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