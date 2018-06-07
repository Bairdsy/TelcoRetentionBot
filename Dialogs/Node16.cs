namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using System.Linq;
    using FluentAssertions;
    using MongoDB.Bson;
    using MongoDB.Driver;

    [Serializable]
    public class Node16 : IDialog<object>
    {
        protected static IMongoClient _client;
        protected static IMongoDatabase _database;

        public async Task StartAsync(IDialogContext context)
        {
            string offer_plan;
            string offer_rental;
            string offer_voice;
            string offer_data;
            string offer_text;
            string offer_handset;

            context.ConversationData.TryGetValue("FBP_OfferPlan", out offer_plan);
            context.ConversationData.TryGetValue("FBP_OfferRent", out offer_rental);
            context.ConversationData.TryGetValue("FBP_OfferVoice", out offer_voice);
            context.ConversationData.TryGetValue("FBP_OfferData", out offer_data);
            context.ConversationData.TryGetValue("FBP_OfferText", out offer_text);
            context.ConversationData.TryGetValue("OfferHandset", out offer_handset);

            string offer_name = offer_plan;

            await context.PostAsync($"Great.  So here is the plan and phone you selected, along with the final costs.  Would you like to continue with the upgrade?");

            var items = new List<ReceiptItem> { };

            _client = new MongoClient();
            _database = _client.GetDatabase("madcalm");

            var collection = _database.GetCollection<BsonDocument>("offer_handsets");
            var filter = Builders<BsonDocument>.Filter.Eq("Name", offer_handset); 

            using (var cursor = await collection.FindAsync(filter))
            {
                while (await cursor.MoveNextAsync())
                {
                    var batch = cursor.Current;
                    foreach (var document in batch)
                    {
                        string Name = (string)document.GetElement("Name").Value;
                        var Cost = document.GetElement("Cost").Value;
                        string cost = string.Format("{0:00}", Cost);
                        string Description = (string)document.GetElement("Description").Value;
                        string Image = (string)document.GetElement("Image").Value;

                        items.Add(new ReceiptItem(offer_handset, price: "€" + cost, image: new CardImage("http:/shop.vodafone.ie" + Image)));
                    }
                }
            }


            switch (offer_plan)
            {
                case "Red Connect Essentials":
                    items.Add(new ReceiptItem("5GB Data"));
                    items.Add(new ReceiptItem("Unlimited any network texts"));
                    items.Add(new ReceiptItem("100 any network minutes"));
                    items.Add(new ReceiptItem("Superfast 4G", image: new CardImage(url: "http://www.vodafone.co.uk/cs/groups/public/documents/webcontent/img_150x140_4g-icon.png")));
                    items.Add(new ReceiptItem("Roaming across Europe included"));
                    items.Add(new ReceiptItem("25GB Dropbox space with Backup+"));
                    items.Add(new ReceiptItem("12 months Secure Net"));
                    items.Add(new ReceiptItem("RED Entertainment pack", image: new CardImage(url: "http://www.vodafone.ie/image/BAU024270.img")));
                    break;
                case "Red Connect":
                    items.Add(new ReceiptItem("15GB Data"));
                    items.Add(new ReceiptItem("Unlimited calls and texts to any network"));
                    items.Add(new ReceiptItem("Superfast 4G", image: new CardImage(url: "http://www.vodafone.co.uk/cs/groups/public/documents/webcontent/img_150x140_4g-icon.png")));
                    items.Add(new ReceiptItem("100 international minutes and texts"));
                    items.Add(new ReceiptItem("Roaming across Europe included"));
                    items.Add(new ReceiptItem("25GB Dropbox space with Backup+"));
                    items.Add(new ReceiptItem("12 months Secure Net"));
                    items.Add(new ReceiptItem("RED Entertainment pack", image: new CardImage(url: "http://www.vodafone.ie/image/BAU024270.img")));
                    break;
                case "Red Connect Super":

                    items.Add(new ReceiptItem("30GB Data"));
                    items.Add(new ReceiptItem("Unlimited calls and texts to any network"));
                    items.Add(new ReceiptItem("Superfast 4G", image: new CardImage(url: "http://www.vodafone.co.uk/cs/groups/public/documents/webcontent/img_150x140_4g-icon.png")));
                    items.Add(new ReceiptItem("500 international minutes and texts"));
                    items.Add(new ReceiptItem("Roaming across Europe included"));
                    items.Add(new ReceiptItem("25GB Dropbox space with Backup+"));
                    items.Add(new ReceiptItem("12 months Secure Net"));
                    items.Add(new ReceiptItem("RED Entertainment pack", image: new CardImage(url: "http://www.vodafone.ie/image/BAU024270.img")));
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
                    new CardAction(ActionTypes.ImBack, "Yes.  That's correct.", value: "Accept Offer"),
                    new CardAction(ActionTypes.ImBack, "No thanks.", value: "Reject Offer")
                }
            };

            var attachment = offerCard.ToAttachment();
            var message = context.MakeMessage();
            message.Attachments.Add(attachment);
            await context.PostAsync(message);
            context.Wait(this.AcceptOffer);
        }

        public virtual async Task AcceptOffer(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            var optionSelected = message.Text;
            try
            {
                switch (optionSelected)
                {
                    case "Accept Offer":
                        context.Call(new Node35(), this.ResumeAfterOptionDialog);
                        break;

                    case "Reject Offer":
                        context.Call(new Node39(), this.ResumeAfterOptionDialog);
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