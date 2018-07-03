namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using com.esendex.sdk.messaging;
    using MongoDB.Bson;
    using MongoDB.Driver;

    [Serializable]
    public class PlanNode : IDialog<object>
    {
        string subsno;
        protected static IMongoClient _client;
        protected static IMongoDatabase _database;
        int failCount;

        public async Task StartAsync(IDialogContext context)
        {
            failCount = 0;
            if (CommonDialog.debugMessages) await context.PostAsync($"DEBUG: entering node2");

            if (context.ConversationData.TryGetValue("SubsNumber", out subsno))
            {
                //await context.PostAsync($"DEBUG: Subs Number is {subsno}");
                int subsnum = Int32.Parse(subsno);

                _client = new MongoClient("mongodb://telcoretentiondb:HsQmjXjc0FBMrWYbJr8eUsGdWoTuaYXvdO2PRj13sxoPYijxxcxG5oSDfhFtVFWAFeWxFbuyf1NbxnFREFssAw==@telcoretentiondb.documents.azure.com:10255/?ssl=true&replicaSet=globaldb");
                _database = _client.GetDatabase("madcalm");


                await context.PostAsync($"I have analysed the way you use your phone and am matching that to the offers we have available to find the very best fit for you.");
                await Task.Delay(3000);
                await this.ShowSummaryCardAsync(context);
                await Task.Delay(3000);

                string generic_msg;
                var CardList = new List<HeroCard>();

                var collection = _database.GetCollection<BsonDocument>("offer_messages");
                var filter = Builders<BsonDocument>.Filter.Eq("Anon Subsno", subsnum);
                var sort = Builders<BsonDocument>.Sort.Ascending("Inbound Eligibility").Ascending("Value Rank");
                
                using (var cursor = await collection.FindAsync(filter, new FindOptions<BsonDocument, BsonDocument>()
                {
                    Sort = sort
                }))
                {
                    while (await cursor.MoveNextAsync())
                    {
                        var batch = cursor.Current;
                        foreach (var document in batch)
                        {
                            generic_msg = (string)document.GetElement("Generic Usage Message").Value;
                            await context.PostAsync($"**{generic_msg}**");
                            await Task.Delay(5000);
                            break;
                        }
                    }
                }
                await this.ShowPlanCarouselAsync(context);

            }
            else
            {
                await context.PostAsync($"Hmmm.  Seems I couldnt get the subscriber number.");
            }
        }


        protected async Task ShowPlanCarouselAsync(IDialogContext context)
        {
            int subsno;

            var reply = context.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = new List<Attachment>();

            var CardList = new List<HeroCard>();
            context.ConversationData.TryGetValue("SubsNumber", out subsno);

            var collection = _database.GetCollection<BsonDocument>("offer_messages");
            var filter = Builders<BsonDocument>.Filter.Eq("Anon Subsno", subsno);
            var sort = Builders<BsonDocument>.Sort.Ascending("Inbound Eligibility").Ascending("Value Rank");
            int count = 0;

            using (var cursor = await collection.FindAsync(filter, new FindOptions<BsonDocument, BsonDocument>()
            {
                Sort = sort
            }))
            {
                while (await cursor.MoveNextAsync())
                {
                    var batch = cursor.Current;
                    foreach (var document in batch)
                    {
                        string Name = (string)document.GetElement("Offer Name").Value;
                        string Image = (string)document.GetElement("Image Name").Value;
                        string Highlight = (string)document.GetElement("Plan Highlight").Value;
                        string Warning = (string)document.GetElement("Plan Warning").Value;
                        string Message = (string)document.GetElement("Plan Choice Message").Value;
                        string Code = (string)document.GetElement("Result Type Code").Value;

                        //await context.PostAsync($"DEBUG: Document [{Name}][{Image}][{Code}]");

                        if (count == 0)
                        {
                            Name = "*Recommended for you* - " + Name;
                            await context.PostAsync($"I've ranked all of your options from best to worst in the list below.");
                        }
                        var Card = new HeroCard
                        {
                            Title = Name,
                            Subtitle = Highlight,
                            Text = Warning,
                            Images = new List<CardImage> { new CardImage("http://www.madcalm.com/wp-content/uploads/2018/07/" + Image + ".png") },
                            Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Pick Me!", value: "Choose " + Code), new CardAction(ActionTypes.ImBack, "Show Analysis", value: "Analyse") }
                        };
                        CardList.Add(Card);
                        count++;
                    }
                }
            }
            
            foreach (HeroCard productCard in CardList)
            {
                reply.Attachments.Add(productCard.ToAttachment());
            }

            await context.PostAsync(reply);
            context.Wait(this.ChosenPlan);

        }

        public virtual async Task ChosenPlan(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            var optionSelected = message.Text;
            try
            {
                StringComparison comparison = StringComparison.InvariantCulture;
                if (optionSelected.StartsWith("Choose", comparison))
                {
                    string resCode = optionSelected.Substring(optionSelected.IndexOf(' ') + 1);
                    //await context.PostAsync($"You chose {resCode}.");
                    int subsnum = Int32.Parse(subsno);

                    var collection = _database.GetCollection<BsonDocument>("offer_messages");
                    var filter = Builders<BsonDocument>.Filter.Eq("Anon Subsno", subsnum) & Builders<BsonDocument>.Filter.Eq("Result Type Code", resCode);

                    using (var cursor = await collection.FindAsync(filter))
                    {
                        while (await cursor.MoveNextAsync())
                        {
                            var batch = cursor.Current;
                            foreach (var document in batch)
                            {
                                string Name = (string)document.GetElement("Offer Name").Value;
                                string Image = (string)document.GetElement("Image Name").Value;
                                string Highlight = (string)document.GetElement("Plan Highlight").Value;
                                string Warning = (string)document.GetElement("Plan Warning").Value;
                                string Message = (string)document.GetElement("Plan Choice Message").Value;

                                context.ConversationData.SetValue("ChosenPlanName", Name);
                                context.ConversationData.SetValue("ChosenPlanImage", "http://www.madcalm.com/wp-content/uploads/2018/07/" + Image + ".png");
                                context.ConversationData.SetValue("ChosenPlanHighlight", Highlight);
                                context.ConversationData.SetValue("ChosenPlanWarning", Warning);


                                StringComparison checkForWarn = StringComparison.InvariantCulture;
                                if (Message.StartsWith("Im not", checkForWarn))
                                {
                                    var Card = new HeroCard
                                    {
                                        Title = "Are you sure?",
                                        Text = Message,
                                        Images = new List<CardImage> { new CardImage("http://www.madcalm.com/wp-content/uploads/2018/06/MADCALM-CONFUSED.png") },
                                        Buttons = new List<CardAction> { new CardAction(ActionTypes.ImBack, "Yes", value: "Keep my choice"), new CardAction(ActionTypes.ImBack, "I'll pick something else.", value: "Something else") }
                                    };
                                    var msg = context.MakeMessage();
                                    msg.Attachments.Add(Card.ToAttachment());

                                    await context.PostAsync(msg);
                                    context.Wait(this.ConfirmPlan);

                                }
                                else
                                {
                                    var Card = new HeroCard
                                    {
                                        Title = "Great Choice!",
                                        Text = Message,
                                        Images = new List<CardImage> { new CardImage("http://www.madcalm.com/wp-content/uploads/2018/06/MADCALM-THUMBSUP.png") },
                                    };
                                    var msg = context.MakeMessage();
                                    msg.Attachments.Add(Card.ToAttachment());

                                    await context.PostAsync(msg);
                                    context.Done(2);
                                }
                            }
                        }
                    }
                }
                else
                {
                    switch (optionSelected)
                    {
                        case "analyse":
                        case "Analyse":
                            await this.ShowUsageCards(context);
                            context.Wait(this.ChosenPlan);
                            break;
                        //context.Call(new Node10(), this.ResumeAfterOptionDialog);
                        //break;
                        default:
                            await context.PostAsync($"I'm sorry.  I don't understand that response.  If you have finished looking at your analysis please scroll back up and choose a plan from the list.");
                            failCount++;
                            if (failCount < 3)
                            {
                                context.Wait(this.ChosenPlan);
                            }
                            else
                            {
                                await this.ShowPlanCarouselAsync(context);
                            }
                            break;
                    }
                }

            }

            catch (TooManyAttemptsException ex)
            {
                await context.PostAsync($"Ooops! Too many attemps :(. But don't worry, I'm handling that exception and you can try again!");

                context.Done(0);
            }
        }

        public virtual async Task ConfirmPlan(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            switch (message.Text)
            {
                case "Yes":
                case "yes":
                case "Keep my choice":
                case "Im sure":
                case "im sure":
                case "i'm sure":
                case "I'm sure":
                    //  call the next node here
                    context.Done(2);
                    break;
                case "No":
                case "no":
                case "Something else":
                    await this.ShowPlanCarouselAsync(context);
                    break;


            }
        }

        protected async Task ShowUsageCards(IDialogContext context)
        {
            int subsno;

            var reply = context.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = new List<Attachment>();

            var CardList = new List<HeroCard>();
            context.ConversationData.TryGetValue("SubsNumber", out subsno);

            var collection = _database.GetCollection<BsonDocument>("images");
            var filter = Builders<BsonDocument>.Filter.Eq("Anon_Subsno", subsno); // new BsonDocument(); 
            var sort = Builders<BsonDocument>.Sort.Ascending("Priority");
            var count = 0;
            //await context.PostAsync($"Querying collection [{subsno}]");
            using (var cursor = await collection.FindAsync(filter, new FindOptions<BsonDocument, BsonDocument>()
            {
                Sort = sort
            }))
            {
                while (await cursor.MoveNextAsync())
                {
                    var batch = cursor.Current;
                    foreach (var document in batch)
                    {
                        // process document
                        count++;
                        var name = document.GetElement("Name").Value;
                        var image = document.GetElement("Image").Value;

                        //await context.PostAsync($"Read name[{name}] and image[{image}]");
                        var title = name;
                        var subtitle = name;
                        var text = "";
                        switch ((string)name)
                        {
                            case "RECENT_BILLS":
                                title = "Recent Bills";
                                subtitle = "Here is a summary of your 3 most recent bills.";
                                break;
                            case "VOICE":
                                title = "Voice Usage";
                                subtitle = "Voice calls made over the last 3 months.";
                                break;
                            case "TEXT":
                                title = "Text Usage";
                                subtitle = "SMS sent over the last 3 months.";
                                break;
                            case "DATA":
                                title = "Data Usage";
                                subtitle = "Data usage over the last 3 months.";
                                break;
                            case "INT_VOICE":
                                title = "International Voice Usage";
                                subtitle = "International voice calls made over the last 3 months.";
                                break;
                            case "INT_TEXT":
                                title = "International Text Usage";
                                subtitle = "International SMS sent over the last 3 months.";
                                break;
                            case "INTL_MAP":
                                title = "International Summary";
                                subtitle = "These are the countries you have called or texted over the last 3 months.";
                                break;
                            case "ROAMING_MAP":
                                title = "Roaming Summary";
                                subtitle = "These are the roaming countries where you have used your phone over the last 3 months.";
                                break;
                            case "SUMMARY":
                                title = "High Level Summary";
                                subtitle = "A high level summary of your average usage over the last 3 months.";
                                break;

                        }

                        var Card = new HeroCard
                        {
                            Title = (string)title,
                            Subtitle = (string)subtitle,
                            Text = (string)text,
                            Images = new List<CardImage> { new CardImage((string)image) },
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
            await context.PostAsync($"Here is your analysis.  You can scroll back up and still choose a plan from the list above once you are finished looking.");
        }


        protected async Task ShowSummaryCardAsync(IDialogContext context)
        {
            int subsno;
            context.ConversationData.TryGetValue("SubsNumber", out subsno);

            var reply = context.MakeMessage();

            var collection = _database.GetCollection<BsonDocument>("images");
            var filter = Builders<BsonDocument>.Filter.Eq("Anon_Subsno", subsno) & Builders<BsonDocument>.Filter.Eq("Name", "SUMMARY");
            //await context.PostAsync($"Querying collection [{subsno}]");
            using (var cursor = await collection.FindAsync(filter))
            {
                
                while (await cursor.MoveNextAsync())
                {
                    var batch = cursor.Current;
                    foreach (var document in batch)
                    {
                        var image = document.GetElement("Image").Value;

                        //await context.PostAsync($"Read name[{name}] and image[{image}]");

                        var Card = new HeroCard
                        {
                            Title = "Analysis Summary",
                            Text = "Here is a high level summary of my analysis of the way you use your phone.  If you'd like to see more detail, click on Show Analysis in the plan list.",
                            Images = new List<CardImage> { new CardImage((string)image) },
                        };
                        reply.Attachments.Add(Card.ToAttachment());


                    }
                }
            }
            await context.PostAsync(reply);
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