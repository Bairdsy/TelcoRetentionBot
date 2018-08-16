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
        bool LuisCalled = false,firstTime,isUnsure = true ;
        int numberOfFailures = 0;

        public NodePhoneFlow(string input)
        {
            phraseFromSubs = input;   
        }

        public override async Task StartAsync(IDialogContext context)
        {    
            List<string> brandsWanted, modelsWanted ;
            string flowType;
            Activity reply, lastMsg = (Activity)context.Activity;
            SuggestedActions suggestedActions = new SuggestedActions
            {   
                Actions = new List<CardAction>()
                {
                    new CardAction(){Title = "Yes, I know what I want",Type=ActionTypes.ImBack,Value = "Yes"},
                    new CardAction(){Title = "No, I haven't made up my mind", Type = ActionTypes.ImBack, Value = "No"}
                }
            };

            try
            {
                if (IndicatedModelsAndOrBrands(out brandsWanted,out modelsWanted))
                {
                    await Miscellany.InsertDelayAsync(context);
                    await context.PostAsync("Yes, I can help you with that.");
                    await ProcessSelectedBrandsAndModels(context,  brandsWanted, modelsWanted,false);
                }
                else
                {
                    if (context.ConversationData.TryGetValue(BotConstants.FLOW_TYPE_KEY, out flowType))
                    {
                        await Miscellany.InsertDelayAsync(context);
                        if (flowType == BotConstants.BOTH_FLOW_TYPE)
                            await context.PostAsync("Sure. Let's start by choosing a new phone");
                        else if (flowType == BotConstants.EQUIPMENT_FLOW_TYPE)
                            await context.PostAsync("Sure. I can help you to choose a new phone.");
                        else if (flowType == BotConstants.PLAN_FLOW_TYPE)
                            await context.PostAsync("Fantastic. Let's choose a new phone for you then.");
                        else
                            throw new Exception("Error...NodePhoneFlow::StartAsync() unknown flow key value : " + flowType);  
                    }
                    else
                        throw new Exception("Error...phone flow entered without flow key being assigned a value");
                    reply = lastMsg.CreateReply("Have you decided already what you want or would you like some support in choosing what's the right phone for you?");
                    reply.SuggestedActions = suggestedActions;
                    await Miscellany.InsertDelayAsync(context);
                    await context.PostAsync(reply);
                    context.Wait(MessageReceivedWithDecisionAsync);
                }
            }
            catch (Exception xception)
            {
                await context.PostAsync(xception.Message);
            }
        }

        private async Task MessageReceivedWithDecisionAsync(IDialogContext context,IAwaitable<IMessageActivity> awaitable)
        {
            IMessageActivity messageActivity = await awaitable;
            string response = messageActivity.Text.TrimStart(' ');
            bool knowsWhatHeWants = !(response.ToLower().StartsWith("no") && ((response.Length == 2) || !char.IsLetter(response[2])));
            List<string> wantedBrands,wantedModels;
            int totalPhones = 3;

            try
            {  
                totalPhones = GetModelCount();
            }
            catch (Exception xception)
            {
                await context.PostAsync("Error...xception message = " + xception.Message);
            }   

            if (knowsWhatHeWants)
            {
                context.ConversationData.RemoveValue(BotConstants.LAST_FEATURE_KEY);
                context.ConversationData.RemoveValue(BotConstants.LAST_NEED_KEY);
                phraseFromSubs = response;  
                if (debugMessages) await context.PostAsync("DEBUG : Response received " + response);
                if (IndicatedModelsAndOrBrands(out wantedBrands, out wantedModels))
                    await ProcessSelectedBrandsAndModels(context, wantedBrands, wantedModels,false);
                else
                {
                    await Miscellany.InsertDelayAsync(context);
                    await context.PostAsync("Great to know. What model do you like the most?");
                    context.Call(new NodeLUISBegin(true), BrandAndModelReceivedAsync);
                }
            }
            else
            {
                if (debugMessages) await context.PostAsync("DEBUG : string representation : " + handSets.BuildStrRepFull());
                /*    context.ConversationData.RemoveValue(BotConstants.LAST_FEATURE_KEY);
                    context.ConversationData.RemoveValue(BotConstants.LAST_NEED_KEY);
                    await DisplayTopSalesCarouselAsync(context);
                    firstTime = false; 
                    context.Wait(PickOrRecommendOptionReceivedAsync);*/
                await AskPickOrRecommendOption(context);
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
                        builder.Append("\\x20*"  + words[wordIndex]);     // after we replace for .* for debug purposes 
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
                incompleteModel = FindPartsOfModels(spaceless, model.ToLower());
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


        private async Task BrandAndModelReceivedAsync(IDialogContext context , IAwaitable<object> awaitable)
        {
            List<string> brandsWanted = null ,
                         modelsWanted = null ;
            StringBuilder stringBuilder = new StringBuilder("Brands : ");
            Dictionary<string, bool> brandsSet = null;
            Dictionary<string, bool> modelsSet;
            Tuple<string, NodeLUISBegin.EIntent> result = (Tuple<string,NodeLUISBegin.EIntent>) (await awaitable);
            string fullSentence;  

            if (debugMessages) await context.PostAsync("Beginning of BrandAndModelReceivedAsync()");  
              
            if (result.Item2 != NodeLUISBegin.EIntent.None)
            {
                string[] temp = result.Item1.Split(':');

                fullSentence = temp[1];
                await Miscellany.InsertDelayAsync(context);
                await context.PostAsync($"You typed \"{temp[0]}\", did you mean \"{Miscellany.QueryCompare(temp[0],fullSentence)}\"?");
            }
            else
                fullSentence = result.Item1;
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

            await ProcessSelectedBrandsAndModels(context, brandsWanted, modelsWanted,true);
        }

        private async Task ProcessSelectedBrandsAndModels(IDialogContext context, List<string> wantedBrands,List<string> wantedModels, bool modelEnteredOnEnd)
        {
            StringBuilder sb  ;
            int x;
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
                await context.PostAsync("I didn’t understand you, Could you just type the specific model or brand so I can present it to you ? ");
                context.Call(new BrandModelNode(), FinalSelectionReceivedAsync);
            }  
            else if ((x = handSets.BagCount()) <= BotConstants.MAX_CAROUSEL_CARDS)
            {
                if (x > 1)
                {
                    await Miscellany.InsertDelayAsync(context);
                    if (modelEnteredOnEnd)
                        await context.PostAsync($"Great choice!There are {x} different versions for you to choose from");
                    else
                        await context.PostAsync($"There are {x} different versions for you to choose from");
                }
                context.Call(new LessThan5Node(selectResult,false), FinalSelectionReceivedAsync);
            }  
            else
            {
                await CallLuisPhoneNodeAsync(selectResult, context);
            }
        }

        private async Task CallLuisPhoneNodeAsync(List<string> basket,IDialogContext context)
        {  
            StringBuilder sb = new StringBuilder();  // Only works for debug
            IntentDecoder theDecoder;
            TopFeatures topFeatures;
            Activity reply = ((Activity)context.Activity).CreateReply("You can also at any stage ask for all phones and work through the different options on your own, just type \"Start Again\"");

            handSets.InitializeBag(basket);
            theDecoder = new IntentDecoder(handSets, null, null, basket);
            topFeatures = new TopFeatures(theDecoder);
            if (debugMessages) await context.PostAsync($"DEBUG : bag is beginning with {handSets.BagCount()}");
            if (debugMessages) await context.PostAsync("DEBUG : String Representation = " + handSets.BuildStrRep());
            if (GetModelCount() == basket.Count)
            {
                await Miscellany.InsertDelayAsync(context);
                await context.PostAsync($"We have over {basket.Count} different models of phone to choose from.");
                await Miscellany.InsertDelayAsync(context);
                await context.PostAsync("As you are unsure what is your best model then let me know what is important to you and I'll select a few for you to choose from");
                await Miscellany.InsertDelayAsync(context);
                await context.PostAsync(" If you like a particular brand just say which ones.");
                await Miscellany.InsertDelayAsync(context);
                await context.PostAsync("Or you can choose features (like weight, battery life, camera...) or just tell me how you mostly use your phone (e.g. I like to play games on my iPhone, I regularly read books on my phone)");
            }
            else
            {
                await Miscellany.InsertDelayAsync(context);
                await context.PostAsync($"We have {basket.Count} phones which match your requirement.If you tell me what is important to you or how you use your phone then I can help you to pick the right one.");
            }
            reply.SuggestedActions = topFeatures.GetTop4Buttons(sb);
            if (debugMessages) await context.PostAsync("DEBUG : Results " + sb.ToString());
            await Miscellany.InsertDelayAsync(context);
            await context.PostAsync(reply);
            context.Call(new NodeLUISPhoneDialog(topFeatures, handSets, null, null, basket), LuisResponseHandlerAsync);
            LuisCalled = true;
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
            imgURL = GetEquipmentImageURL(subsModel,false,context);
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

            context.Done(0);
            //context.Wait(MessageReceivedAsync);
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


        private async Task PickOrRecommendOptionReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> awaitable)
        {
            IMessageActivity activity = (IMessageActivity) await awaitable;
            string option = activity.Text,selectedModel;
            var totalPhones = GetModelCount();
              
            if (option.Equals("I'll pick")) 
            {
                context.Call(new BrandModelNode(), FinalSelectionReceivedAsync); 
            }
            else if (option.StartsWith("I want "))
            {
                if (debugMessages) await context.PostAsync("text received = " + option);
                selectedModel = option.Substring(7).ToLower();
                if (debugMessages) await context.PostAsync("selected model = " + selectedModel);
                context.ConversationData.SetValue("SelectedPhone", selectedModel);
                context.Call(new ColorsNode(selectedModel), MessageReceivedAsync);  
            }
            else if (option.StartsWith("Plan Prices for "))
            {
                List<CardAction> buttons; 

                selectedModel = option.Substring(16).ToLower();
                await PlanPricesButtonHandlerAsync(context, selectedModel);
                activity = ((Activity)(context.Activity)).CreateReply();
                activity.Text = $"There are {totalPhones} phones to choose from or I can help you choose one";
                buttons = new List<CardAction>()
                {
                    new CardAction(){Title = "I'll decide by myself",Type=ActionTypes.ImBack,Value = "I'll pick"},
                    new CardAction(){Title = "Help me work it out, based on what's important for me",Type= ActionTypes.ImBack,Value = "Help me work it out"}
                };
                activity.SuggestedActions = new SuggestedActions(actions: buttons);
                await context.PostAsync(activity);           
            }
            else if (option.StartsWith("Help me work it out"))
                await RecommendPhoneAsync(context, null, null);
            else if (!firstTime && option.ToLower().StartsWith("start again"))
            { 
                if (LuisCalled)
                    context.Call(new BrandModelNode( ), FinalSelectionReceivedAsync);
                else
                {
                    await CallLuisPhoneNodeAsync(GetAllModels(), context);
                }
            }
            else 
            {
                if (++numberOfFailures <= BotConstants.MAX_ATTEMPTS)
                {
                    await context.PostAsync("Sorry. I don't quite follow what you're saying. Click on \"Pick Me\" if there is a phone you like, or cliclk on \"Phone Price per Plan\" to see if the cost for that phone on the different plans available.");
                    await context.PostAsync("You can also click \"Expert Reviews\" or \"Specifications\" for more details on any phone. Or if you want to go back to choose another brand or model type \"Start Again\"");
                }
                else
                {
                    await context.PostAsync("Too many failed attempts, I'm sorry but I cannot understand what you want to do, recovery from this situation is left to Help Node");
                    context.Wait(MessageReceivedAsync);
                }
            }
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
                    Activity message = (Activity)context.Activity;
                    Activity reply = message.CreateReply(isUnsure ? "If you like a particular brand just say which one and I can display all the models available from that supplier in a carousel below" :
                                                                      "Remember that you can always ask for all phones and work through the different options on your own,just enter \"Start Again\"");
                    reply.SuggestedActions = topFeatures.GetTop4Buttons(sb);  
                    if (debugMessages) await context.PostAsync("DEBUG : " + sb.ToString());
                    await Miscellany.InsertDelayAsync(context);
                    await context.PostAsync($"We have over { count} different models of phone to choose from. ");
                    if (isUnsure)
                    {
                        await Miscellany.InsertDelayAsync(context);
                        await context.PostAsync("As you are unsure what is your best model then let me know what is important to you and I'll select a few for you to choose from. If you like a particular brand just say which ones.");
                    }
                    await Miscellany.InsertDelayAsync(context);
                    await context.PostAsync("Or you can choose features (like weight, battery life, camera...) ");
                    await Miscellany.InsertDelayAsync(context);
                    await context.PostAsync("I can also recommend some models if you tell me how you mostly use your phone (e.g. I like to play games on my iPhone, I regularly read books on my phone)");
                    await Miscellany.InsertDelayAsync(context);
                    await context.PostAsync(reply);   
                    context.Call(new NodeLUISPhoneDialog(topFeatures,handSets, brand, lowerThreshold, null), LuisResponseHandlerAsync);
                }
                else
                    context.Call(new LessThan5Node(handSets.GetBagModels(),true), FinalSelectionReceivedAsync);  
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
            bool showGreatChoiceBanner = true;

            LuisCalled = true;
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
            modelsInBag = handSets.GetBagModels();
            if (debugMessages)  if (debugMessages) await context.PostAsync($"DEBUG : bag has {modelsInBag.Count}");
            if (decoder.FeatureOrNeedDesc == "Start Again")
            {
                // context.Call(new BrandModelNode(), FinalSelectionReceivedAsync);
                await AskPickOrRecommendOption(context);
            }
            else
            {
                if (decoder.FeatureOrNeedDesc == "Show Me All")
                {
                    decoder.FeatureOrNeedDesc = null;
                    /* In this case, there is no need to write "Great Choice! ... */
                    showGreatChoiceBanner = false;
                    context.ConversationData.SetValue<List<string>>(BotConstants.SELECTED_BRANDS_KEY, new List<string>() { BotConstants.SHOW_ME_ALL });
                }
                context.Call(new LessThan5Node(modelsInBag, showGreatChoiceBanner  , decoder.LastOneWasNeed, decoder.FeatureOrNeedDesc), FinalSelectionReceivedAsync);
            }
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
         * > it's just that brand, but newer than current phone (deprecated)
         * : it's the brand and model node, he wants to explicitly pick the brand and then model
         * ~ it's everything that is not that brand 
         *     
         */
        private async Task FinalSelectionReceivedAsync(IDialogContext context, IAwaitable<object> awaitable)
        {
            string selection = (string)(await awaitable);
            List<string> models2Exclude,remainingModels;

            if (debugMessages) await context.PostAsync("DEBUG: Final selection received : " + selection);  
            switch (selection[0])  
            {  
                case LessThan5Node.SOME_OTHER_BRAND:  
                    isUnsure = false;
                    await RecommendPhoneAsync(context, "!" + selection.Substring(1));
                    break;
                case LessThan5Node.STICK_WITH_BRAND:
                    isUnsure = false;
                    await RecommendPhoneAsync(context, selection.Substring(1));
                    break;
                case LessThan5Node.NONE_OF_THESE_MODELS:     // Start again from LessThan5Node
                    await AskPickOrRecommendOption(context);
                    break;
                    models2Exclude = new List<string>(selection.Substring(1).Split(LessThan5Node.NONE_OF_THESE_MODELS));
                    if (models2Exclude.Count == GetAllModels().Count)
                    {
                        await Miscellany.InsertDelayAsync(context);
                        await context.PostAsync("Let's retry and find a phone that is suitable for your needs");
                        models2Exclude.Clear();
                    }
                    if (LuisCalled)
                        context.Call(new BrandModelNode(models2Exclude), FinalSelectionReceivedAsync);    
                    else
                    {
                        remainingModels = new List<string>( GetAllModels().Except(models2Exclude));
                        await CallLuisPhoneNodeAsync(remainingModels, context);
                    }
                    break;
                default:
                    context.Call(new ColorsNode(selection), MessageReceivedAsync);
                    break;
            }
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

        private async Task DisplayTopSalesCarouselAsync(IDialogContext context)
        {
            int x;
            string reviewsUrl;
            List<string> topSellers;
            Activity reply = ((Activity)context.Activity).CreateReply();
            Activity reply2 = ((Activity)context.Activity).CreateReply();
            HeroCard heroCard;
            List<CardAction> buttons;

            topSellers = GetTop5Sellers(); 
            x = topSellers.Count;
            reply.Text = ""; 
            foreach(var model in topSellers)
            {
                try
                {
                    heroCard = new HeroCard()
                    {
                        Title = Miscellany.Capitalize(GetModelBrand(model)),
                        Subtitle = Miscellany.Capitalize(model),
                        Text = "",
                        Images = new List<CardImage>() { new CardImage(GetEquipmentImageURL(model, true,context), "img/jpeg") },
                        Buttons = new List<CardAction>()
                    {
                        new CardAction(){Title = "Pick Me!",Type = ActionTypes.ImBack, Value ="I want " + Miscellany.Capitalize(model) /*model*/},
                        new CardAction(){Title = "Plan Prices", Type = ActionTypes.ImBack,Value = "Plan Prices for " + Miscellany.Capitalize(model) /*model*/ },
                        new CardAction (){Title = "Specifications",Type=ActionTypes.OpenUrl,Value = GetModelSpecsUrl( model) }
                    }
                    };
                }     
                catch (Exception xception)
                {
                    if (CommonDialog.debugMessages) await context.PostAsync("Error...xception message = " + xception.ToString());
                    heroCard = null;
                }
                if ((reviewsUrl = GetModelReviewsUrl(model)) != null)
                {
                    heroCard.Buttons.Add(new CardAction() { Title = "Reviews", Type = ActionTypes.OpenUrl, Value = reviewsUrl });
                }
                reply.Attachments.Add(heroCard.ToAttachment());
            }
            reply.AttachmentLayout = "carousel";
            await Miscellany.InsertDelayAsync(context);
            await context.PostAsync("Here are our latest TOP 5 sellers to choose from, select if you see anything you like");
            await Miscellany.InsertDelayAsync(context);
            await Miscellany.InsertDelayAsync(context);
            await context.PostAsync(reply);
            buttons = new List<CardAction>()
                {
                    new CardAction(){Title = "I'll decide by myself",Type=ActionTypes.ImBack,Value = "I'll pick"},
                    new CardAction(){Title = "Help me work it out, based on what's important for me",Type= ActionTypes.ImBack,Value = "Help me work it out"}
                };
            reply2.SuggestedActions = new SuggestedActions(actions: buttons);
            reply2.Text = "If you dont find anything you like, Let’s work some other options";
            await Miscellany.InsertDelayAsync(context);
            await context.PostAsync(reply2);
        }

        private async Task AskPickOrRecommendOption(IDialogContext context)
        {
            if (debugMessages) await context.PostAsync("DEBUG : Beginning of AskPickOrRecommendOption() method");
            context.ConversationData.RemoveValue(BotConstants.LAST_FEATURE_KEY);
            context.ConversationData.RemoveValue(BotConstants.LAST_NEED_KEY);
            await DisplayTopSalesCarouselAsync(context);
            firstTime = false;
            context.Wait(PickOrRecommendOptionReceivedAsync);
        }
    }

}