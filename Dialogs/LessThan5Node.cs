using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

using MultiDialogsBot.Database;
using MultiDialogsBot.Helper;  


namespace MultiDialogsBot.Dialogs
{
    [Serializable]
    public class LessThan5Node : CommonDialog
    {
        public const int MAX_NUMBER_OF_TIMES_RUBBISH_ENTERED = 3;
        public const char STICK_WITH_BRAND = '-';  // Wants to stick with the same brand
        public const char SOME_OTHER_BRAND = '~';  // Doesn't want to continue with that brand
        public const char NONE_OF_THESE_MODELS = ':';

        List<string> modelList;
        string selectedModel,needIntent,featureIntent;
        bool weAreOnBranch7,firstTime = true, answerWasFeature;
        int numTimesRubbishEntered = 0;

        public LessThan5Node(List<string> models2Show, bool insideBranch7,bool isNeed = false,string text = null)
        {
            modelList = models2Show;
            weAreOnBranch7 = insideBranch7;

            if (insideBranch7)
            {
                answerWasFeature = !isNeed;
                if (isNeed)
                    needIntent = text;
                else
                    featureIntent = text;
            }
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
            string modelPicked,unwantedModels,buttonPressed = msg.Text ;
            string[] models2Exclude;

            if (buttonPressed.StartsWith("I want "))
            {
                modelPicked = selectedModel = buttonPressed.Substring(7);      
                context.Done(modelPicked);
            }    
            else if (buttonPressed.ToLower() == "start again")
            {
                models2Exclude = modelList.ToArray();
                unwantedModels = string.Concat(NONE_OF_THESE_MODELS, string.Join(NONE_OF_THESE_MODELS.ToString(), models2Exclude));
                context.Done(unwantedModels);  
            }
            else if (buttonPressed.StartsWith("Plan Prices for "))
            {
                modelPicked = buttonPressed.Substring(16);
                await PlanPricesButtonHandlerAsync(context, modelPicked);
            }
            else  // Anything else 
            {
                if (++numTimesRubbishEntered <= MAX_NUMBER_OF_TIMES_RUBBISH_ENTERED)
                {  
                    await context.PostAsync("Sorry. I don't quite follow what you're saying. Click on \"Pick Me\" if there is a phone you like, or click on \"Phone Price per Plan\" to see the cost for that phone on the different plans available.");
                    await context.PostAsync("You can also click \"Expert reviews\" or \"Specifications\" for more details on any phone. Or if you want to go back to choose another brand or model type \"Start Again\"");
                    await DisplayMultiPhoneCarouselAnsyc(context, modelList);
                }
                else
                {
                    await context.PostAsync("TODO : Too many failed attempts. recovery from this situation is left to the HELP node");
                    context.Wait(MessageReceivedAsync);
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
            else if (buttonPressed.StartsWith("Plan Prices for "))
            {
                model = buttonPressed.Substring(16);  
                await PlanPricesButtonHandlerAsync(context, model);
            }
            else if (buttonPressed.StartsWith("No"))
            {
                string brandLower;
                string brand = Miscellany.Capitalize(brandLower = GetModelBrand(this.selectedModel));
                int numberOfModels = GetBrandModels(brandLower).Count;

                if (numberOfModels > 1)
                {
                    options = new List<string>() { $"Yes, I want to stay with {brand}", "No" };
                    PromptDialog.Choice(context,
                        WrongRecoverOptionReceivedAsync,
                        options,
                        $"Do you still want to look at {brand} range or look at other phones?",
                        "Not understood, please try again",
                        4);
                }
                else  // If it is just one, there is really no point in asking if he wants to stick with the same brand...
                    context.Done(SOME_OTHER_BRAND + brandLower);   
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
            string reviewsUrl;
            var reply = ((Activity)context.Activity).CreateReply();
            HeroCard heroCard;
            int x = modelList.Count;
            List<string> brands;
            List<Tuple<HeroCard,HandSetFeatures>> heroCards = new List<Tuple<HeroCard,HandSetFeatures>>();

             if (firstTime)
            {
                if (!answerWasFeature)
                {
                    await context.PostAsync($"Great, {needIntent} , Here are our TOP {x} models to choose from. Or let's look at some other options, please type \"Start again\"");
                }
                else
                {
                    if (context.ConversationData.TryGetValue<List<string>>(BotConstants.SELECTED_BRANDS_KEY, out brands))
                    {
 
                        await context.PostAsync($"Great, here are the {Miscellany.BuildBrandString(brands)} models that currently I have to choose from");
                    }
                    else
                    {
                        await context.PostAsync($"Great, here are the TOP {x} models {(featureIntent != null ? "for " + featureIntent : "")} to choose from.");
                    }
                    await context.PostAsync("Or let's work out some other options if you are not happy with these ones, please type \"Start again\"");
                }
                firstTime = false;
            }
            reply.AttachmentLayout = "carousel"; 
            foreach (var model in models)
            {
                heroCard = new HeroCard()
                {
                    Title = Miscellany.Capitalize(GetModelBrand(model)),
                    Subtitle = Miscellany.Capitalize(model),    
                    Text = "",
                    Images = new List<CardImage>() { new CardImage(GetEquipmentImageURL(model,true,context), "img/jpeg") },
                    Buttons = new List<CardAction>()
                    {
                        new CardAction(){Title = "Pick Me!",Type = ActionTypes.ImBack, Value ="I want " + model},
                        new CardAction(){Title = "Plan Prices", Type = ActionTypes.ImBack,Value = "Plan Prices for " + model },   
                        new CardAction (){Title = "Specifications",Type=ActionTypes.OpenUrl,Value = GetModelSpecsUrl( model) }
                    }
                };
                
                if ((reviewsUrl = GetModelReviewsUrl(model)) != null)
                {
                    heroCard.Buttons.Add(new CardAction() { Title = "Expert Reviews", Type = ActionTypes.OpenUrl, Value = reviewsUrl });
                }
                heroCards.Add(new Tuple<HeroCard,HandSetFeatures>(heroCard,handSets.GetModelFeatures(model)));
            }
            Miscellany.SortCarousel(heroCards);
            for (int n = 0; n < heroCards.Count; ++n)
                reply.Attachments.Add(heroCards[n].Item1.ToAttachment());
            await context.PostAsync(reply);
            context.Wait(CarouselSelectionButtonReceivedAsync);
        }

        private async Task DisplaySinglePhoneCardAsync(IDialogContext context, string model)
        {
            string equipmentURL,reviewsUrl;
            var reply = ((Activity)context.Activity).CreateReply();
            HeroCard heroCard;

            this.selectedModel = model;
            if (!weAreOnBranch7)
            {
                await Miscellany.InsertDelayAsync(context);
                await context.PostAsync("Great. Let's have a look at that phone now.");
            }
            else
            {
                await Miscellany.InsertDelayAsync(context);
                await context.PostAsync("Great. Based on what you told me, I've narrowed it down to this recommended model");
            }
            equipmentURL = GetEquipmentImageURL(model,true,context);    
            heroCard = new HeroCard()      
            {
                Title = Miscellany.Capitalize(GetModelBrand(model)),
                Subtitle = Miscellany.Capitalize(model),
                Text = weAreOnBranch7 ? "Select from one of the buttons below" : "Click one of the buttons to continue.",
                Images = new List<CardImage> { new CardImage(equipmentURL, "img/jpeg") },
                Buttons = new List<CardAction>()
                {
                    new CardAction(){Title = weAreOnBranch7 ? "Yes - great choice" : "Yes - that's the phone I like", Type=ActionTypes.ImBack, Value = "I want " + model},
                    new CardAction(){Title = weAreOnBranch7 ? "No. That's not quite right" : "No. I am after a different model",Type = ActionTypes.ImBack,Value = "No. I am after a different model"},
                    new CardAction(){Title = "Phone Price per Plan",Type=ActionTypes.ImBack,Value = "Plan Prices for " + model},
                    new CardAction(){Title = "Specifications",Type=ActionTypes.OpenUrl, Value= GetModelSpecsUrl( model) }
                }
            };
            reviewsUrl = GetModelReviewsUrl(model);
            if (reviewsUrl != null)
                heroCard.Buttons.Add(new CardAction() { Title = "Expert Reviews", Type = ActionTypes.OpenUrl, Value = reviewsUrl });
            reply.Attachments.Add(heroCard.ToAttachment());

            await context.PostAsync(reply);
            context.Wait(SelectionButtonReceivedAsync);
        }          

        private async Task CongratulateSubsAsync(IDialogContext context,string model)
        {   
            string phoneMatchMsg = "The phone match message will be inserted here";

            await context.PostAsync($"Excellent selection - The {Miscellany.Capitalize(model)} is great for you because {phoneMatchMsg}. The next step is to work out what plan is the best for you");
        }

        private async Task WrongRecoverOptionReceivedAsync(IDialogContext context,IAwaitable<string> awaitable)
        {
            string brand, ans;
            string currentModel ;
            List<string> options = new List<string>() { "Yes" , "No, I don't mind"};

            context.ConversationData.TryGetValue("HandsetModelKey", out currentModel);
            brand = GetModelBrand(selectedModel);

            ans = await awaitable;
            if (ans.StartsWith("Yes"))    // subs wants to stick with the brand of the current SELECTED phone
            {
                context.Done(STICK_WITH_BRAND + brand);   
            }
            else if (ans == "No")   // Subs wants a different brand
                context.Done(SOME_OTHER_BRAND + brand);
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

        private bool AllBrandsModelsIncluded(List<string> brands,int listLen)
        {
            int counter = 0;

            foreach (var brand in brands)
                counter += GetBrandModels(brand).Count;
            if (counter < listLen)
                throw new Exception("Error...more models in carousel than the available in the brands subs chose");
            return listLen == counter;
        }


    }
}