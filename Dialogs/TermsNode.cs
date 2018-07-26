using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

using MultiDialogsBot.Database;
using MultiDialogsBot.Helper;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MultiDialogsBot.Dialogs
{

    [Serializable]
    public class TermsNode : CommonDialog
    {
        protected static IMongoClient _client;
        protected static IMongoDatabase _database;
        string subsno;
        int failCount;

        public override async Task StartAsync(IDialogContext context)
        {
            failCount = 0;
            if (CommonDialog.debugMessages) await context.PostAsync($"DEBUG: entering TermsNode");

            if (context.ConversationData.TryGetValue("SubsNumber", out subsno))
            {
                await Task.Delay(2500);
                int subsnum = Int32.Parse(subsno);

                string planName, planImage, phoneName, resCode;
                var choiceList = new List<string>();

                if (context.ConversationData.TryGetValue("ChosenPlanName", out planName))
                {
                    if (CommonDialog.debugMessages) await context.PostAsync($"DEBUG: Chosen plan is {planName}");

                    context.ConversationData.TryGetValue("ChosenPlanImage", out planImage);
                    context.ConversationData.TryGetValue("ChosenPlanCode", out resCode);

                    
                    if (context.ConversationData.TryGetValue("SelectedPhone", out phoneName))
                    {
                        if (CommonDialog.debugMessages) await context.PostAsync($"DEBUG: selected phone is {phoneName}");
                        await context.PostAsync($"Okay.  The phone and plan you have chosen are shown below.");

                        //  They have a new plan and a new phone, so show both in a carousel
                        var reply = context.MakeMessage();
                        reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                        reply.Attachments = new List<Attachment>();

                        var CardList = new List<HeroCard>();

                        var planCard = new HeroCard
                        {
                            Title = "Your selected plan",
                            Subtitle = planName,
                            
                            Images = new List<CardImage> { new CardImage(planImage) },
                            //  Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Pick Me!", value: "Choose " + Code), new CardAction(ActionTypes.ImBack, "Show Analysis", value: "Analyse") }
                        };

                        reply.Attachments.Add(planCard.ToAttachment());

                        var phoneCard = new HeroCard()
                        {
                            Title = Miscellany.Capitalize(GetModelBrand(phoneName)),
                            Subtitle = Miscellany.Capitalize(phoneName),
                            Text = "",
                            Images = new List<CardImage>() { new CardImage(GetEquipmentImageURL(phoneName, true, context), "img/jpeg") },
                            
                        };

                        reply.Attachments.Add(phoneCard.ToAttachment());
                        choiceList.Add("Yes - both are correct.");
                        choiceList.Add("No - I want a different phone.");
                        choiceList.Add("No - I want a different plan.");
                        choiceList.Add("No - I have changed my mind.");

                        await context.PostAsync(reply);
                    }
                    else
                    {
                        //  New plan only
                        await context.PostAsync($"Okay.  Please confirm the plan you have chosen below.");

                        //  They have a new plan and a new phone, so show both in a carousel
                        var reply = context.MakeMessage();
                        reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                        reply.Attachments = new List<Attachment>();

                        var CardList = new List<HeroCard>();

                        var planCard = new HeroCard
                        {
                            Title = "Your selected plan",
                            Subtitle = planName,

                            Images = new List<CardImage> { new CardImage(planImage) },
                            //  Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Pick Me!", value: "Choose " + Code), new CardAction(ActionTypes.ImBack, "Show Analysis", value: "Analyse") }
                        };

                        reply.Attachments.Add(planCard.ToAttachment());
                        choiceList.Add("Yes - that is correct.");
                        choiceList.Add("No - I want a different plan.");
                        choiceList.Add("No - I want a phone as well.");
                        choiceList.Add("No - I have changed my mind.");

                        await context.PostAsync(reply);

                    }
                }
                else
                {
                    if (CommonDialog.debugMessages) await context.PostAsync($"DEBUG: no Chosen Plan detected.");
                    if (context.ConversationData.TryGetValue("SelectedPhone", out phoneName))
                    {
                        if (CommonDialog.debugMessages) await context.PostAsync($"DEBUG: chosen phone is {phoneName}");

                        // New phone only
                        await context.PostAsync($"Okay.  Please confirm the phone you have chosen below.");

                        //  They have a new plan and a new phone, so show both in a carousel
                        var reply = context.MakeMessage();
                        reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                        reply.Attachments = new List<Attachment>();

                        var CardList = new List<HeroCard>();

                        
                        var phoneCard = new HeroCard()
                        {
                            Title = Miscellany.Capitalize(GetModelBrand(phoneName)),
                            Subtitle = Miscellany.Capitalize(phoneName),
                            Text = "",
                            Images = new List<CardImage>() { new CardImage(GetEquipmentImageURL(phoneName, true, context), "img/jpeg") },

                        };

                        reply.Attachments.Add(phoneCard.ToAttachment());

                        choiceList.Add("Yes - that is correct.");
                        choiceList.Add("No - I want a different phone.");
                        choiceList.Add("No - I want to change plan as well.");
                        choiceList.Add("No - I have changed my mind.");

                        await context.PostAsync(reply);

                    }
                    else
                    {
                        //  Nothing
                        await context.PostAsync("ERROR:  End of flow with no phone or plan chosen.");
                        context.Done(2);
                    }
                }

                PromptDialog.Choice(context, this.ConfirmSale, choiceList, "Are these details for your order correct?", "Not a valid option", 3);


            }
            else
            {
                await context.PostAsync($"Hmmm.  Seems I couldnt get the subscriber number.");
                context.Done(2);
            }
        }


        private async Task ConfirmSale(IDialogContext context, IAwaitable<string> result)
        {
            var message = await result;

            switch (message)
            {
                case "YES":
                case "Yes":
                case "yes":
                case "Yes - both are correct.":
                case "Yes - that is correct.":
                    context.Call(new Terms1(), TermsDone);
                    break;

                case "No - I want a different phone.":
                case "No - I want a phone as well.":
                    string thing = message;
                    context.Call(new NodePhoneFlow(thing), TermsDone);
                    break;

                case "No - I want to change plan as well.":
                case "No - I want a different plan.":
                    context.ConversationData.SetValue("ChosenPlanName", "-");
                    context.Call(new PlanNode(), TermsDone);
                    break;
                
                case "No - I have changed my mind.":
                case "NO":
                case "no":
                case "No":
                    var Card = new HeroCard
                    {
                        Title = "That's OK.",
                        Text = "If you have changed your mind then I will save this order so you can access it later, or just start again next time you want to chat.",
                        Images = new List<CardImage> { new CardImage("http://www.madcalm.com/wp-content/uploads/2018/06/MADCALM-PROCESSING.png") },
                    };
                    var msg = context.MakeMessage();
                    msg.Attachments.Add(Card.ToAttachment());

                    await context.PostAsync(msg);
                    context.Done(2);
                    break;
                default:
                    context.Done(2);
                    break;
            }
        }

        private async Task TermsDone(IDialogContext context, IAwaitable<object> result)
        {
            if (CommonDialog.debugMessages) await context.PostAsync($"DEBUG: inside TermsDone");

            context.Done(2);
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
                if (CommonDialog.debugMessages) await context.PostAsync($"DEBUG: in TermsNode.ResumeAfterOptionDialog");

                context.Done(2);
            }
        }
    }
}