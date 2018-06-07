using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Threading.Tasks;
using System.Text;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

using AdaptiveCards;

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
            Activity message;
            Dictionary<string, bool> modelsSet;

            if (debugMessages) await context.PostAsync("DEBUG : BrandModelNode : StartAsync()");

            if (xclude == null)  
                await AskBrandAndModelAsync(context);
            else
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
            }
        }

        private async Task ChoiceMadeAsync(IDialogContext context, IAwaitable<object> awaitable)
        {
            Activity ans = ((Activity)(await awaitable));
            string contents = ans.Text;
            string model;
            StringBuilder errorMsg = new StringBuilder();

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
            }
        }

        private async Task BrandChoiceMadeAskModelAsync(IDialogContext context, IAwaitable<object> awaitable)
        {
            Activity messageActivity = (Activity)await awaitable;
            string brand = messageActivity.Text.Substring(7);
            Dictionary<string, bool> tempHash;
            List<string> brandModelsList = new List<string>();


            brandChosen = brand;
            tempHash = GetBrandModels(brand);
            foreach (string model in tempHash.Keys)
                brandModelsList.Add(model);
            brandModels = brandModelsList;

            var reply = messageActivity.CreateReply("Which model would you like to have? Please pick one");
            reply.AttachmentLayout = "carousel";
            ComposeModelCarousel(brand,brandModelsList, reply);
            await context.PostAsync(reply);
            context.Wait(ChoiceMadeAsync);
        }

        private void ComposeBrandsCarousel(Activity reply)
        {
            HeroCard card;

            
            foreach (string brand in brands.Keys)
            {
                card = new HeroCard()
                {
                    Title = brand,
                    Subtitle = brand,
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
            List<string> listOfBrands = new List<string>();
            Dictionary<string, bool> brandSet;   
            StringBuilder builder = new StringBuilder();
            var reply = ((Activity)context.Activity).CreateReply();

            
            try
            {
                brands = brandSet = GetAllBrands();
          
                reply.Text = "OK, This is the whole list of Brands that I have available, click on the one that you are interested : ";
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

            foreach (var model in modelsVector)
            {
                heroCard = new HeroCard()
                {
                    Title = model,
                    Subtitle = "",
                    Text = "From " + brand,
                    Images = new List<CardImage>() { new CardImage(GetEquipmentImageURL(model), "img/jpeg"), new CardImage(GetEquipmentImageURL(model), "img/jpeg") },
                    Buttons = new List<CardAction>
                    {
                        new CardAction() { Title = "Pick Me!", Type = ActionTypes.ImBack, Value = "I want a " + model },
                        new CardAction() { Title = "Plan Prices", Type = ActionTypes.ImBack, Value = "Show me Plan Prices"},
                        new CardAction() { Title = "Reviews", Type = ActionTypes.ImBack, Value = "Show me the Reviews"},
                        new CardAction() { Title = "Specifications", Type = ActionTypes.ImBack, Value = "Show me Specifications"},
                    },
                };
                reply.Attachments.Add(heroCard.ToAttachment());
            }
        }
    }
}