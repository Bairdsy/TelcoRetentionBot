using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


using System.Text;
 
using System.Threading.Tasks;


using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

using MultiDialogsBot.Database;
using MultiDialogsBot.Helper;


namespace MultiDialogsBot.Dialogs
{
    [Serializable]
    public class NodePhoneFlow : CommonDialog 
    {
        string preferredBrand;
        string phraseFromSubs;

        public NodePhoneFlow(string input)
        {
            phraseFromSubs = input;   
        }

        public override async Task StartAsync(IDialogContext context)
        {    
            List<string> brandsWanted, modelsWanted ;
            Activity reply, lastMsg = (Activity)context.Activity;
            SuggestedActions suggestedActions = new SuggestedActions
            {
                Actions = new List<CardAction>()
                {
                    new CardAction(){Title = "Yes, I know what I want",Type=ActionTypes.ImBack,Value = "Yes"},
                    new CardAction(){Title = "No, I haven't made up my mind", Type = ActionTypes.ImBack, Value = "No"}
                }
            };

            await context.PostAsync("I'd really like to see if I can help you here.");
            try
            {
                if (IndicatedModelsAndOrBrands(out brandsWanted,out modelsWanted))
                {
                    await ProcessSelectedBrandsAndModels(context,  brandsWanted, modelsWanted);
                }
                else
                {
                    reply = lastMsg.CreateReply("Have you decided already what you want or would you like some support in choosing what's the right phone for you?");
                    reply.SuggestedActions = suggestedActions;
                    // PromptDialog.Choice(context, MessageReceivedWithDecisionAsync, options, "Do you know what make and model you want?", "Did not quite follow that, could you please repeat?", 5);
                    await context.PostAsync(reply);
                    context.Wait(MessageReceivedWithDecisionAsync);
                }
                 //   PromptDialog.Confirm(context, MessageReceivedWithDecisionAsync, "Do you know what make and model you want?", "Didn't quite follow that, could you please repeat?", promptStyle: PromptStyle.Keyboard);
            }
            catch (Exception xception)
            {
                await context.PostAsync(xception.Message);
            }
        }

        private async Task MessageReceivedWithDecisionAsync(IDialogContext context,IAwaitable<IMessageActivity> awaitable)
        {
            IMessageActivity messageActivity = await awaitable;
            string response = messageActivity.Text;
            bool hasDecided = response.ToLower().Contains("yes");
            List<string> options,wantedBrands,wantedModels;
            int totalPhones = 3;

            try
            {
                totalPhones = GetModelCount();
            }
            catch (Exception xception)
            {
                await context.PostAsync("Error...xception message = " + xception.Message);
            }   

            if (hasDecided)
            {   
                phraseFromSubs = response;
                if (debugMessages) await context.PostAsync("DEBUG : Response received " + response);
                if (IndicatedModelsAndOrBrands(out wantedBrands, out wantedModels))
                    await ProcessSelectedBrandsAndModels(context, wantedBrands, wantedModels);
                else
                {
                    await context.PostAsync("Great to know. What model do you like the most?");
                    context.Wait(BrandAndModelReceived);
                }
            }
            else
            {
                if (debugMessages) await context.PostAsync("DEBUG : string representation : " + handSets.BuildStrRepFull());
                options = new List<string>() { "I'll pick", "Help me work it out" };
                PromptDialog.Choice(
                    context,
                    PickOrRecommendOptionReceivedAsync,
                    options,
                    $"OK. Do you want to pick a phone from a list of everything ({totalPhones} models) , or should I try to recommend a few for you?",
                    "Sorry, not a valid option",
                    4);
            }
        }

        string RemoveSpaces(string strWithSpaces)
        {
            string[] words;
            StringBuilder strWithoutSpaces = new StringBuilder();

            words = strWithSpaces.Split(' ');
            for (int i = 0; i < words.Length; ++i)
                strWithoutSpaces.Append(words[i]);
            return strWithoutSpaces.ToString();
        }

        string FindPartsOfModels(string str2BeSearched,string multiWord)
        {
            string[] words = multiWord.Split(' ');
            StringBuilder builder = new StringBuilder();
            int index,startIndex = 0,wordIndex,maxWordIndex = -1;

            do
            {
                for (wordIndex = 0; wordIndex < words.Length; ++wordIndex)
                {
                    if (wordIndex == 0)
                        index = str2BeSearched.IndexOf(words[wordIndex].ToLower(), startIndex);
                    else
                        if (str2BeSearched.Substring(startIndex).StartsWith(words[wordIndex].ToLower()))
                        {
                            index = startIndex;
                        }
                        else
                            index = -1;
                    if (index == -1)
                        break;
                    if (maxWordIndex < wordIndex)
                    {
                        builder.Append("\\x20*"  + words[wordIndex]);     // Depois substitutir por .x for debug purposes 
                        maxWordIndex = wordIndex;
                    }
                    startIndex = index + words[wordIndex].Length;
                }
            }
            while (wordIndex != 0);
         
            return builder.Length != 0 ? ".*" + builder.ToString() : string.Empty;
        }


        void MergeIntoList(List<StringBuilder> modelsInc,string model2mergeIntoList)
        {
            int len = model2mergeIntoList.Length;

            model2mergeIntoList = model2mergeIntoList.ToLower();// Oliver = night
            foreach (StringBuilder model in modelsInc)
            {
                string copy = model.ToString().ToLower();  // Oliver = night
                if (copy.Length > len)
                {
                    if (copy.StartsWith(model2mergeIntoList)) // if Contains
                        return;                               // that's coz it's already there, nothing left to do
                }
                else
                    if (model2mergeIntoList.StartsWith(copy))
                    {
                        model.Append(model2mergeIntoList.Substring(copy.Length));
                        return;
                    }
            }
            // doesn't contain nor is contained by any of them ? Then it's something new. Let's add it
            modelsInc.Add(new StringBuilder(model2mergeIntoList));
        }
        
        List<string> FindModelOccurrences(string fullUtterance, Dictionary<string,bool> modelsSet)
        {
            List<string> returnVal = new List<string>();
            string spaceless = RemoveSpaces(fullUtterance.ToLower());
            List<StringBuilder> detectedModels = new List<StringBuilder>();
            string incompleteModel;

            foreach (string model in modelsSet.Keys)
            {
                incompleteModel = FindPartsOfModels(spaceless, model.ToLower());    // Oliver = night

                if (incompleteModel.Length != 0)  // Empty string means nothing is there
                {
                    MergeIntoList(detectedModels, incompleteModel);
                }
            }
    
            for (int i = 0; i < detectedModels.Count; ++i)
                returnVal.Add(detectedModels[i].Append(".*").ToString());
            return returnVal;
        }

        List<string> FindBrandOccurrences(string fullUtterance, Dictionary<string,bool> brands)
        {
            List<string> keys = new List<string>(),returnStrs = new List<string>();

            
            fullUtterance = fullUtterance.ToLower();

            foreach (string brand in brands.Keys)
                keys.Add(brand);

            foreach (string brand in keys)
                if (-1 != fullUtterance.IndexOf(brand.ToLower()))
                {
                    returnStrs.Add(brand);
                }
            return returnStrs;
        }


        private async Task BrandAndModelReceived(IDialogContext context , IAwaitable<object> awaitable)
        {
            List<string> brandsWanted = null ,
                         modelsWanted = null ;
            StringBuilder stringBuilder = new StringBuilder("Brands : ");
            Dictionary<string, bool> brandsSet = null;
            Dictionary<string, bool> modelsSet;

            string fullSentence = ((Activity)(await awaitable)).Text;
            try
            {
                brandsSet = GetAllBrands();
            }
            catch (Exception xception)
            {
                await context.PostAsync("Error... xception message = " + xception.Message);
            }
            modelsSet = GetBrandModels(null);
            try
            {
                brandsWanted = FindBrandOccurrences(fullSentence, brandsSet);
                modelsWanted = FindModelOccurrences(fullSentence, modelsSet);
            }
            catch (Exception xception)
            {
                await context.PostAsync($"Exception message = {xception.Message}");
            }

            await ProcessSelectedBrandsAndModels(context, brandsWanted, modelsWanted);
        }

        private async Task ProcessSelectedBrandsAndModels(IDialogContext context, List<string> wantedBrands,List<string> wantedModels)
        {
            StringBuilder sb  ;
            TopFeatures topFeatures;
            IntentDecoder theDecoder;
            List<string> selectResult;

            sb = new StringBuilder("DEBUG : Brands indicated : ");
            foreach (string brand in wantedBrands)
                sb.Append("-->" + brand + "\r\n");
            if (debugMessages) await context.PostAsync("DEBUG : brands identified : " + sb.ToString());
            sb = new StringBuilder("DEBUG : Models indicated : ");
            foreach (string model in wantedModels)
                sb.Append("-->" + model + "\r\n");
            if (debugMessages) await context.PostAsync("DEBUG : models identified  : " + sb.ToString());
            selectResult = SelectWithFilter(wantedModels);
            sb = new StringBuilder("DEBUG : Models selected by filter: ");
            foreach (string model in selectResult)
                sb.Append("-->" + model + "\r\n");
            if (debugMessages) await context.PostAsync("DEBUG : models selected with regex : " + sb.ToString());
            AddUncoveredBrands(wantedBrands, selectResult);
            sb = new StringBuilder("DEBUG : Models selected including uncovered brands: ");
            foreach (string model in selectResult)
                sb.Append("-->" + model + "\r\n");
            if (debugMessages) await context.PostAsync("DEBUG : models identified with uncovered brands : " + sb.ToString());

            handSets.InitializeBag(selectResult);

            if (debugMessages) await context.PostAsync("DEBUG : contents of bag : " + handSets.BuildStrRep());

            if (selectResult.Count == 0)
            {
                await context.PostAsync("Sorry I got that wrong, could you just type the specific model and brand so I can show it to you?");
                context.Call(new BrandModelNode(), MessageReceivedAsync);
            }
            else if (handSets.BagCount() < 5)
            {
                context.Call(new LessThan5Node(selectResult), FinalSelectionReceivedAsync);
            }
            else
            {
                Activity reply = ((Activity)context.Activity).CreateReply("What is the most important thing for you on a phone?");

                await context.PostAsync($"I have quite a few equipments on my list that match your brand and model, specifically {selectResult.Count}");
                await context.PostAsync("As such, I would like to narrow it down a little bit for you, so allow me to ask you");
                theDecoder = new IntentDecoder(handSets, null, null, selectResult);
                topFeatures = new TopFeatures(theDecoder);
                reply.SuggestedActions = topFeatures.GetTop4Buttons(sb);
                await context.PostAsync(reply);

                if (debugMessages) await context.PostAsync($"DEBUG : bag is beginning with {handSets.BagCount()}");
                if (debugMessages) await context.PostAsync("DEBUG : String Representation = " + handSets.BuildStrRep());
                context.Call(new NodeLUISPhoneDialog(topFeatures,handSets, null, null, selectResult), LuisResponseHandlerAsync);     
            }
        }

        private async Task ShowCurrentPhoneAsync(IDialogContext context)
        {
            string imgURL;
            string subsBrand = null, subsModel;
            var reply = ((Activity)context.Activity).CreateReply();
            HeroCard heroCard;
 
            if (  !context.ConversationData.TryGetValue("HandsetModelKey", out subsModel))
            {
                await context.PostAsync("I don't know what to do here, one of them is missing...");  /* Should never happen */
                return;
            }
            /* For now , let's pretend user has a iphone 7 plus- 256gb */
            subsModel = "iphone 7 plus- 256gb";
            try
            {
                preferredBrand = subsBrand = GetModelBrand(subsModel);
            }
            catch (Exception xception)
            {
                await context.PostAsync("Error in GetModelBrand(), exception message= " + xception.Message);    
            }
            imgURL = GetEquipmentImageURL(subsModel,false);
            heroCard = new HeroCard()
            {
                Title = subsBrand,
                Subtitle = subsModel,
                Text = "Please select one of the buttons below",
                Images = new List<CardImage>() { new CardImage(imgURL,"img/jpeg") },
                Buttons = new List<CardAction>()
                    {
                        new CardAction(){ Title = "Yes", Type=ActionTypes.ImBack, Value="Yes" },
                        new CardAction(){ Title = $"No, I'm not happy with {subsBrand}", Type=ActionTypes.ImBack, Value="No" },
                        new CardAction(){ Title = "No, I don't care", Type=ActionTypes.ImBack,Value = "It doesn't matter"}
                    }
            };

            reply.Text = $"I see you have a {subsModel}, would you like to stick with {subsBrand}?"  ;
            reply.Attachments.Add(heroCard.ToAttachment());
            await context.PostAsync(reply);
            context.Wait(ChangeBrandAnswerReceived);
        }


        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> awaitable)
        {
            Activity messageReceived = ((Activity) await awaitable);

            await context.PostAsync("NodePhoneFlow - The End");

            await context.PostAsync(  "0 OK, 0:1");
            context.Wait(MessageReceivedAsync);
        }

        private void AddUncoveredBrands(List<string> brands, List<string> models)
        {
            Dictionary<string, bool> unkBrandModels,brandSet = new Dictionary<string, bool>();

            foreach (var brand in brands)
                brandSet.Add(brand,false);
            if (brandSet.Count == 0)
                return;

            foreach (var model in models)
            {
                string modelBrand = GetModelBrand(model);
                if (brandSet.ContainsKey(modelBrand))
                    brandSet[modelBrand] = true;
            }

            foreach (var brand in brandSet.Keys)
                if (!brandSet[brand])
                {
                    unkBrandModels = GetBrandModels(brand);
                    foreach (var model in unkBrandModels.Keys)
                        models.Add(model);
                }
        }


        private async Task PickOrRecommendOptionReceivedAsync(IDialogContext context, IAwaitable<string> awaitable)
        {
            string option = await awaitable;

            if (option.Equals("I'll pick"))
            {
                context.Call(new BrandModelNode(), FinalSelectionReceivedAsync);
            }
            else
                await ShowCurrentPhoneAsync(context);
        }

        private async Task ChangeBrandAnswerReceived(IDialogContext context,IAwaitable<object> awaitable)
        {
            Activity activity = (await awaitable) as Activity;
            string ans = activity.Text;
            string subsModel, subsBrand;
            List<string> opt = new List<string>() { "Yes", "No, I don't mind" };       
                
            context.ConversationData.TryGetValue("HandsetModelKey", out subsModel);
            /* For now     it will be 7 plus 256 */      
            subsModel = "iphone 7 plus- 256gb";
            subsBrand = GetModelBrand(subsModel);
            if (ans.ToLower().Equals("yes"))   // He wants to stick with the brand
            {
                PromptDialog.Choice(context, WantsNewerMsgReceivedAsync, opt, "Great! And are you looking for something newer than what you have now?", "Not Understood, please try again", 4);
            }
            else
            {
                if (ans.ToLower().Equals("no"))  // He wants to choose some other brand
                {
                    preferredBrand = "!" + preferredBrand;
                    if (debugMessages) await context.PostAsync("DEBUG : He wants to see everything but his or her current brand");
                    await RecommendPhoneAsync(context, "!" + subsBrand, null);
                }
                else
                {
                    preferredBrand = null;  // s/he doesnt have a preference for the brand
                    if (debugMessages) await context.PostAsync("DEBUG : He doesn't really care, he wants to see everything");
                    await RecommendPhoneAsync(context, null, null);
                }
            }
        }

        private async Task RecommendPhoneAsync(IDialogContext context,string brand,DateTime? lowerThreshold = null)
        {
            int count;
            TopFeatures topFeatures ;
            IntentDecoder theDecoder = new IntentDecoder(handSets, brand, lowerThreshold, null);

            StringBuilder sb = new StringBuilder("");   // For debugging purposes

            try
            {
                topFeatures = new TopFeatures(theDecoder);    
                handSets.InitializeBag(brand, lowerThreshold);
                count = handSets.BagCount();
                if (count > BotConstants.MAX_CAROUSEL_CARDS)
                {
                    if (debugMessages)  if (debugMessages) await context.PostAsync($"DEBUG : bag is beginning with {handSets.BagCount()}");
                    if (debugMessages) await context.PostAsync("DEBUG : String Representation = " + handSets.BuildStrRep());
                    await context.PostAsync($"OK, I have {count} available and I'm trying to work out the right one for you");
                    Activity message = (Activity)context.Activity;
                    Activity reply = message.CreateReply("Name one, the most important thing on your decision?");
                    reply.SuggestedActions = topFeatures.GetTop4Buttons(sb);
                    if (debugMessages) await context.PostAsync("DEBUG : " + sb.ToString());
                    await context.PostAsync(reply);
                    context.Call(new NodeLUISPhoneDialog(topFeatures,handSets, brand, lowerThreshold, null), LuisResponseHandlerAsync);
                }
                else
                    context.Call(new LessThan5Node(handSets.GetBagModels()), FinalSelectionReceivedAsync);
            }
            catch (Exception xception )
            {
                if (debugMessages) await context.PostAsync("DEBUG : Message from the exception : " + xception.Message + "\r\nDEBUG : string builder : " + sb.ToString());

            }
        }

        private async Task LuisResponseHandlerAsync(IDialogContext context,IAwaitable<object> awaitable)
        {
            List<string> modelsInBag;
            int handSetsLeft= 4;
            IntentDecoder decoder = null;

            try
            {
                decoder = (IntentDecoder)(await awaitable);
                handSets = (HandSets)decoder.PhonesLeft;
                handSetsLeft = handSets.BagCount();
            }
            catch (Exception xception)
            {
                if (debugMessages) await context.PostAsync("DEBUG : xception message " + xception.Message);
            }
            if (handSetsLeft <= BotConstants.MAX_CAROUSEL_CARDS)   // It's narrowed down enough
            {
                modelsInBag = handSets.GetBagModels();
                if (debugMessages)  if (debugMessages) await context.PostAsync($"DEBUG : bag has {modelsInBag.Count}");
                context.Call(new LessThan5Node(modelsInBag), FinalSelectionReceivedAsync);
            }  
            else
            {
                context.Call(new KnockOutRecommendationsNode(decoder), DoneNarrowingFurtherAsync);
            }
        }

        private async Task DoneNarrowingFurtherAsync(IDialogContext context,IAwaitable<object> awaitable)
        {
            IntentDecoder decoder;
            List<string> vector;

            if (debugMessages) await context.PostAsync("DEBUG : DoneNarrowingFurtherAsync()");

            decoder = (IntentDecoder)(await awaitable);
            handSets = decoder.PhonesLeft;
            vector = handSets.GetBagModels();
            context.Call(new LessThan5Node(vector), FinalSelectionReceivedAsync);
        }

        private async Task WantsNewerMsgReceivedAsync(IDialogContext context,IAwaitable<string> awaitable)
        {
            string ans,subsBrand,subsModel;
            context.ConversationData.TryGetValue("HandsetModelKey", out subsModel);
 //           subsModel = "iphone 7 plus- 256gb";
            subsBrand = GetModelBrand(subsModel);

            if (debugMessages) await context.PostAsync("DEBUG : Brand obtained : " + subsBrand);
            ans = await awaitable;      

            if (ans.ToLower() != "yes")                                         // Anything goes
                await RecommendPhoneAsync(context, subsBrand);
            else
                await RecommendPhoneAsync(context, subsBrand, GetModelReleaseDate(subsModel));
                //context.Wait(MessageReceivedAsync);
        }

        /*
         * - it's just that brand
         * > it's just that brand, but newer than current phone
         * ~ it's everything that is not that brand 
         * 
         */
        private async Task FinalSelectionReceivedAsync(IDialogContext context, IAwaitable<object> awaitable)
        {
            string selection = (string)(await awaitable);
            string subsModel;


            if (debugMessages) await context.PostAsync("DEBUG: Final selection received : " + selection);
            switch (selection[0])
            {
                case '~':
                    await RecommendPhoneAsync(context, "!" + selection.Substring(1));
                    break;
                case '-':
                    await RecommendPhoneAsync(context, selection.Substring(1));
                    break;
                case '>':
                    if (!context.ConversationData.TryGetValue("HandsetModelKey", out subsModel))
                        throw new Exception("Error...HandsetModelKey not present in conversation data");
                    await RecommendPhoneAsync(context, selection.Substring(1), GetModelReleaseDate(subsModel));
                    break;
                default:
                    context.Call(new ColorsNode(selection), MessageReceivedAsync);
                    break;
            }
        }

        private async Task FinalSelectionReceivedAsync2(IDialogContext context,IAwaitable<object> awaitable)
        {
            string selection = (string)(await awaitable);


            if (debugMessages) await context.PostAsync("DEBUG: Final selection received : " + selection);
            if (selection[0] == '~') /* subscriber doesn't want the phone we narrowed down to */
            {
                if (selection.Length > 1)
                    context.Call(new BrandModelNode(new List<string>(selection.Substring(1).Split('~'))),FinalSelectionReceivedAsync);
                else
                    context.Call(new BrandModelNode(), FinalSelectionReceivedAsync);
            }

            else
                context.Call(new ColorsNode(selection), MessageReceivedAsync);
        }

        private bool IndicatedModelsAndOrBrands(out List<string> wantedBrands,out List<string> wantedModels)
        {
            Dictionary<string, bool> modelSet, brandSet;
            List<string> brandsDetected, modelsDetected;

            brandSet = GetAllBrands();
            modelSet = GetBrandModels(null);
            brandsDetected = FindBrandOccurrences(phraseFromSubs, brandSet);
            modelsDetected = FindModelOccurrences(phraseFromSubs, modelSet);
            wantedBrands = brandsDetected;
            wantedModels = modelsDetected;
            return (modelsDetected.Count != 0) || (brandsDetected.Count != 0);
        }
    }

}