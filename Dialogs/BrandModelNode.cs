using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Threading.Tasks;
using System.Text;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

using MultiDialogsBot.Helper;

namespace MultiDialogsBot.Dialogs
{
    [Serializable]
    public class BrandModelNode : CommonDialog
    {
        const int MAX_FAILURES = 3;
         
        string brandChosen;
        int availableModelsCount,failureNumber = 0;
        List<string> brandModels, xclude = null;
        Dictionary<string,bool> brands;  


        public BrandModelNode(List<string> models2Exclude) : this()
        {
            xclude = models2Exclude;
            availableModelsCount -= xclude.Count;
        }

        public BrandModelNode()
        {
            availableModelsCount = GetModelCount();
        }

        public override async Task StartAsync(IDialogContext context)
        {
            if (debugMessages) await context.PostAsync("DEBUG : BrandModelNode : StartAsync()");
            await AskBrandAndModelAsync(context);
        }
          
        private async Task ChoiceMadeAsync(IDialogContext context, IAwaitable<object> awaitable)
        {
            Activity reply,ans = ((Activity)(await awaitable));
            string contents = ans.Text;  
            string model = null;
            StringBuilder errorMsg = new StringBuilder();
            string errStr = "Sorry. I don't quite follow what you're saying. Click on \"Pick Me\" if there is a phone you like, or click on \"Phone Price per Plan\" to see the cost for that phone on the different plans available";
            string errStr2 = "You can also click \"Expert reviews\" or \"Specifications\" for more details on any phone. Or if you want to go back to choose another brand or model type \"Start Again\"";

            if (contents.StartsWith("I want a "))
            {
                model = contents.Substring(9);
                if (!GetAllModels().Contains(model))
                    model = null;
            }
            else if (contents.StartsWith("Show me Plan Prices for "))
            {
                await PlanPricesButtonHandlerAsync(context, contents.Substring(24));
                return;
            }

            if (model != null)
            {
                if (debugMessages) await context.PostAsync("DEBUG: OK, you picked " + model);
                context.Done(model); 
            }
            else if ((failureNumber != 0) && (contents.ToLower() == "start again"))
            {
                reply = ans.CreateReply("OK, these are the remaining Brands that I have available, click on the one that you are interested if you know what you want just type it");
                brands.Remove(brandChosen);
                ComposeBrandsCarousel(reply);
                await context.PostAsync(reply);
                context.Wait(BrandChoiceMadeAskModelAsync);
            }
            else if (failureNumber++ < MAX_FAILURES )
            {
                reply = ((Activity)context.Activity).CreateReply(errStr);
                ++failureNumber;
                ComposeModelCarousel(brandChosen, brandModels, reply);
                await context.PostAsync(reply);
                await context.PostAsync(errStr2);
            }
            else
            {
                await context.PostAsync($"Error...too many failed attempts. Recovery from this situation is left to the Help Node");
                context.Wait(this.MessageReceivedAsync);
            }
        }

        private async Task BrandChoiceMadeAskModelAsync(IDialogContext context, IAwaitable<object> awaitable)
        {
            Activity messageActivity = (Activity)await awaitable;
            string brand = messageActivity.Text.StartsWith("I want ") ? messageActivity.Text.Substring(7) : messageActivity.Text;
            Dictionary<string, bool> tempHash;  
            List<string> brandModelsList = new List<string>();
            bool moreThanOne, unavailable;
            IEnumerable<string> vector;

            if (debugMessages) await context.PostAsync("Beginning of BrandChoiceMadeAskModelAsync()");
            brandChosen = brand;
            tempHash = GetBrandModels(brand);
            if (debugMessages) await context.PostAsync("Collecting models' hash");
            if (!(unavailable = IsBrandUnavailable(brand)) && (tempHash.Count() > 0))
            {
                vector = xclude != null ? tempHash.Keys.Except(xclude) : tempHash.Keys;
                foreach (string model in vector)
                    brandModelsList.Add(model);
                brandModels = brandModelsList;
                moreThanOne = (brandModels.Count != 1);
                brand = Miscellany.Capitalize(brand);
                var reply = messageActivity.CreateReply(moreThanOne ? "Which model would you like to have? Please pick one" : $"This is the only model I have available from {brand}");
                 
                ComposeModelCarousel(brand, brandModelsList, reply);
                await context.PostAsync(reply);
                if (debugMessages) await context.PostAsync("DEBUG : Exiting BrandChoiceMadeAskModelAsync(), brand is available on stock");
                context.Wait(ChoiceMadeAsync);
            }
            else if (unavailable)
            {
                int x = availableModelsCount;

                await context.PostAsync($"Unfortunately we don't have that brand in stock, but you can choose from over {x} plus models from Apple, Samsung, Nokia and other leading brands. Type the brands below from this list");
                if (debugMessages) await context.PostAsync("DEBUG : Exiting BrandChoiceMadeAskModelAsync(), brand is not available on stock");
                context.Wait(BrandChoiceMadeAskModelAsync);
            }
            else
            {
                await context.PostAsync("Not understood, please pick or type the brand you like");
                if (debugMessages) await context.PostAsync("DEBUG : Exiting BrandChoiceMadeAskModelAsync(), brand is not available on stock");
                context.Wait(BrandChoiceMadeAskModelAsync);
            }
        }

        private void ComposeBrandsCarousel(Activity reply)
        {
            HeroCard card;
            List<string> brandModels;
              
            foreach (string brand in brands.Keys)
            {
                brandModels = new List<string>(GetBrandModels(brand).Keys);
                if ((xclude != null) && // Avoid passing null to Except()
                    (brandModels.Except(xclude).Count() == 0))
                    continue;
                card = new HeroCard()
                {
                    Title = Miscellany.Capitalize(brand),
                    Subtitle = Miscellany.Capitalize( brand),
                    Text = "",
                    Images = new List<CardImage>() { new CardImage(handSets.GetBrandLogo(brand), "img/jpeg") },
                    Buttons = new List<CardAction>()
                    {
                        new CardAction(){Title = "Pick Me!", Type = ActionTypes.ImBack, Value = "I want " + brand},
                    }
                };
                reply.Attachments.Add(card.ToAttachment());
            }
            reply.AttachmentLayout = "carousel";
        }

        private async Task AskBrandAndModelAsync(IDialogContext context )
        {
            StringBuilder builder = new StringBuilder();
            var reply = ((Activity)context.Activity).CreateReply();
            int numberOfModels = availableModelsCount; 

            try
            {
                brands =  GetAllBrands();

                reply.Text = $"You can choose from {numberOfModels} models from Apple, Samsung, Nokia and other leading brands. Click or type the brands below from this list";
                if (debugMessages) await context.PostAsync("DEBUG : Calling ComposeBrandsCarousel()");
                ComposeBrandsCarousel(reply);
            }
            catch (Exception xception)
            {
                await context.PostAsync("xception message = " + xception.Message);  
            }

            await context.PostAsync(reply);
            context.Wait(BrandChoiceMadeAskModelAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context,IAwaitable<object> awaitable)
        {
            await context.PostAsync("BrandModelNode - The end");
            await context.PostAsync("0 OK, 0:1");
            context.Wait(MessageReceivedAsync);
        }

        private void  ComposeModelCarousel(string brand,List<string> modelsVector,Activity reply)
        {   
            HeroCard heroCard;
            string reviewsUrl;

            foreach (var model in modelsVector)   
            {
                reviewsUrl = GetModelReviewsUrl(model);
                heroCard = new HeroCard()
                {
                    Title = Miscellany.Capitalize(model),
                    Subtitle = "",
                    Text = "From " + Miscellany.Capitalize(brand),
                    Images = new List<CardImage>() {new CardImage(GetEquipmentImageURL(model,true), "img/jpeg") },
                    Buttons = new List<CardAction>
                    {
                        new CardAction() { Title = "Pick Me!", Type = ActionTypes.ImBack, Value = "I want a " + model },
                        new CardAction() { Title = "Plan Prices", Type = ActionTypes.ImBack, Value = "Show me Plan Prices for " + model},
                        new CardAction() { Title = "Specifications", Type = ActionTypes.OpenUrl, Value = GetModelSpecsUrl(model)},
                    },
                };
                if (reviewsUrl != null)
                    heroCard.Buttons.Add(new CardAction() { Title = "Reviews", Type = ActionTypes.OpenUrl, Value = GetModelReviewsUrl(model) });
                reply.Attachments.Add(heroCard.ToAttachment());
            }
            reply.AttachmentLayout = "carousel";
        }

        private bool BrandHasAtLeastOne(string brand)
        {
            Dictionary<string, bool> modelsBag = GetBrandModels(brand);

            return false;
        }
    }
}