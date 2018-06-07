namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using System.Linq;
    using FluentAssertions;
    using MongoDB.Bson;
    using MongoDB.Driver;

    [Serializable]
    public class Node12 : IDialog<object>
    {
        string manufacturer;

        protected static IMongoClient _client;
        protected static IMongoDatabase _database;
       // List<HeroCard> CardList;

        public async Task StartAsync(IDialogContext context)
        {
            var CardList = new List<HeroCard>();

            if (context.ConversationData.TryGetValue("HSet_Brand_Show", out manufacturer))
            {

            }
            else
            {
                context.ConversationData.TryGetValue("HandsetMakerKey", out manufacturer);

            }

            await context.PostAsync($"Okay.  Here are the latest models from {manufacturer}.  Click on one you might be interested in or let me know if you have something else in mind by typing 'other'.");

            _client = new MongoClient();
            _database = _client.GetDatabase("madcalm");

            var collection = _database.GetCollection<BsonDocument>("offer_handsets");
            //            var filter = Builders<BsonDocument>.Filter.Eq("Manufacturer", manufacturer); // new BsonDocument(); 
            var filter = Builders<BsonDocument>.Filter.Regex("Manufacturer", new BsonRegularExpression("/^" + manufacturer + "$/i"));

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

                        //await context.PostAsync($"Creating HSet HeroCard [{Name}] [{cost}] [{Image}] [{Description}]");

                        // process document
                        var Card = new HeroCard
                        {
                            Title = Name,
                            Subtitle = "From €" + cost,
                            Text = Description,
                            Images = new List<CardImage> { new CardImage("http://shop.vodafone.ie" + Image) },
                            Buttons = new List<CardAction> { new CardAction(ActionTypes.ImBack, "Choose this phone.", value: Name), new CardAction(ActionTypes.ImBack, "Show me other options.", value: "other") }
                        };
                        CardList.Add(Card);

                    }
                }
            }

            var reply = context.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = new List<Attachment>();
            foreach (HeroCard productCard in CardList)
            {
                reply.Attachments.Add(productCard.ToAttachment());
            }

            await context.PostAsync(reply);
            context.Wait(this.PhoneSelection);
        }

        public virtual async Task PhoneSelection(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            var optionSelected = message.Text;
            try
            {
                switch (optionSelected)
                {
                    case "other":
                        context.Call(new Node14(), this.ResumeAfterOptionDialog);
                        break;

                    case "Other":
                        context.Call(new Node14(), this.ResumeAfterOptionDialog);
                        break;

                    default:
                        context.ConversationData.SetValue("OfferHandset", optionSelected);
                        context.Call(new Node31(), this.ResumeAfterOptionDialog);
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