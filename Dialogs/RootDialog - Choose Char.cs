﻿namespace MultiDialogsBot.Dialogs
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


    using Dialogs;


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
            context.Wait(this.MainEntryPoint);
        }   

        private async Task MainEntryPoint(IDialogContext context,IAwaitable<IMessageActivity> awaitable)
        {
            DateTime time = DateTime.Now;
            int hour = time.Hour;
            string salutation;
            TimeZone tz = TimeZone.CurrentTimeZone;

            context.ConversationData.SetValue("HandsetModelKey", "iphone 7 plus- 256gb");
            if (CommonDialog.debugMessages)
            { 
                await context.PostAsync("DEBUG : Beginning of program");
                await context.PostAsync("DEBUG : My timezone is " + tz.StandardName.ToString());
            }
            if ((hour > 5) && (hour < 12))
                salutation = "Good morning, ";    
            else if (hour < 19)
                salutation = "Good afternoon, ";
            else
                salutation = "Good evening, ";

            await context.PostAsync(salutation + "John Doe");
            await context.PostAsync("Welcome to the phones and plans page, if you need any assistance at any point\r\n I'd be delighted to help you choose the best phone and plan for you");
            await context.PostAsync("If you are an existing customer I can certainly make sure that any recommendation\r\n is highly personalized to your usage and phone requirements");
            await context.PostAsync("Let me know if I can help you with a new phone or plan");
            context.Call(new NodeLUISBegin(), DoneInitiaLuis);
         //   context.Wait(MessageReceivedAsync);
        }

        private async Task DoneInitiaLuis(IDialogContext context, IAwaitable<object> result)
        {
            var ret = await result;

            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : NodeLuisBegin returned : " + ret.ToString());
            if (((Tuple<string, NodeLUISBegin.EIntent>)ret).Item2 == NodeLUISBegin.EIntent.HandSet)
                context.Call(new NodePhoneFlow(((Tuple<string, NodeLUISBegin.EIntent>)ret).Item1), PhoneFlowDone);
           // else
                //PromptDialog.Choice(context, this.PhoneSelection, new List<string>() { NewPhone, CurrentPhone, NotSure }, "Have you thought about whether you want to get a new phone or if you are happy with your current phone?", "Not a valid option", 3);
        }

        private async Task PhoneFlowDone(IDialogContext context,IAwaitable<object> result)
        {
            await context.PostAsync("End of phone Flow");
            context.Wait(MessageReceivedAsync);
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


 
        public async Task StoreDocumentValues(IDialogContext context, BsonDocument document)
        {
            string resType = (string)document.GetElement("Result_Type").Value;

            context.ConversationData.SetValue("HandsetMakerKey", document.GetElement("HSet_Brand").Value);
            context.ConversationData.SetValue("HandsetModelKey", document.GetElement("HSet_Model").Value);
            context.ConversationData.SetValue("SubsNumber", document.GetElement("Anon_Subsno").Value);
            context.ConversationData.SetValue("CustNumber", document.GetElement("Cust_No").Value);
            context.ConversationData.SetValue("CurrentPlan", document.GetElement("Current_Plan").Value);
            context.ConversationData.SetValue(resType + "_OfferPlan", document.GetElement("Offer_Plan").Value);
            context.ConversationData.SetValue(resType + "_OfferVoice", document.GetElement("Offer_Voice_AddOn").Value);
            context.ConversationData.SetValue(resType + "_OfferText", document.GetElement("Offer_Text_AddOn").Value);
            context.ConversationData.SetValue(resType + "_OfferData", document.GetElement("Offer_Data_AddOn").Value);
            context.ConversationData.SetValue("CurrentRev", document.GetElement("Current_Revenue").Value);
            context.ConversationData.SetValue("CurrentMAF", document.GetElement("Current_MAF").Value);
            context.ConversationData.SetValue("CurrentRent", document.GetElement("Current_Rental").Value);
            context.ConversationData.SetValue("CurrentOvg", document.GetElement("Current_Overage").Value);
            context.ConversationData.SetValue(resType + "_OfferRev", document.GetElement("Offer_Revenue").Value);
            context.ConversationData.SetValue(resType + "_OfferMAF", document.GetElement("Offer_MAF").Value);
            context.ConversationData.SetValue(resType + "_OfferRent", document.GetElement("Offer_Rental").Value);
            context.ConversationData.SetValue(resType + "_OfferOvg", document.GetElement("Offer_Overage").Value);
            context.ConversationData.SetValue("Month1", document.GetElement("Analysed_Month_1").Value);
            context.ConversationData.SetValue("Month2", document.GetElement("Analysed_Month_2").Value);
            context.ConversationData.SetValue("Month3", document.GetElement("Analysed_Month_3").Value);
            context.ConversationData.SetValue(resType + "_Message1", document.GetElement("Sales_Message__1").Value);
            context.ConversationData.SetValue(resType + "_Message2", document.GetElement("Sales_Message__2").Value);
            context.ConversationData.SetValue(resType + "_Message3", document.GetElement("Sales_Message__3").Value);
            context.ConversationData.SetValue(resType + "_Message4", document.GetElement("Sales_Message__4").Value);
            context.ConversationData.SetValue(resType + "_Message5", document.GetElement("Sales_Message__5").Value);
            context.ConversationData.SetValue(resType + "_Message6", document.GetElement("Sales_Message__6").Value);
            context.ConversationData.SetValue(resType + "_Message7", document.GetElement("Sales_Message__7").Value);
            context.ConversationData.SetValue(resType + "_Message8", document.GetElement("Sales_Message__8").Value);
            context.ConversationData.SetValue(resType + "_Message9", document.GetElement("Sales_Message__9").Value);
            context.ConversationData.SetValue(resType + "_Message10", document.GetElement("Sales_Message__10").Value);
            context.ConversationData.SetValue(resType + "_Value1", document.GetElement("Reserved_Column_16").Value);
            context.ConversationData.SetValue(resType + "_Value2", document.GetElement("Reserved_Column_17").Value);
            context.ConversationData.SetValue(resType + "_Value3", document.GetElement("Reserved_Column_18").Value);
            context.ConversationData.SetValue(resType + "_Value4", document.GetElement("Reserved_Column_19").Value);
            context.ConversationData.SetValue(resType + "_Value5", document.GetElement("Reserved_Column_20").Value);
            context.ConversationData.SetValue(resType + "_Value6", document.GetElement("Reserved_Column_21").Value);
            context.ConversationData.SetValue(resType + "_Value7", document.GetElement("Reserved_Column_22").Value);
            context.ConversationData.SetValue(resType + "_Value8", document.GetElement("Reserved_Column_23").Value);
            context.ConversationData.SetValue(resType + "_Value9", document.GetElement("Reserved_Column_24").Value);
            context.ConversationData.SetValue(resType + "_Value10", document.GetElement("Reserved_Column_25").Value);
            context.ConversationData.SetValue(resType + "_BirthDate", document.GetElement("DATE_OF_BIRTH").Value);

            string Handset = document.GetElement("HSet_Brand").Value + " " + document.GetElement("HSet_Model").Value;
            context.ConversationData.SetValue("Handset", Handset);


        }

        public virtual async Task ShowCharacters(IDialogContext context)
        {
            var Card_1 = new HeroCard
            {
                Title = "Ryan",
                Subtitle = "The tech lover.",
                Text = "Ryan loves to have the latest tech.  He loves to have the internet everywhere he goes and his phone is great for this as he travels a lot for his work.",
                Images = null/*new List<CardImage> { new CardImage("www.madcalm.com/wp-content/uploads/2018/06/boy_c.png") }*/,
                Buttons = new List<CardAction> {  }
            };

            var Card_2 = new HeroCard
            {
                Title = "Pete",
                Subtitle = "The travelling business man.",
                Text = "Pete's business takes him all over the world and he always takes his phone with him.  He has been with the same carrier for a long time and needs lots of international calls, roaming and data included.",
                Images = null/*new List<CardImage> { new CardImage("www.madcalm.com/wp-content/uploads/2018/06/boy_d.png") }*/,
                Buttons = new List<CardAction> { }
            };

            var Card_3 = new HeroCard
            {
                Title = "Jennifer",
                Subtitle = "Family all over the world.",
                Text = "Jennifer has recently moved overseas.  She loves to keep in touch with her family back home and all over the world, so needs lots of international calls included.",
                Images = null/*new List<CardImage> { new CardImage("www.madcalm.com/wp-content/uploads/2018/06/girl_a.png") }*/,
                Buttons = new List<CardAction> { }
            };

            var Card_4 = new HeroCard
            {
                Title = "Mervyn",
                Subtitle = "The data user.",
                Text = "Mervyn is a millennial who is just starting out in the workplace - so his budget is tight.  He does really use his phone for calls or texts but is always on the latest apps or browsing the internet.",
                Images = null/*new List<CardImage> { new CardImage("www.madcalm.com/wp-content/uploads/2018/06/boy_a.png") }*/,
                Buttons = new List<CardAction> { }
            };

            var Card_5 = new HeroCard
            {
                Title = "Tania",
                Subtitle = "The techo-phobe.",
                Text = "Tania has just gotten used to using her phone to call and text, so she doesn't really use it for data.  She just wants something easy to use and simple.",
                Images = null/*new List<CardImage> { new CardImage("www.madcalm.com/wp-content/uploads/2018/06/girl_d.png") }*/,
                Buttons = new List<CardAction> { }
            };

            var Card_6 = new HeroCard
            {
                Title = "Oliver",
                Subtitle = "The heavy user.",
                Text = "Oliver uses his phone for absolutely everything.  He is always calling, text or browsing the internet.",
                Images = null/*new List<CardImage> { new CardImage("www.madcalm.com/wp-content/uploads/2018/06/boy_b.png") }*/,
                Buttons = new List<CardAction> { }
            };

            var Card_7 = new HeroCard
            {
                Title = "Amanda",
                Subtitle = "The chatter-box",
                Text = "Amanda is always on the phone to friends and family but is also conscious of how much she uses so that she doesnt get charged extra.",
                Images = null/*new List<CardImage> { new CardImage("www.madcalm.com/wp-content/uploads/2018/06/girl_b.png") }*/,
                Buttons = new List<CardAction> { }
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