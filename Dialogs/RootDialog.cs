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


    using MultiDialogsBot.Helper;    
      

    [Serializable]
    public class RootDialog :  CommonDialog  // IDialog<object>       
    {
        private const string YesOption = "Yes - lets do it!!";  
        private const string BusyOption = "No - Im busy right now.  Lets do it later.";
        private const string NoOption = "No - Im not interested.";           
        private const string CallBackOption = "No - Id rather talk to a person about it.";   
          
        protected static IMongoClient _client;    
        protected static IMongoDatabase _database;
           
        string HSET_Brand;    
        string HSET_Model;  
            
        public override async Task StartAsync(IDialogContext context)
        {     
            context.Wait(this.ShowCharacters);  
        }   

        private async Task MainEntryPoint(IDialogContext context )
        {
            DateTime time = DateTime.Now;
            int hour = time.Hour;
            string salutation,subsName;
            TimeZone tz = TimeZone.CurrentTimeZone;


            //context.ConversationData.SetValue("HandsetModelKey", "iphone 7 plus- 256gb");
            if (CommonDialog.debugMessages)
            { 
                await context.PostAsync("DEBUG : Beginning of program");
                await context.PostAsync("DEBUG : My timezone is " + tz.StandardName.ToString());
            }
            await Miscellany.InsertDelayAsync(context);
            if ((hour > 5) && (hour < 12))
                salutation = "Good morning, ";    
            else if (hour < 19)
                salutation = "Good afternoon, ";
            else
                salutation = "Good evening, ";
            context.ConversationData.TryGetValue("SubsName", out subsName);
            await context.PostAsync(salutation + subsName);
            await Miscellany.InsertDelayAsync(context);
            await context.PostAsync("Welcome to the MC upgrade BOT demo. Currently the demo covers plan changes, phone upgrades or both of these together.");
            await Miscellany.InsertDelayAsync(context);
            await context.PostAsync("How can I help you today?");
            context.Call(new NodeLUISBegin(), DoneInitiaLuisAsync);
        }
               
        public virtual async Task ShowCharacters(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var heroCard = new HeroCard
            {
                Title = "Hi There!",
                Subtitle = "Im MC - the MadCalm demo bot.",
                Text = "Im here to show you how great a chat bot can be for helping customers to change their plan or get a new phone.  First of all though, we need to choose a customer to use for the rest of the demo, so please make a selection from the list below.",
                Images = new List<CardImage> { new CardImage("http://madcalm.com/wp-content/uploads/2018/06/MADCALM-HELLO.png") },
                Buttons = new List<CardAction> { }
            };

            var message = context.MakeMessage();
            message.Attachments.Add(heroCard.ToAttachment());

            await context.PostAsync(message);
            await Task.Delay(5000);


            var reply = ((Activity)context.Activity).CreateReply();

            var Card_1 = new ThumbnailCard
            {
                Title = "Ryan",
                Subtitle = "The tech lover.",
                Text = "Ryan loves to have the latest tech.  He loves to have the internet everywhere he goes and his phone is great for this as he travels a lot for his work.",
                Images = new List<CardImage> { new CardImage("http://www.madcalm.com/wp-content/uploads/2018/06/boy_c.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Pick Me!", value: "876524834") }
            };

            var Card_2 = new ThumbnailCard
            {
                Title = "Pete",
                Subtitle = "The travelling business man.",
                Text = "Pete's business takes him all over the world and he always takes his phone with him.  He has been with the same carrier for a long time and needs lots of international calls, roaming and data included.",
                Images = new List<CardImage> { new CardImage("http://www.madcalm.com/wp-content/uploads/2018/06/boy_d.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Pick Me!", value: "872033118") }
            };

            var Card_3 = new ThumbnailCard
            {
                Title = "Jennifer",
                Subtitle = "Family all over the world.",
                Text = "Jennifer has recently moved overseas.  She loves to keep in touch with her family back home and all over the world, so needs lots of international calls included.",
                Images = new List<CardImage> { new CardImage("http://www.madcalm.com/wp-content/uploads/2018/06/girl_a.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Pick Me!", value: "857701192") }
            };

            var Card_4 = new ThumbnailCard
            {
                Title = "Mervyn",
                Subtitle = "The data user.",
                Text = "Mervyn is a millennial who is just starting out in the workplace - so his budget is tight.  He doesn't really use his phone for calls or texts but is always on the latest apps or browsing the internet.",
                Images = new List<CardImage> { new CardImage("http://www.madcalm.com/wp-content/uploads/2018/06/boy_a.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Pick Me!", value: "876403453") }
            };

            var Card_5 = new ThumbnailCard
            {
                Title = "Tania",
                Subtitle = "The techo-phobe.",
                Text = "Tania has just gotten used to using her phone to call and text, so she doesn't really use it for data.  She just wants something easy to use and simple.",
                Images = new List<CardImage> { new CardImage("http://www.madcalm.com/wp-content/uploads/2018/06/girl_d.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Pick Me!", value: "830123752") }
            };

            var Card_6 = new ThumbnailCard
            {
                Title = "Oliver",
                Subtitle = "The heavy user.",
                Text = "Oliver uses his phone for absolutely everything.  He is always calling, text or browsing the internet.",
                Images = new List<CardImage> { new CardImage("http://www.madcalm.com/wp-content/uploads/2018/06/boy_b.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Pick Me!", value: "834795990") }
            };

            var Card_7 = new ThumbnailCard
            {
                Title = "Amanda",
                Subtitle = "The chatter-box",
                Text = "Amanda is always on the phone to friends and family but is also conscious of how much she uses so that she doesnt get charged extra.",
                Images = new List<CardImage> { new CardImage("http://www.madcalm.com/wp-content/uploads/2018/06/girl_b.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Pick Me!", value: "830313284") }
            };

            reply.AttachmentLayout = "carousel";
            reply.Attachments.Add(Card_1.ToAttachment());
            reply.Attachments.Add(Card_2.ToAttachment());
            reply.Attachments.Add(Card_3.ToAttachment());
            reply.Attachments.Add(Card_6.ToAttachment());
            reply.Attachments.Add(Card_4.ToAttachment());
            reply.Attachments.Add(Card_5.ToAttachment());
            reply.Attachments.Add(Card_7.ToAttachment());


            await context.PostAsync(reply);
            context.Wait(CharacterSelectedAsync);


        }

        private async Task CharacterSelectedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (message.Text.All(Char.IsDigit))
            {
                int subsno = Int32.Parse(message.Text);
                if (CommonDialog.debugMessages) await context.PostAsync($"DEBUG: Searching for documents matching [{message.Text}][{subsno}]");
                _client = new MongoClient("mongodb://telcoretentiondb:HsQmjXjc0FBMrWYbJr8eUsGdWoTuaYXvdO2PRj13sxoPYijxxcxG5oSDfhFtVFWAFeWxFbuyf1NbxnFREFssAw==@telcoretentiondb.documents.azure.com:10255/?ssl=true&replicaSet=globaldb");
                _database = _client.GetDatabase("madcalm");

                var collection = _database.GetCollection<BsonDocument>("offers");
                var filter = Builders<BsonDocument>.Filter.Eq("Anon Subsno", subsno); // new BsonDocument(); 
                var count = 0;
                using (var cursor = await collection.FindAsync(filter))
                {
                    while (await cursor.MoveNextAsync())
                    {
                        var batch = cursor.Current;
                        foreach (var document in batch)
                        {
                            // process document
                            count++;

                            await this.StoreDocumentValues(context, document);

                            var brand = document.GetElement("HSet Brand").Value;
                            var model = document.GetElement("HSet Model").Value;

                            HSET_Brand = (string)brand;
                            HSET_Model = (string)model;

                            //await context.PostAsync($"Found matching document no [{count}] Handset [{HSET_Brand}][{HSET_Model}]");
                        }
                    }
                }

                if (CommonDialog.debugMessages) await context.PostAsync($"DEBUG:Total matching documents found is [{count}].");

                var hset_collection = _database.GetCollection<BsonDocument>("handsets");
                var hset_filter = Builders<BsonDocument>.Filter.Eq("Maker", HSET_Brand) & Builders<BsonDocument>.Filter.Eq("Model", HSET_Model);
                using (var hset_cursor = await hset_collection.FindAsync(hset_filter))
                {
                    while (await hset_cursor.MoveNextAsync())
                    {
                        var hset_batch = hset_cursor.Current;
                        foreach (var hset_document in hset_batch)
                        {
                            // process document
                            var IMAGE = hset_document.GetElement("Image").Value;
                            context.ConversationData.SetValue("Handset_Image", hset_document.GetElement("Image").Value);

                            //await context.PostAsync($"Handset image is [{IMAGE}].");

                        }
                    }
                }
                await MainEntryPoint(context);
              //  context.Wait(this.MainEntryPoint);//await this.ShowOptions(context);
            }
            else
            {
                context.Done(1);
            }
        }



        public async Task StoreDocumentValues(IDialogContext context, BsonDocument document)
        {
            string resType = (string)document.GetElement("Result Type Code").Value;

            context.ConversationData.SetValue("HandsetMakerKey", document.GetElement("HSet Brand").Value);
            context.ConversationData.SetValue("HandsetModelKey", document.GetElement("HSet Model").Value);
            context.ConversationData.SetValue("SubsNumber", document.GetElement("Anon Subsno").Value);
            context.ConversationData.SetValue("SubsName", document.GetElement("Subscriber Name").Value);
            context.ConversationData.SetValue("CustNumber", document.GetElement("Cust No").Value);
            context.ConversationData.SetValue("CurrentPlan", document.GetElement("Current Plan").Value);
            context.ConversationData.SetValue(resType + "_OfferPlan", document.GetElement("Offer Plan").Value);
            context.ConversationData.SetValue(resType + "_OfferVoice", document.GetElement("Offer Voice Add-On").Value);
            context.ConversationData.SetValue(resType + "_OfferText", document.GetElement("Offer Text Add-On").Value);
            context.ConversationData.SetValue(resType + "_OfferData", document.GetElement("Offer Data Add-On").Value);
            context.ConversationData.SetValue(resType + "_OfferPlanCode", document.GetElement("Offer Plan").Value);
            context.ConversationData.SetValue(resType + "_OfferVoiceCode", document.GetElement("Offer Voice Code").Value);
            context.ConversationData.SetValue(resType + "_OfferTextCode", document.GetElement("Offer Text Code").Value);
            context.ConversationData.SetValue(resType + "_OfferDataCode", document.GetElement("Offer Data Code").Value);
            context.ConversationData.SetValue("CurrentRev", document.GetElement("Current Revenue").Value);
            context.ConversationData.SetValue("CurrentMAF", document.GetElement("Current MAF").Value);
            context.ConversationData.SetValue("CurrentRent", document.GetElement("Current Rental").Value);
            context.ConversationData.SetValue("CurrentOvg", document.GetElement("Current Overage").Value);
            context.ConversationData.SetValue(resType + "_OfferRev", document.GetElement("Offer Revenue").Value);
            context.ConversationData.SetValue(resType + "_OfferMAF", document.GetElement("Offer MAF").Value);
            context.ConversationData.SetValue(resType + "_OfferRent", document.GetElement("Offer Rental").Value);
            context.ConversationData.SetValue(resType + "_OfferOvg", document.GetElement("Offer Overage").Value);
            context.ConversationData.SetValue("Month1", document.GetElement("Analysed Month 1").Value);
            context.ConversationData.SetValue("Month2", document.GetElement("Analysed Month 2").Value);
            context.ConversationData.SetValue("Month3", document.GetElement("Analysed Month 3").Value);
            context.ConversationData.SetValue("ValueScore", document.GetElement("Value Score").Value);
            context.ConversationData.SetValue("ValueRank", document.GetElement("Value Rank").Value);

            string Handset = document.GetElement("HSet Brand").Value + " " + document.GetElement("HSet Model").Value;
            context.ConversationData.SetValue("Handset", Handset); 


        }


        private async Task DoneInitiaLuisAsync(IDialogContext context, IAwaitable<object> result)             
        { 
            var ret = await result;
            Tuple<string, NodeLUISBegin.EIntent> luisOutput = ret as Tuple<string, NodeLUISBegin.EIntent>;

            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : NodeLuisBegin returned : " + ret.ToString());
            if (((Tuple<string, NodeLUISBegin.EIntent>)ret).Item2 == NodeLUISBegin.EIntent.HandSet)
            {
                context.ConversationData.SetValue(BotConstants.FLOW_TYPE_KEY, BotConstants.EQUIPMENT_FLOW_TYPE);
                context.Call(new NodePhoneFlow(((Tuple<string, NodeLUISBegin.EIntent>)ret).Item1), PhoneFlowDone);
            }
            else if (((Tuple<string, NodeLUISBegin.EIntent>)ret).Item2 == NodeLUISBegin.EIntent.Plan)
            {
                context.ConversationData.SetValue(BotConstants.FLOW_TYPE_KEY, BotConstants.PLAN_FLOW_TYPE);
                await Miscellany.InsertDelayAsync(context);
                await context.PostAsync("Sure.  I can help you to choose a new plan.");
                context.Call(new PlanNode(), PlanFlowDone);
            }
            else if (luisOutput.Item2 == NodeLUISBegin.EIntent.Both)
            {
                context.ConversationData.SetValue(BotConstants.FLOW_TYPE_KEY, BotConstants.BOTH_FLOW_TYPE);
                if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : Both intent detected");
                context.Call(new NodePhoneFlow(((Tuple<string, NodeLUISBegin.EIntent>)ret).Item1), PhoneFlowDone);
            }
            else
            {
                context.Wait(Restarting);
                context.Wait(ShowCharacters);
            }
        }

        public virtual async Task Restarting(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            await result;
            await MainEntryPoint(context);
        }
            private async Task PhoneFlowDone(IDialogContext context,IAwaitable<object> result)
        {
            await context.PostAsync("End of phone Flow - enter something");
            context.Wait(CharacterSelectedAsync);
            //  context.Wait(MessageReceivedAsync);
        }

        private async Task PlanFlowDone(IDialogContext context, IAwaitable<object> result)
        {
            await context.PostAsync("O.K.  Now I need to take you through the terms and conditions to finalise your order.");
            //     context.Wait(MessageReceivedAsync);
            context.Wait(CharacterSelectedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            await context.PostAsync("End of root node");

            if (message.Text.All(Char.IsDigit))
            {
                int subsno = Int32.Parse(message.Text);
                await context.PostAsync($" -> Searching for documents matching [{message.Text}][{subsno}]");

                _client = new MongoClient("mongodb://telcoretentiondb:HsQmjXjc0FBMrWYbJr8eUsGdWoTuaYXvdO2PRj13sxoPYijxxcxG5oSDfhFtVFWAFeWxFbuyf1NbxnFREFssAw==@telcoretentiondb.documents.azure.com:10255/?ssl=true&replicaSet=globaldb");
                _database = _client.GetDatabase("madcalm");

                var collection = _database.GetCollection<BsonDocument>("offers");
                var filter = Builders<BsonDocument>.Filter.Eq("Anon_Subsno", subsno); // new BsonDocument(); */
                var count = 0;
                using (var cursor = await collection.FindAsync(filter))
                {
                    while (await cursor.MoveNextAsync())
                    {
                        var batch = cursor.Current;
                        await context.PostAsync($"O batch tem {batch.Count()} documentos");
                        foreach (var document in batch)
                        {
                            // process document
                            count++;

                            await this.StoreDocumentValues(context, document);

                            var brand = document.GetElement("HSet_Brand").Value;
                            var model = document.GetElement("HSet_Model").Value;

                            HSET_Brand = (string)brand ;
                            HSET_Model = (string)model;

                            await context.PostAsync($"Found matching document no [{count}] Handset [{HSET_Brand}][{HSET_Model}]");
                        }
                    }
                }

                await context.PostAsync($"Total matching documents found is [{count}].");
                await context.PostAsync("Results of test method:" + TestMethod(subsno));
                


                var hset_collection = _database.GetCollection<BsonDocument>("handsets");
                var hset_filter = Builders<BsonDocument>.Filter.Eq("Maker", HSET_Brand) & Builders<BsonDocument>.Filter.Eq("Model", HSET_Model);
                using (var hset_cursor = await hset_collection.FindAsync(hset_filter))
                {
                    while (await hset_cursor.MoveNextAsync())
                    {
                        var hset_batch = hset_cursor.Current;
                        foreach (var hset_document in hset_batch)
                        {
                            // process document
                            var IMAGE = hset_document.GetElement("Image").Value;
                            context.ConversationData.SetValue("Handset_Image", hset_document.GetElement("Image").Value);

                            await context.PostAsync($"Handset image is [{IMAGE}].");

                        }
                    }
                }

                var img_collection = _database.GetCollection<BsonDocument>("images");
                var img_filter = Builders<BsonDocument>.Filter.Eq("Anon_Subsno", subsno);
                using (var img_cursor = await img_collection.FindAsync(img_filter))
                {
                    while (await img_cursor.MoveNextAsync())
                    {
                        var img_batch = img_cursor.Current;
                        foreach (var img_document in img_batch)
                        {
                            // process document
                            var name    = (string)img_document.GetElement("Name").Value;
                            var image   = (string)img_document.GetElement("Image").Value;
                            context.ConversationData.SetValue(name, image);
                        }
                    }
                }

                var intl_collection = _database.GetCollection<BsonDocument>("international");
                var intl_filter = Builders<BsonDocument>.Filter.Eq("Subscriber", subsno);
                string intl_desc = "";
                int intl_counter = 0;
                using (var intl_cursor = await intl_collection.FindAsync(intl_filter))
                {
                    while (await intl_cursor.MoveNextAsync())
                    {
                        var intl_batch = intl_cursor.Current;
                        foreach (var intl_document in intl_batch)
                        {
                            intl_counter++;

                           /* // process document
                            var country = intl_document.GetElement("Country").Value;
                            var m1_min = intl_document.GetElement("M1_Minutes").Value;
                            var m2_min = intl_document.GetElement("M2_Minutes").Value;
                            var m3_min = intl_document.GetElement("M3_Minutes").Value;
                            var m1_sms = intl_document.GetElement("M1_Texts").Value;
                            var m2_sms = intl_document.GetElement("M2_Texts").Value;
                            var m3_sms = intl_document.GetElement("M3_Texts").Value;

                            float imin = m1_min + m2_min + m3_min;
                            float isms = m1_sms + m2_sms + m3_sms;
                            //int i_m = (int)imin;
                            //int i_s = (int)isms;*/
                            if (intl_counter > 1)
                            {
                                intl_desc = intl_desc + ", ";
                            }
                            intl_desc = intl_desc + "{country}({imin} Minutes,{isms} Texts)";

                            context.ConversationData.SetValue("International_Desc", intl_desc);
                            await context.PostAsync($"Set international desc [{intl_desc}]");
                        }
                    }
                }

                await this.ShowOptions(context);
                //OnOptionSelected();
            }
            else
            {
                context.Done(1);
            }
        }


 
 
        public virtual async Task ShowOptions(IDialogContext context)
        {
            var heroCard = new HeroCard
            {
                Title = "Hi There!",
                Subtitle = "Im Vinnie from Vodafone.",
                Text = "Hi there!  I'm Vinnie from Vodafone, the Vodafone help bot.  I just thought I'd let you know that you are now eligible for an upgrade to a new phone.  We have some great new offers available that Ive personalised just for you.",
                Images = null/*new List<CardImage> { new CardImage("C:\\Development\\Usage Analysis Images\\vinnie.png") }*/,
                Buttons = new List<CardAction> {  }
            };

            var message = context.MakeMessage();
            /************/
            String strAppPath = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            String strFilePath = System.IO.Path.Combine(strAppPath, "Resources");
            String strFullFilename = System.IO.Path.Combine(strFilePath, "Vinnie.png");
            /***********/
            heroCard.Images = new List<CardImage> { new CardImage(strFullFilename) };
            message.Attachments.Add(heroCard.ToAttachment());

            await context.PostAsync(message);
            PromptDialog.Choice(context, this.OnOptionSelected, new List<string>() { YesOption, BusyOption, NoOption, CallBackOption }, "Are you able to go through this with me now?", "Not a valid option", 3);
            //context.Wait(this.OnOptionSelected);

        }

        private async Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                switch (optionSelected)
                {
                    case YesOption:
                        context.Call(new Node2(), this.ResumeAfterOptionDialog);
                        break;

                    case BusyOption:
                        context.Call(new Node3(), this.ResumeAfterOptionDialog);
                        break;

                    case NoOption:
                        context.Call(new Node4(), this.ResumeAfterOptionDialog);
                        break;

                    case CallBackOption:
                        context.Call(new Node5(), this.ResumeAfterOptionDialog);
                        break;

                }
            }
            catch (TooManyAttemptsException ex)
            {
                await context.PostAsync($"Ooops! Too many attemps :(. But don't worry, I'm handling that exception and you can try again!");

                context.Wait(this.MessageReceivedAsync);
            }
        }

        private async Task ResumeAfterSupportDialog(IDialogContext context, IAwaitable<int> result)
        {
            var ticketNumber = await result;

            await context.PostAsync($"Thanks for contacting our support team. Your ticket number is {ticketNumber}.");
            context.Wait(this.MessageReceivedAsync);
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
                context.Wait(this.MessageReceivedAsync);
            }
        }
    }
}