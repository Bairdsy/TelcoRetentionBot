using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Threading.Tasks;


using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace MultiDialogsBot.Dialogs
{
    [Serializable]
    public class LessThan5Node : CommonDialog
    {
        List<string> modelList;
        string selectedModel;

        public LessThan5Node(List<string> models2Show)
        {
            modelList = models2Show;
        }


        public override async Task StartAsync(IDialogContext context)
        {
            if (debugMessages) await context.PostAsync($"DEBUG : StartAsync() method in LessThan5Node object, I received {modelList.Count} models to display");

            if (modelList.Count == 1)
                await DisplaySinglePhoneCardAsync(context, modelList[0]);
            else
                await DisplayMultiPhoneCarouselAnsyc(context, modelList);
        }

        private async Task CarouselSelectionButtonReceivedAsync(IDialogContext context,IAwaitable<object> awaitable)
        {
            IMessageActivity msg = (IMessageActivity)(await awaitable);
            string modelPicked,buttonPressed = msg.Text ;

            if (buttonPressed.StartsWith("I want "))
            {
                modelPicked = selectedModel = buttonPressed.Substring(7);
                context.Done(modelPicked);
            }
            else if (buttonPressed == "choose")
            {
                if (MoreThanOneBrand() ||  // If there is more than one brand or if the carousel covers the whole of model for the brand, He needs to choose from the list of brands. 
                    modelList.Count >= GetBrandModels(GetModelBrand(modelList[0])).Count)
                    context.Done("~");    
                else
                {
                    /*   string[] models2Exclude = this.modelList.ToArray();

                       unwantedModels = string.Concat("~", string.Join("~", models2Exclude));
                       context.Done(unwantedModels);*/
                    AskChangeBrandOrDifferentModel(context);
                }
            }
        }

        private void AskChangeBrandOrDifferentModel(IDialogContext context)
        {
            List<string> opt = new List<string>() { "Change Brand" };
            string prompt,brand = GetModelBrand(modelList[0]);
            Dictionary<string, bool> modelSet = GetBrandModels(brand);

            prompt = $"Do you want to change brand or see the other {modelSet.Count - modelList.Count} phones that I have on {brand}?";
            opt.Add($"See {brand} catalogue");
            PromptDialog.Choice(context, ChangeBrandChoiceReceivedAsync, opt, prompt, "Not understood, please try again", 5);
        }

        private async Task SelectionButtonReceivedAsync(IDialogContext context,IAwaitable<object> awaitable)
        {
            IMessageActivity messageActivity = (IMessageActivity)(await awaitable);
            string model,buttonPressed;
            List<string> options = new List<string>() { "I'll pick", "Help me work it out" };
 

            buttonPressed = messageActivity.Text;

            if (buttonPressed.StartsWith("I want "))
            {
                selectedModel = model = buttonPressed.Substring(7);
                context.Done(model);
            }
            else if (buttonPressed.StartsWith("No"))
            {
                PromptDialog.Choice(context, 
                    WrongRecoverOptionReceivedAsync, 
                    options, 
                    "I'm sorry I got that wrong. Shall we do another go or do you want to pick a phone from a list of everything?",
                    "Not understood, please try again",
                    4);
            }
        }

        private async Task MessageReceivedAsync(IDialogContext context,IAwaitable<object> awaitable)
        {
            await context.PostAsync("LessThan5Node object - End of dialog");
            await context.PostAsync("0 OK, 0:1");
            context.Wait(MessageReceivedAsync);
        }

        private async Task DisplayMultiPhoneCarouselAnsyc(IDialogContext context, List<String> models)
        {
            var reply = ((Activity)context.Activity).CreateReply();
            HeroCard heroCard;

            await context.PostAsync("Great! Here are some phones that match your choices. Choose one of them that you'd like");
            await context.PostAsync("or if you are not happy with these choices, type \"choose\"");

            reply.AttachmentLayout = "carousel";
            foreach (var model in models)
            {
                heroCard = new HeroCard()
                {
                    Title = GetModelBrand(model),
                    Subtitle = model,    
                    Text = "",
                    Images = new List<CardImage>() { new CardImage(GetEquipmentImageURL(model,true), "img/jpeg") },
                    Buttons = new List<CardAction>()
                    {
                        new CardAction(){Title = "Pick Me!",Type = ActionTypes.ImBack, Value ="I want " + model},
                        new CardAction(){Title = "Plan Prices", Type = ActionTypes.ImBack,Value = "Plan Prices for " + model },   
                        new CardAction(){Title = "Reviews",Type = ActionTypes.ImBack,Value = "Reviews for " + model},
                        new CardAction (){Title = "Specifications",Type=ActionTypes.ImBack,Value = "Specs for " + model }
                    }
                };
                reply.Attachments.Add(heroCard.ToAttachment());
            }
            await context.PostAsync(reply);
            context.Wait(CarouselSelectionButtonReceivedAsync);
        }

        private async Task DisplaySinglePhoneCardAsync(IDialogContext context, string model)
        {
            string equipmentURL;
            var reply = ((Activity)context.Activity).CreateReply();
            HeroCard heroCard;

            this.selectedModel = model;
            await context.PostAsync("Great! I've narrowed it down to the perfect phone for you. This is it :");
            equipmentURL = GetEquipmentImageURL(model,true);
            heroCard = new HeroCard()    
            {
                Title = GetModelBrand(model),
                Subtitle = model,
                Text = "Please click one of the buttons to continue",
                Images = new List<CardImage> { new CardImage(equipmentURL, "img/jpeg") },
                Buttons = new List<CardAction>()
                {
                    new CardAction(){Title = "Yes - Great Choice", Type=ActionTypes.ImBack, Value = "I want " + model},
                    new CardAction(){Title = "No - That's not quite right",Type = ActionTypes.ImBack,Value = "No - That's not quite right"},
                    new CardAction(){Title = "Plan Prices",Type=ActionTypes.ImBack,Value = "Plan Prices for " + model},
                    new CardAction(){Title = "Reviews",Type=ActionTypes.ImBack,Value = "Reviews for " + model},
                    new CardAction(){Title = "Specifications",Type=ActionTypes.ImBack, Value= "Specs for " + model }
                }
            };
            reply.Attachments.Add(heroCard.ToAttachment());

            await context.PostAsync(reply);
            context.Wait(SelectionButtonReceivedAsync);
        }          

        private async Task CongratulateSubsAsync(IDialogContext context,string model)
        {
            string phoneMatchMsg = "The phone match message will be inserted here";

            await context.PostAsync($"Great Choice - The {model} is perfect for you because {phoneMatchMsg}. Now we need to work out what plan you should be on");
        }

        private async Task WrongRecoverOptionReceivedAsync(IDialogContext context,IAwaitable<string> awaitable)
        {
            string brand, ans, prompt;
            Dictionary<string,bool> modelsVector;
            List<string> options = new List<string>() { "Change Brand" };

            ans = await awaitable;
            if (ans.StartsWith("I'll "))
            {
                brand = GetModelBrand(selectedModel);
                modelsVector = GetBrandModels(brand);
                options.Add($"See {brand} catalogue");
                prompt = $"Do you want to change brand or see the other {modelsVector.Count - 1} phones that we have on {brand}?";
                PromptDialog.Choice(context, ChangeBrandChoiceReceivedAsync, options, prompt, "Not understood, please try again", 4);
            }
            else
                context.Wait(MessageReceivedAsync);
        }

        public async Task ChangeBrandChoiceReceivedAsync(IDialogContext context,IAwaitable<string> awaitable)   
        {
            string ans = await awaitable;

            if (debugMessages) await context.PostAsync($"DEBUG : You picked " + ans);

            if (ans.StartsWith("Change"))  // He doesn't want this brand
                context.Done("~");
            else if (ans.StartsWith("See"))
                context.Done(string.Concat("~", string.Join("~",modelList.ToArray())));
            else
                context.Wait(MessageReceivedAsync);
        }

        private bool MoreThanOneBrand()
        {
            string brand = GetModelBrand(this.modelList[0]);

            for (int i = 1; i < modelList.Count; ++i)
                if (brand != GetModelBrand(modelList[i]))
                    return true;
            return false;
        }
    }
}