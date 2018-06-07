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
            context.Wait(this.MessageReceivedAsync);
 
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
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