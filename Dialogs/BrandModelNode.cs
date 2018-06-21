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
        int failureNumber = 0;
        List<string> brandModels, xclude = null;
        Dictionary<string,bool> brands;  


        public BrandModelNode(List<string> models2Exclude)
        {
            xclude = models2Exclude;
        }

        public BrandModelNode() { }

        public override async Task StartAsync(IDialogContext context)
        {
            //Activity message;
            //Dictionary<string, bool> modelsSet;

            if (debugMessages) await context.PostAsync("DEBUG : BrandModelNode : StartAsync()");
     //       if (xclude == null)  
            await AskBrandAndModelAsync(context);
         /*   else
            {
                try
                {
                    brandModels = new List<string>();
                    brandChosen = GetModelBrand(xclude[0]);
                    brands = GetAllBrands();
                    message = ((Activity)context.Activity).CreateReply();    
                    message.Text = $"OK, these are the other models I have from the {brandChosen} brand, do you like any of them?";
                    modelsSet = GetBrandModels(brandChosen);  

                    foreach (var model in modelsSet.Keys)
                        if ( !xclude.Contains( model))  
                        {
                            brandModels.Add(model);  
                        }
                    ComposeModelCarousel(brandChosen, brandModels, message);
                    message.AttachmentLayout = "carousel";
                    await context.PostAsync(message);
                }
                catch (Exception xception)
                {
                    await context.PostAsync("Error...xception message = " + xception.Message);
                }
                context.Wait(ChoiceMadeAsync);
            }*/
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

            /*
            if (contents.StartsWith("I want a "))
            {
                model = contents.Substring(9);
                if (debugMessages) await context.PostAsync("DEBUG: OK, you picked " + model);
                context.Done(model);
            }
               
            else if ((failureNumber != 0) && (contents.ToLower() == "choose"))
            {
                var reply = ans.CreateReply("OK, these are the remaining brands that I have available, click on the one that interests you");
                brands.Remove(brandChosen);
                ComposeBrandsCarousel(reply);
                await context.PostAsync(reply);
                context.Wait(BrandChoiceMadeAskModelAsync);
            }
            else if (failureNumber++ != MAX_FAILURES)
            {
                errorMsg.Append("I'm sorry. I don't understand that. Click on \"Pick Me\" if there is a phone you like, or click on \"Plan Prices\" to see the cost");
                errorMsg.Append(" for that phone on the different plans available. You can also click \"Reviews\" or \"Sepcifications\" for more details on any");
                errorMsg.Append(" phone. Or if you want to go back to choose another brand or model type \"choose\"");
                var reply = ans.CreateReply(errorMsg.ToString());
                ComposeModelCarousel(brandChosen, brandModels, reply);
                reply.AttachmentLayout = "carousel";
                await context.PostAsync(reply);
            }
            else
            {
                await context.PostAsync($"Error...too many failed attempts. Recovery from this situation is left to the Help Node");
                context.Wait(this.MessageReceivedAsync);
            }*/
        }

        private async Task BrandChoiceMadeAskModelAsync(IDialogContext context, IAwaitable<object> awaitable)
        {
            Activity messageActivity = (Activity)await awaitable;
            string brand = messageActivity.Text.Substring(7);
            Dictionary<string, bool> tempHash;
            List<string> brandModelsList = new List<string>();
            bool moreThanOne;

            brandChosen = brand;
            tempHash = GetBrandModels(brand);
            foreach (string model in tempHash.Keys.Except(xclude))
                brandModelsList.Add(model); 
            brandModels = brandModelsList;
            moreThanOne = (brandModels.Count != 1);
            brand = Miscellany.Capitalize(brand);
            var reply = messageActivity.CreateReply(moreThanOne ? "Which model would you like to have? Please pick one" : $"This is the only model I have available from {brand}");

            ComposeModelCarousel(brand,brandModelsList, reply);
            await context.PostAsync(reply);
            context.Wait(ChoiceMadeAsync);
        }

        private void ComposeBrandsCarousel(Activity reply)
        {
            HeroCard card;
            List<string> brandModels;
              
            foreach (string brand in brands.Keys)
            {
                brandModels = new List<string>(GetBrandModels(brand).Keys);
                if (brandModels.Except(xclude).Count() == 0)
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
            int numberOfModels = GetModelCount(); 

            try
            {
                brands =  GetAllBrands();
                if (xclude != null)
                    numberOfModels -= xclude.Count;
                reply.Text = $"You can choose from {numberOfModels} models from Apple, Samsung, Nokia and other leading brands. Click or type the brands below from this list";
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
                        new CardAction() { Title = "Plan Prices", Type = ActionTypes.ImBack, Value = "Show me Plan Prices"},
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