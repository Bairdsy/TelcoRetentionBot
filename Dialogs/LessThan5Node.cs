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
            else if (buttonPressed.ToLower() == "start again")
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
            List<string> options;  
 

            buttonPressed = messageActivity.Text;

            if (buttonPressed.StartsWith("I want "))
            {
                selectedModel = model = buttonPressed.Substring(7);
                context.Done(model);
            }
            else if (buttonPressed.StartsWith("No"))
            {
                string brand = Capitalize(GetModelBrand(this.selectedModel));

                options = new List<string>() { $"Yes, I want to stay with {brand}", "No" };
                PromptDialog.Choice(context, 
                    WrongRecoverOptionReceivedAsync, 
                    options, 
                    $"Do you still want to look at {brand} range or look at other phones?",
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

            await context.PostAsync($"Great choice! There are {modelList.Count} different versions for you to choose from");
            await context.PostAsync("If you change your mind I can help you to choose something else. Just type \"Start Again\" to find a more suitable model");

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
                        new CardAction(){Title = "Reviews",Type = ActionTypes.OpenUrl,Value = GetModelReviewsUrl( model)},
                        new CardAction (){Title = "Specifications",Type=ActionTypes.OpenUrl,Value = GetModelSpecsUrl( model) }
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
            await context.PostAsync("Great. Let's have a look at that phone now.");
            equipmentURL = GetEquipmentImageURL(model,true);
            heroCard = new HeroCard()      
            {
                Title = Capitalize(GetModelBrand(model)),
                Subtitle = Capitalize(model),
                Text = "Click one of the buttons to continue.",
                Images = new List<CardImage> { new CardImage(equipmentURL, "img/jpeg") },
                Buttons = new List<CardAction>()
                {
                    new CardAction(){Title = "Yes - that's the phone I like", Type=ActionTypes.ImBack, Value = "I want " + model},
                    new CardAction(){Title = "No. I am after a different model",Type = ActionTypes.ImBack,Value = "No. I am after a different model"},
                    new CardAction(){Title = "Phone Price per Plan",Type=ActionTypes.ImBack,Value = "Plan Prices for " + model},
                    new CardAction(){Title = "Expert Reviews",Type=ActionTypes.OpenUrl,Value = GetModelReviewsUrl( model) },
                    new CardAction(){Title = "Specifications",Type=ActionTypes.OpenUrl, Value= GetModelSpecsUrl( model) }
                }
            };
            reply.Attachments.Add(heroCard.ToAttachment());

            await context.PostAsync(reply);
            context.Wait(SelectionButtonReceivedAsync);
        }          

        private async Task CongratulateSubsAsync(IDialogContext context,string model)
        {   
            string phoneMatchMsg = "The phone match message will be inserted here";

            await context.PostAsync($"Excellent selection - The {model} is great for you because {phoneMatchMsg}. The next step is to work out what plan is the best for you");
        }

        private async Task WrongRecoverOptionReceivedAsync(IDialogContext context,IAwaitable<string> awaitable)
        {
            string brand, ans, prompt;
            string currentModel ;
            List<string> options = new List<string>() { "Yes" , "No, I don't mind"};

            context.ConversationData.TryGetValue("HandsetModelKey", out currentModel);
            brand = GetModelBrand(selectedModel);

            ans = await awaitable;
            if (ans.StartsWith("Yes"))    // subs wants to stick with the brand of the current SELECTED phone
            {
                context.Done("-" + brand);   
            }
            else if (ans == "No")   // Subs wants a different brand
                context.Done("~" + brand);
            else
                context.Wait(MessageReceivedAsync);
        }

        /* This one is just for backup */
        private async Task WrongRecoverOptionReceivedAsync2(IDialogContext context,IAwaitable<string> awaitable)
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
            string brand = GetModelBrand(selectedModel);

            
            if (debugMessages) await context.PostAsync($"DEBUG : You picked " + ans);

            if (ans.StartsWith("Ye"))  // He wants something newer
                context.Done(">" + brand);
            else if (ans.StartsWith("No"))
                context.Done("-" + brand);
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

        private string Capitalize(string str2Capitalize)
        {
            if (str2Capitalize.ToLower().StartsWith("iphone"))
                return "iP" + str2Capitalize.Substring(2);
            else
                return str2Capitalize.First().ToString().ToUpper() + str2Capitalize.Substring(1);
        }
    }
}