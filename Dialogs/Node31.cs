namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using System.Linq;
    using FluentAssertions;
    using MongoDB.Bson;
    using MongoDB.Driver;

    [Serializable]
    public class Node31 : IDialog<object>
    {
        private const string Bills = "RECENT_BILLS";

        protected static IMongoClient _client;
        protected static IMongoDatabase _database;
 

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync($"Great.  I have analysed the way you use your phone and am matching that to the offers we have available to find the very best fit for you.  In the meantime, here is some analysis of your usage over the last 3 months.");
            await Task.Delay(3000);
            //await context.PostAsync($"calling ShowUsageCards");
            await this.ShowUsageCards(context);
            context.Wait(this.Finished);

        }

        protected async Task ShowUsageCards(IDialogContext context)
        {
            int subsno;

            var reply = context.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = new List<Attachment>();

            var CardList = new List<HeroCard>();
            //await context.PostAsync($"Connecting DB");
            _client = new MongoClient();
            _database = _client.GetDatabase("madcalm");
            //await context.PostAsync($"Connected");
            context.ConversationData.TryGetValue("SubsNumber", out subsno);

            var collection = _database.GetCollection<BsonDocument>("images");
            var filter = Builders<BsonDocument>.Filter.Eq("Anon_Subsno", subsno); // new BsonDocument(); 
            var count = 0;
            //await context.PostAsync($"Querying collection [{subsno}]");
            using (var cursor = await collection.FindAsync(filter))
            {
                while (await cursor.MoveNextAsync())
                {
                    var batch = cursor.Current;
                    foreach (var document in batch)
                    {
                        // process document
                        count++;
                        //await context.PostAsync($"Processing record {count}");

                        var name = document.GetElement("Name").Value;
                        var image = document.GetElement("Image").Value;

                        //await context.PostAsync($"Read name[{name}] and image[{image}]");
                        var title = name;
                        var subtitle = name;
                        var text = name;
                        switch ((string)name)
                        {
                            case "RECENT_BILLS":
                                title = "Recent Bills";
                                subtitle = "Here is a summary of your 3 most recent bills.";
                                text = "Here is a summary of your 3 most recent bills.";
                                break;
                            case "VOICE":
                                title = "Voice Usage";
                                subtitle = "Voice calls made over the last 3 months.";
                                text = "Voice calls made over the last 3 months.";
                                break;
                            case "TEXT":
                                title = "Text Usage";
                                subtitle = "SMS sent over the last 3 months.";
                                text = "SMS sent over the last 3 months.";
                                break;
                            case "DATA":
                                title = "Data Usage";
                                subtitle = "Data usage over the last 3 months.";
                                text = "Data usage over the last 3 months.";
                                break;
                            case "INT_VOICE":
                                title = "International Voice Usage";
                                subtitle = "International voice calls made over the last 3 months.";
                                text = "International voice calls made over the last 3 months.";
                                break;
                            case "INT_TEXT":
                                title = "International Text Usage";
                                subtitle = "International SMS sent over the last 3 months.";
                                text = "International SMS sent over the last 3 months.";
                                break;
                            case "INTL_MAP":
                                title = "International Summary";
                                subtitle = "These are the countries you have called or texted over the last 3 months.";
                                //context.ConversationData.TryGetValue("International_Desc", out text);
                                //await context.PostAsync($"Read international desc [{text}]");
                                break;
                            case "ROAMING_MAP":
                                title = "Roaming Summary";
                                subtitle = "These are the roaming countries where you have used your phone over the last 3 months.";
                                text = "These are the roaming countries where you have used your phone over the last 3 months.";
                                break;

                        }

                        var Card = new HeroCard
                        {
                            Title = (string)title,
                            Subtitle = (string)subtitle,
                            Text = (string)text,
                            Images = new List<CardImage> { new CardImage((string)image) },
                            Buttons = new List<CardAction> { new CardAction(ActionTypes.ImBack, "Click here when finished.", value: "Done") }
                        };
                        CardList.Add(Card);
                    }
                }
            }
             
            foreach (HeroCard Card in CardList)
            {
                reply.Attachments.Add(Card.ToAttachment());
            }

            await context.PostAsync(reply);

        }


 /*       private async Task get_Intl_Text(IDialogContext context, ref string text)
        {
            int subsno;
            context.ConversationData.TryGetValue("SubsNumber", out subsno);

            text = "";
            var collection = _database.GetCollection<BsonDocument>("international");
            var filter = Builders<BsonDocument>.Filter.Eq("Subscriber", subsno); // new BsonDocument(); 
            var count = 0;
            //await context.PostAsync($"Querying collection [{subsno}]");
            using (var cursor = await collection.FindAsync(filter))
            {
                while (await cursor.MoveNextAsync())
                {
                    var batch = cursor.Current;
                    foreach (var document in batch)
                    {
                        // process document
                        count++;
                        //await context.PostAsync($"Processing record {count}");

                        var country = document.GetElement("Country").Value;
                        var min_1 = document.GetElement("M1_Minutes").Value;
                        var sms_1 = document.GetElement("M1_Texts").Value;
                        var min_2 = document.GetElement("M2_Minutes").Value;
                        var sms_2 = document.GetElement("M2_Texts").Value;
                        var min_3 = document.GetElement("M3_Minutes").Value;
                        var sms_3 = document.GetElement("M3_Texts").Value;

                        if (count > 1)
                        {
                            text = text + ", ";
                        }
                        int mins = min_1 + min_2 + min_3;
                        int txts = sms_1 + sms_2 + sms_3;
                        text = text + "{country} ({mins} mins, {txts} SMS)";
                    }
                }
            }
        }*/
                        
                        
        public virtual async Task Finished(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            var optionSelected = message.Text;
            try
            {
                switch (optionSelected)
                {
                    case "done":
                    case "Done":
                        context.Call(new Node15(), this.ResumeAfterOptionDialog);
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
                context.Done(0);
            }
        }
    }
}