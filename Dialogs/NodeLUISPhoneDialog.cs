using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

using MultiDialogsBot.Helper;
using MultiDialogsBot.Database;


namespace MultiDialogsBot.Dialogs
{
    [LuisModel("f245439f-5379-464a-8481-e68985e4504b", "99127c285bd3420aa9d9f460091b7683", LuisApiVersion.V2, null, SpellCheck = true, Verbose = true)]
    [Serializable]
    public class NodeLUISPhoneDialog : LuisDialog<object>
    {
        readonly Dictionary<EIntents, string> smallDesc;

        public enum EIntents
        {
            None = 0,
            BandWidth,
            BatteryLife,
            Brand,
            Camera,
            Cheap,
            Color,
            DualCamera,
            DualSIM, 
            ExpandableMemory,   
            FMRadio,
            FaceID,
            GPS,
            HDVoice,
            HighResDisplay,
            LargeStorage,
            OS,
            ScreenSize,
            SecondaryCamera,
            Small,
            WaterResist,
            Weight,
            WiFi,
            Newest,
            RAM
        }

        private enum EKeywords
        {
            None = 0,
            ShowMeAll,
            StartAgain
        }

        string brandDesired;
        DateTime? ReleaseDateCurrentModel;
        IntentDecoder decoder;
        HandSets handSetsBag;
        TopFeatures topButtons;

        EIntents desiredFeature;
        double desiredFeatureScore;
        LuisResult res;

        ScoreFuncs needsScores;

        public NodeLUISPhoneDialog(TopFeatures mostDemanded,HandSets handSets, string brand, DateTime? currentModelReleaseDate,List<string> narrowedListOfModels) : base()
        {
            smallDesc = new Dictionary<EIntents, string>()
            {
                {EIntents.BatteryLife,"Battery life" },
                {EIntents.Small, "Physical Size" }
            };

            handSetsBag = handSets;
            brandDesired = brand;
            ReleaseDateCurrentModel = currentModelReleaseDate;
            decoder = mostDemanded.AssociatedDecoder; 
            topButtons = mostDemanded;
            needsScores = new ScoreFuncs(handSets);
        }

        [LuisIntent("None")]
        public async Task None(IDialogContext context,LuisResult result)  
        {
            await context.PostAsync("Not understood");
            await ShowDebugInfoAsync(context, result);
            if (CommonDialog.debugMessages) await context.PostAsync("I could not understand that, I'm afraid");
            desiredFeature = EIntents.None;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("BandWidth")]
        public async Task BandWidth(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            if (CommonDialog.debugMessages) await context.PostAsync("I understand that you want a phone with access to internet and with wide bandwidth");
            desiredFeature = EIntents.BandWidth;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("BatteryLife")]
        public async Task BatteryLife(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            if (CommonDialog.debugMessages) await context.PostAsync("I understand you want a big battery life");
            desiredFeature = EIntents.BatteryLife;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("Brand")]
        public async Task Brand(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            if (CommonDialog.debugMessages)  await context.PostAsync("I understand that the most important thing for you is brand");
            desiredFeature = EIntents.Brand;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("Camera")]
        public async Task Camera(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            if (CommonDialog.debugMessages)  await context.PostAsync("I understand that the most important thing for you is the presence of a camera");
            desiredFeature = EIntents.Camera;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("Cheap")]
        public async Task Cheap(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            string chosenPlan;

            if (context.ConversationData.TryGetValue("ChosenPlanName", out chosenPlan))
            {
                await context.PostAsync("I understand that the most important thing for you is the price.");
                desiredFeature = EIntents.Cheap;
                await ProcessNeedOrFeatureAsync(context, result);
            }
            else
            {
                await context.PostAsync("I understand that the most important thing for you is the price, but this will depend on what plan you take it with.  Lets work the plan out first then and come back to the phone.");
                desiredFeature = EIntents.Cheap;
                res = result;
                context.Call(new PlanNode(), ProcessAfterPlanAsync);
            }
 
        }

        private async Task ProcessAfterPlanAsync(IDialogContext context,IAwaitable<object> awaitable)
        {
            string chosenPlanName;

            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : Beginning of ProcessAfterPlanAsync");
            context.ConversationData.TryGetValue("ChosenPlanName", out chosenPlanName);

            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : name of chosen plan = " + chosenPlanName);
            await ProcessNeedOrFeatureAsync(context, res);
        }

        [LuisIntent("DualCamera")]
        public async Task DualCamera(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            if (CommonDialog.debugMessages) await context.PostAsync("I understand that you want a phone with Dual Camera");
            desiredFeature = EIntents.DualCamera;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("DualSIM")]
        public async Task DualSIM(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            if (CommonDialog.debugMessages) await context.PostAsync("I undertand that you would like a phone with DualSIM");
            desiredFeature = EIntents.DualSIM;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("ExpandableMemory")]
        public async Task ExpandableMemory(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that an expandable memory is the most important thing for you");
            desiredFeature = EIntents.ExpandableMemory;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("FMRadio")]
        public async Task FMRadio(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you is the presence of an FM Radio Antenna");
            desiredFeature = EIntents.FMRadio;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("FaceID")]
        public async Task FaceID(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you is the presence of Face ID recognition");
            desiredFeature = EIntents.FaceID;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("GPS")]
        public async Task GPS(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you is the presence of GPS");
            desiredFeature = EIntents.GPS;
            await ProcessNeedOrFeatureAsync(context, result);
        }


        [LuisIntent("HDVoice")]
        public async Task HDVoice(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you is the presence of High Definition voice");
            desiredFeature = EIntents.HDVoice;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("HighResDisplay")]
        public async Task HighResDisplay(IDialogContext context, LuisResult result)
        { 
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you is a High resolution display");
            desiredFeature = EIntents.HighResDisplay;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("LargeStorage")]
        public async Task LargeStorage(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you is a phone with a large storage capability");
            desiredFeature = EIntents.LargeStorage;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("OS")]
        public async Task OS(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you in a phone is the operating System");
            desiredFeature = EIntents.OS;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("ScreenSize")]
        public async Task ScreenSize(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you on a phone is the screen size");
            desiredFeature = EIntents.ScreenSize;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("SecondaryCamera")]
        public async Task SecondaryCamera(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you is the presence of a secondary camera");
            desiredFeature = EIntents.SecondaryCamera;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("Small")]
        public async Task Small(IDialogContext context, LuisResult result)         
        {
            string currentModel;


            if (!context.ConversationData.TryGetValue("HandsetModelKey",out currentModel))
                currentModel = "It's not there";

            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you are the dimensions of your new phone");
            await context.PostAsync("Your current phone is " + currentModel);
            desiredFeature = EIntents.Small;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("WaterResist")]
        public async Task WaterResist(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you is that your phone should be water resistant");
            desiredFeature = EIntents.WaterResist;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("Weight")]
        public async Task Weight(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you is the weight of your phone");
            desiredFeature = EIntents.Weight;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("WiFi")]
        public async Task WiFi(IDialogContext context, LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you is the presence of WiFi");
            desiredFeature = EIntents.WiFi;
            await ProcessNeedOrFeatureAsync(context, result);
        }
        /************* Daqui para baixo nao foi testado ******************/
        [LuisIntent("Color")]
        public async Task Color(IDialogContext context,LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understand that the most important thing for you is the color");
            desiredFeature = EIntents.Color;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("Newest")]
        public async Task Newest(IDialogContext context , LuisResult result)
        {
            await ShowDebugInfoAsync(context, result);
            await context.PostAsync("I understant that you want a recent model");
            desiredFeature = EIntents.Newest;
            await ProcessNeedOrFeatureAsync(context, result);
        }

        [LuisIntent("")]
        public async Task NoneAtAll(IDialogContext context,LuisResult result )
        {
            await ShowDebugInfoAsync(context, result);
        }

        string TyposWarning(LuisResult result)
        {
            string returnValue = null;  

            if (result.AlteredQuery != null )
            {
                returnValue = $"You typed {result.Query}, did you mean {result.AlteredQuery} ?";
            }  
            return returnValue;
        }

        private async Task ShowDebugInfoAsync (IDialogContext context,LuisResult luisResult)
        {
            IntentRecommendation topIntent;

            topIntent = luisResult.TopScoringIntent;
            desiredFeatureScore = topIntent.Score ?? 0;
            if (CommonDialog.debugMessages) await context.PostAsync($"DEBUG : The most scored intent is {topIntent.Intent} with skore = {topIntent.Score}");
            if (CommonDialog.debugMessages) await context.PostAsync(GetEntityScores(luisResult));
        }

        private string GetEntityScores(LuisResult result)
        {
            StringBuilder sb = new StringBuilder("DEBUG: Entities detected:\r\n");

            foreach(var entity in result.Entities)
            {
                sb.Append($"Type = {entity.Type}\r\nEntity = {entity.Entity}\r\nSkore = {entity.Score}\r\n");
                sb.Append("Next one :\r\n");
            }
            sb.Append("No next one\r\n");
            return sb.ToString();
        }

        private async Task ProcessNeedOrFeatureAsync(IDialogContext context, LuisResult luisResult)
        {
            EKeywords keywords = CheckForKeywords(luisResult);
            var msg = context.MakeMessage(); 
            string text = luisResult.AlteredQuery != null ? luisResult.AlteredQuery : luisResult.Query;

            if (EKeywords.ShowMeAll == keywords)
            {
                if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : found one keyword, it is " + "Show Me All");
                decoder.FeatureOrNeedDesc = "Show Me All";
                context.Done(decoder);
                return;
            } 
            else if (EKeywords.StartAgain == keywords) 
            {
                if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : Found one keyword, it is " + "Start Again");
                decoder.FeatureOrNeedDesc = "Start Again";
                context.Done(decoder);
                return;
            }
            if (desiredFeature == EIntents.None)
                return;
            res = luisResult;
            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : Beginning of ProcessNeedOrFeatureAsync() method");
            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : Text received = " + text);
            msg.Text = text;
            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : Forwarding it to Luis needs...");
            try
            {
                await context.Forward(new NodeLuisSubsNeeds(), ProcessLuisNeedsResult, msg, CancellationToken.None);
            }
            catch (Exception xception)
            {
                if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : Error...failure to forward to LUIS Subscriber needs node, xception message = " + xception.Message);
            }
            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : End of ProcessNeedOrFeatureAsync()");
        }

        private async Task UpdateUserAsync(IDialogContext context,int handSetsLeft,int handSetsB4)
        {
            StringBuilder sb = new StringBuilder("-->");
            List<EIntents> ranking = this.decoder.IntentsRanking(sb);
            var reply = ((Activity)context.Activity).CreateReply("What else is important for you on a mobile?");
          
            foreach (var intent in ranking)
                sb.Append(intent.ToString() + "\r\n");
            if (CommonDialog.debugMessages)
            {
                await context.PostAsync("DEBUG : Ranking : \r\n");
                await context.PostAsync(sb.ToString() + "\r\n");
            }
            if (handSetsLeft == handSetsB4)
                await context.PostAsync("Unfortunately, that doesn't help in narrowing the list down");
            else if (handSetsLeft == 0)
            {   
                await context.PostAsync("I'm afraid that's a very high standard, I don't have any equipment that fulfills it.");
                handSetsLeft = handSetsB4;
            }
            if  (handSetsLeft > BotConstants.MAX_CAROUSEL_CARDS)
            {
                await context.PostAsync($"We have now {handSetsLeft} models that might be suitable for your needs. I could help short list further if you tell me what else is important for you");
                sb = new StringBuilder("");
                reply.SuggestedActions = topButtons.GetTop4Buttons(sb);
                await context.PostAsync(reply);
            }
            else
            {
                if (CommonDialog.debugMessages) await context.PostAsync($"DEBUG : Number of phones on bag : {handSetsBag.BagCount()}");
                context.Done(decoder);
            }
        }

        public async Task ProcessLuisNeedsResult(IDialogContext context,IAwaitable<object> awaitable)
        {
            Tuple<NodeLuisSubsNeeds.ENeeds, double> result = (Tuple<NodeLuisSubsNeeds.ENeeds,double>) await awaitable;
            StringBuilder sb = new StringBuilder();
            double needsScore = result.Item2;
            NodeLuisSubsNeeds.ENeeds needsIntent = result.Item1;
            int handSetsLeft,handSetsNow = decoder.CurrentNumberofHandsetsLeft();

            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : Beginning of the method ProcessLuisNeedResult()");
            if (needsScore > desiredFeatureScore)  // WE have a need 
            {
                if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : It's a need, namely " + needsIntent.ToString()); 
                decoder.LastOneWasNeed = true;
                decoder.FeatureOrNeedDesc = NodeLuisSubsNeeds.GetNeedIntentDesc(result.Item1);
                handSetsLeft = needsScores.GetTopFive(needsIntent);
                await UpdateUserAsync(context, handSetsLeft, handSetsNow);    
            }  
            else
            {
                try 
                {
                    if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : I'm goin to set frequency");
                    topButtons.SetNewFreq(desiredFeature, sb);
                    if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : New Frequency set, getting into the switch to switch to correct one (no pun intented)");
                    switch (desiredFeature)
                    {
                        case EIntents.Small:
                            if (!ExtractPhoneSizeInfo(res))
                            {
                                PromptDialog.Choice(context, ProcessSizeChoice, new List<string>() { "BIGGER", "SMALLER", "THE SAME" }, "Are you looking for a phone with a similar size as your existing model or something bigger or smaller?", "Not understood, please try again", 3);
                            }
                            break;
                        case EIntents.Camera:
                            if (!GetCameraCompositeEntityData(res))  // The desired megapixels aren't present, so in this particular case we'll send it to fuzzy engine
                            {
                                handSetsLeft = needsScores.GetTopFive(NodeLuisSubsNeeds.ENeeds.Camera);
                                await UpdateUserAsync(context, handSetsLeft, handSetsNow);
                            }
                            else
                                await DecodeAndProcessIntentAsync(context);
                            break;
                        case EIntents.OS:
                            if (!GetOSData(res))
                            {
                                if (CommonDialog.debugMessages) await context.PostAsync("Handling OS intent");
                                PromptDialog.Choice(context, ProcessEnumeratedChoice, handSetsBag.GetBagOSes(), "Could you please indicate your favourite Operating System?", "Not understood, please try again", 3);
                            }
                            else
                                await DecodeAndProcessIntentAsync(context);
                            break;
                        case EIntents.Color:
                            if (!GetPreferredColors(res))
                            {
                                PromptDialog.Choice(context, ProcessEnumeratedChoice, handSetsBag.GetBagColors(), "Could you please indicate your favourite Color?", "Not understood, please try again", 3);
                            }
                            else
                                await DecodeAndProcessIntentAsync(context);
                            break;
                        case EIntents.Brand:
                            if (!GetSpecificBrands(res))
                            {
                                PromptDialog.Choice(context, ProcessEnumeratedChoice, handSetsBag.GetBagBrands(), "Could you please indicate your favourite brand?", "Not understood, please try again", 3);
                            }
                            else   
                                await DecodeAndProcessIntentAsync(context);
                            break;
                        default:
                            await DecodeAndProcessIntentAsync(context);
                            break;
                    }
                    if (CommonDialog.debugMessages) await context.PostAsync("I'm going to update the frequency for the feature = " + desiredFeature.ToString());
                    if (CommonDialog.debugMessages) await context.PostAsync("SetNewFreq() returned = " + sb.ToString());
                }
                catch (ArgumentException)
                {
                    await context.PostAsync("Argument xception");
                } 
                catch (Exception xception)
                {
                    await context.PostAsync($"Error...Exception Message = {xception.Message}");
                }
            }
        }

        private async Task ProcessSizeChoice(IDialogContext context,IAwaitable<string> awaitable)
        {
            string ans = await awaitable;
            string currentModel;
            HandSetFeatures phoneFeatures;
            int handSetsLeft, handSetsNow = decoder.CurrentNumberofHandsetsLeft();

            context.ConversationData.TryGetValue("HandsetModelKey", out currentModel);
            currentModel = "iphone 7 plus  256gb";
            phoneFeatures = handSetsBag.GetModelFeatures(currentModel);

            if (ans == "THE SAME")
            {
                needsScores.CurrentPhone = currentModel;
                handSetsLeft = needsScores.GetTopFive(NodeLuisSubsNeeds.ENeeds.PhoneSize);
                await UpdateUserAsync(context, handSetsLeft, handSetsNow);
            } 
            else
            {
                if (ans == "SMALLER")
                {
                    decoder.SetSizeRequirements(Miscellany.Product(phoneFeatures.BodySize), false);
                }
                else
                    decoder.SetSizeRequirements(Miscellany.Product(phoneFeatures.BodySize), true);
                await DecodeAndProcessIntentAsync(context);
            }
        }

        private async Task ProcessEnumeratedChoice(IDialogContext context,IAwaitable<string> awaitable)
        {
            string ans = await awaitable;
            

            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : He picked = " + ans);    
            decoder.StrKeyWords = new List<string>() { ans.ToLower() };
            await DecodeAndProcessIntentAsync(context);
        }

        private async Task DecodeAndProcessIntentAsync(IDialogContext context)
        {
            int handSetsLeft, handSetsNow = decoder.CurrentNumberofHandsetsLeft();
            string featureText;

            if (CommonDialog.debugMessages) await context.PostAsync("Beginning of DecodeAndProcessIntentAsync() method;");
            decoder.LastOneWasNeed = false;
            if (smallDesc.TryGetValue(desiredFeature, out featureText))
                decoder.FeatureOrNeedDesc = featureText;
            else
                decoder.FeatureOrNeedDesc = null;
            if (CommonDialog.debugMessages) await context.PostAsync("I'm going to call the decoder with " + desiredFeature.ToString());
            handSetsLeft = decoder.DecodeIntent(desiredFeature, res);
            if (CommonDialog.debugMessages) await context.PostAsync($"DEBUG : I have here {handSetsLeft} equipments left, bag contents = {handSetsBag.BuildStrRep()}, bag count = {handSetsBag.BagCount()}");
            await UpdateUserAsync(context, handSetsLeft, handSetsNow);
        }

        private bool GetOSData(LuisResult result)
        {
            List<string> subsOSChoices = new List<string>();

            foreach (var entity in result.Entities)
                if (entity.Type == "OperatingSystem")
                    subsOSChoices.Add(entity.Entity.ToLower());

            decoder.StrKeyWords = subsOSChoices;
            return decoder.StrKeyWords.Count != 0;
        }

        private bool GetPreferredColors(LuisResult result)
        {
            List<string> colorVector = new List<string>();

            foreach (var entity in result.Entities)
                if (entity.Type == "Color")
                    colorVector.Add(entity.Entity.ToLower());
            decoder.StrKeyWords = colorVector;
            return decoder.StrKeyWords.Count != 0;
        }

        private bool GetCameraCompositeEntityData(LuisResult result)
        {
            double megaPixels = 0;

            foreach (var cEntity in result.CompositeEntities)
                if (cEntity.ParentType == "cameracomposite")
                {
                    foreach (var child in cEntity.Children)
                        if ((child.Type == "builtin.number") && double.TryParse(child.Value, out megaPixels))
                        {
                            return true;
                        }
                }
            return false;
        }

        private bool ExtractPhoneSizeInfo(LuisResult result)
        {
            bool desc = false;  // By default, ascending
            double threshold = -1;
            string[] tokens;
            int index = 0;
            bool additionalInfoDetected = false;
            double[] volume = new double[3];

            foreach (var cEntity in result.CompositeEntities)
                if (cEntity.ParentType == "SizeComposite")
                {
                    foreach (var child in cEntity.Children)
                        switch (child.Type)
                        {
                            case "OrderByWay":
                                desc = ("small" != child.Value.ToLower() && ("smallest" != child.Value.ToLower()));
                                additionalInfoDetected = true;
                                break;
                            case "buildin.number":
                                if ((index < 3) && double.TryParse(child.Value, out volume[index]))
                                {
                                    ++index;
                                }
                                break;
                            case "DimensionsRegEx":
                                if (index >= 3)
                                    continue; // We already have the info we need about the desired volume threshold
                                tokens = child.Value.ToLower().Split('x');
                                if (double.TryParse(tokens[0], out volume[0]) && double.TryParse(tokens[1], out volume[1]) && double.TryParse(tokens[2], out volume[2]))
                                    index = 3;
                                break;
                            default:
                                break;
                        }
                    if (index == 3)  // OK, we have valid data
                    {
                        additionalInfoDetected = true;
                        threshold = Miscellany.Product(volume);
                    }
                }
            if (additionalInfoDetected)
                decoder.SetSizeRequirements(threshold, desc);
            return additionalInfoDetected;
        }

        private EKeywords CheckForKeywords(LuisResult result)
        {
            foreach (var entity in result.Entities)
                if (entity.Type == "ShowMeAll")
                    return EKeywords.ShowMeAll;
                else if (entity.Type == "StartAgain")
                    return EKeywords.StartAgain;
            return EKeywords.None;
        }

        private bool GetSpecificBrands(LuisResult res)
        {
            List<string> returnVal = new List<string>();
            string ent;

            foreach (var entity in res.Entities)
                if ((entity.Type == "Brand") && (entity.Entity.ToUpper() != "BRAND"))
                {
                    ent = entity.Entity.ToLower();
                    returnVal.Add(ent == "iphone" ? "apple" : ent);
                }
            decoder.StrKeyWords = returnVal;
            return decoder.StrKeyWords.Count != 0;
        } 
    }
}