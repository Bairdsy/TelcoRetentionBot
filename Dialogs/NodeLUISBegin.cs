using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Threading;
using System.Threading.Tasks;


using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

using Microsoft.Bot.Connector;

using MultiDialogsBot.Helper;

namespace MultiDialogsBot.Dialogs
{
    [LuisModel("0ffacaae-8314-4b1d-af4d-68718eb880f6", "99127c285bd3420aa9d9f460091b7683", LuisApiVersion.V2,null,SpellCheck = true,Verbose = true)]
    [Serializable]
    public class NodeLUISBegin : LuisDialog<object>
    {
        const int MAX_TRIES = 5;

        public enum EIntent
        {
            None,
            Both,
            HandSet,
            Plan
        }

        enum EDegreeOfCertain
        {
            High,
            Medium,
            Low
        }


        static Dictionary<string,Tuple<string,EIntent>> humanFriendlyIntent = new Dictionary<string, Tuple<string,EIntent>>()
        {
            { "Upgrade Both" , new Tuple<string,EIntent>("want to upgrade both",EIntent.Both)},
            {"Upgrade Equipment",new Tuple<string,EIntent>("want to upgrade equipment only",EIntent.HandSet) },
            { "Upgrade plan", new Tuple<string,EIntent>("want to upgrade plan",EIntent.Plan)},
            

        };  
        
        static int numberOfTries = 1;

        LuisUpdater updater = new LuisUpdater();
        string nonUnderstoodUtterance,initialPhrase;
        bool justCheck4Errors;

        public NodeLUISBegin(bool checkSpelling = false)
        {
            justCheck4Errors = checkSpelling;
        }

        [LuisIntent("Upgrade Both")]
        public async Task UpgradeBoth(IDialogContext context,LuisResult result)
        {
            string intention = "Upgrade Both"; 
            string secondIntention = null;
            bool closeToSecond = CloseToSecond(result);
            string typosWarning = TyposInformation(result);

            if (justCheck4Errors)
            {
                CheckSpelling(context, result);
                return;
            }

            EDegreeOfCertain degreeOfCertain = GetDegreeOfCertain(result);


            if (typosWarning != null)
            {
                await context.PostAsync(typosWarning);
                initialPhrase = result.AlteredQuery.ToLower();
            }
            else
                initialPhrase = result.Query.ToLower();
            await this.PostDebugInfoAsync(context, result, intention);

            if (degreeOfCertain == EDegreeOfCertain.High)
            {
                if (CommonDialog.debugMessages) await context.PostAsync($"DEBUG : I understand that you {humanFriendlyIntent[intention].Item1}");
                context.Done(Tuple.Create(initialPhrase,humanFriendlyIntent[intention].Item2));
            }
            else if (degreeOfCertain == EDegreeOfCertain.Medium)
            {
                nonUnderstoodUtterance = initialPhrase;
                if (closeToSecond)
                    secondIntention = ObtainSecondMostLikelyIntent(result);
                await DoubleCheck(context, intention,secondIntention);
            }
            else
            {
                await AskToRephraseAsync(context, result);
            }
        }

        [LuisIntent("Upgrade Equipment")]
        public async Task UpgradeEquipment(IDialogContext context, LuisResult result)
        {
            string intention = "Upgrade Equipment";  
            string secondIntention = null;      
            bool closeToSecond = CloseToSecond(result);
            string typosWarning = null;

            if (justCheck4Errors)
            {
                CheckSpelling(context, result);
                return;
            }

            try
            {
                 typosWarning = TyposInformation(result);
            }
            catch (Exception xception)
            {
                await context.PostAsync("DEBUG : Exception message = " + xception.Message + " <==");
            }
            EDegreeOfCertain degreeOfCertain = GetDegreeOfCertain(result);

            if (typosWarning != null)
            {
                await context.PostAsync(typosWarning);
                initialPhrase = result.AlteredQuery.ToLower();        
            }
            else
                initialPhrase = result.Query.ToLower();

            await this.PostDebugInfoAsync(context, result, intention);

            if (degreeOfCertain == EDegreeOfCertain.High)
                context.Done(Tuple.Create(initialPhrase,   humanFriendlyIntent[intention].Item2));
            else if (degreeOfCertain == EDegreeOfCertain.Medium)
            {
                nonUnderstoodUtterance = initialPhrase;
                if (closeToSecond)
                    secondIntention = ObtainSecondMostLikelyIntent(result);
                await DoubleCheck(context, intention, secondIntention);
            }
            else   
            {
                await AskToRephraseAsync(context, result);
            }
        }

        [LuisIntent("Upgrade plan")]
        public async Task UpgradePlan(IDialogContext context,LuisResult result)
        {
            string intention = "Upgrade plan";
            string secondIntention = null;
            string typosWarning = TyposInformation(result); 
            bool typos = typosWarning != null;
            bool closeToSecond = CloseToSecond(result);

            if (justCheck4Errors)
            {
                CheckSpelling(context, result);
                return;
            }

            EDegreeOfCertain degreeOfCertain = GetDegreeOfCertain(result);

            if (typos)
            {
                await context.PostAsync(typosWarning);
                initialPhrase = result.AlteredQuery.ToLower();
            }
            else
                initialPhrase = result.Query.ToLower();
            await this.PostDebugInfoAsync(context, result, intention);

            if (degreeOfCertain == EDegreeOfCertain.High)
            {
                if (CommonDialog.debugMessages) await context.PostAsync($"I understand that you {humanFriendlyIntent[intention].Item1}");
                context.Done(Tuple.Create(initialPhrase,humanFriendlyIntent[intention].Item2));
            }
            else if (degreeOfCertain == EDegreeOfCertain.Medium)
            {
                nonUnderstoodUtterance = initialPhrase;
                if (closeToSecond)
                    secondIntention = ObtainSecondMostLikelyIntent(result);
                await DoubleCheck(context, intention,secondIntention);
            }
            else
            {
                await AskToRephraseAsync(context, result);
            }
        }

        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string typosWarning = TyposInformation(result);
            bool typos = typosWarning != null;
            double score = ObtainTopIntentScore(result);

            if (justCheck4Errors)
            {
                CheckSpelling(context, result);
                return;
            }

            EDegreeOfCertain degreeOfCertain = GetDegreeOfCertain(result);

            if (typos)
            {
                await context.PostAsync(typosWarning);
                initialPhrase = result.AlteredQuery;
            }
            else
                initialPhrase = result.Query;
            await this.PostDebugInfoAsync(context, result, "No intention" );

            await context.PostAsync($"Im sorry. Either I didn’t understand how I can help or I understood that you dont need any help right now. I’ll be here whenever you type something new and we can start again.");
            context.Done(Tuple.Create(initialPhrase,EIntent.None));
        }

        [LuisIntent("")]
        public async Task NoneAtAll(IDialogContext context,LuisResult result)
        {
            double degreeOfCertain = ObtainTopIntentScore(result);

            if (justCheck4Errors)
            {
                CheckSpelling(context, result);
                return;
            }

            await AskToRephraseAsync(context, result);
        }
    
        private async Task PostDebugInfoAsync(IDialogContext context, LuisResult result, string intention)   
        {
            double intentionScore = ObtainTopIntentScore(result);
            double secondIntentionScore = ObtainSecondTopIntentScore(result);
            string secondMostLikelyIntention = ObtainSecondMostLikelyIntent(result);   

            if (CommonDialog.debugMessages) await context.PostAsync($"DEBUG: First Intent is {intention}, with skore = {intentionScore}");          
            if (CommonDialog.debugMessages) await context.PostAsync($"DEBUG: Second Intent is {secondMostLikelyIntention}, with skore = {secondIntentionScore}");
        }

        private async Task AskToRephraseAsync(IDialogContext context,LuisResult result)
        {
            if (numberOfTries >= MAX_TRIES)
            {
                await context.PostAsync("I'm sorry, I'm afraid I can't understand what you want to do. Would you like to talk to a human?");
                numberOfTries = 1; // reset it
                context.Done(0);
                return;
            }
            if (numberOfTries++ == 1)
            {
                await context.PostAsync("Sorry, I couldn't understand what you would like to do. ");
                await context.PostAsync("Would it be possible to rephrase?");
                context.Wait(this.MessageReceived);
            }
            else
            {
                numberOfTries++;
                await context.PostAsync("I still didn't get it");
                await context.PostAsync("Can you please rephrase it?");
                context.Wait(this.MessageReceived);
            }
        }
        private bool CloseToSecond(LuisResult result)
        {
            double topIntentScore = ObtainTopIntentScore(result);
            double secondHighest;
            Tuple<string, EIntent> dummyTuple;

            if ((result.Intents.Count < 2) ||
                (!humanFriendlyIntent.TryGetValue( result.Intents[1].Intent,out dummyTuple)) ||
                (topIntentScore == 0)) // Avoid divide by zero
                return false;
            secondHighest = result.Intents[1].Score ?? 0;

            return (secondHighest / topIntentScore)  >= 0.5;
        }



        private EDegreeOfCertain GetDegreeOfCertain(LuisResult result)
        {
            EDegreeOfCertain returnValue;
            double score = ObtainTopIntentScore(result);
            bool secondIsClose = CloseToSecond(result);

            if (score >= BotConstants.PERCENT_HIGH_CERTAINTY )
                returnValue = EDegreeOfCertain.High;
            else if (score > BotConstants.PERCENT_MEDIUM_CERTAINTY) 
                returnValue = EDegreeOfCertain.Medium;
            else
                returnValue = EDegreeOfCertain.Low;

            if (secondIsClose)
                if (returnValue == EDegreeOfCertain.High)
                    return EDegreeOfCertain.Medium;
                else
                    return EDegreeOfCertain.Low;
            else
                return returnValue;
        }

        private string ObtainSecondMostLikelyIntent(LuisResult result)
        {
            if (result.Intents.Count < 2)
                return null;
            return result.Intents[1].Intent  ;  //-humanFriendlyIntent[ result.Intents[1].Intent].Item1;
        }

        private double ObtainSecondTopIntentScore(LuisResult result)
        {
            if (result.Intents.Count < 2)
                return 0;
            return result.Intents[1].Score ?? 0;
        }

        private double ObtainTopIntentScore(LuisResult result)
        {
            if (result.TopScoringIntent != null)
                return result.TopScoringIntent.Score ?? 0;
            else
                return 0;
        }


        private async Task CheckUserAnswerAsync(IDialogContext context,IAwaitable<IMessageActivity> result)
        {   
            var ans = (await result);
            var text = ans.Text;
            bool LUISUpdated,luisTrained,firstTime = true;
            Tuple<string, EIntent> tuple;
            string retVal = "";
            ITypingActivity typingActivity;
            var connector = new ConnectorClient(new Uri(((Activity)ans).ServiceUrl));
            int counter = 0,maxTimes = 8;
              
            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : Beginning of method CheckUserAnswerAsync()");
            if (humanFriendlyIntent.TryGetValue(text, out tuple))  
            {    
                try
                {
                    if (CommonDialog.debugMessages) await context.PostAsync("Checking if we saw utterance enough times, the non-unnderstood utterance is " + nonUnderstoodUtterance);
                    LUISUpdated = await updater.UpdateUtteranceAsync(text, nonUnderstoodUtterance);
                    if (LUISUpdated)
                    {
                        if (CommonDialog.debugMessages) await context.PostAsync("We did, utterance added to LUIS and order for training sent");
                        if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : Debug Messages : " + updater.debug);
                        if (CommonDialog.debugMessages) await context.PostAsync("DEBUG: Training status = " + (await updater.CheckTrainStatus()));
                        do
                        {
                            if (luisTrained = await updater.CheckTrainStatus()) break;
                            if (!firstTime)
                            {
                                typingActivity = ((Activity)ans).CreateReply();
                                typingActivity.Type = ActivityTypes.Typing;   
                                await connector.Conversations.SendToConversationAsync((Activity)typingActivity);
                            }
                            else
                                firstTime = false;
                            Thread.Sleep(2500);  
                            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG: Checking Training status = " + (await updater.CheckTrainStatus()));
                            
                            if (CommonDialog.debugMessages) await context.PostAsync("DEBUG: Training status = " + (await updater.CheckTrainStatus()));
                        }
                        while ((counter < maxTimes) && !luisTrained);
                            
                        retVal = await updater.PublishLuisAsync();   
                        if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : Publish POST returned : " + retVal);
                        if (CommonDialog.debugMessages) await context.PostAsync("DEBUG: Debug mesages = " + updater.debug);
                    }
                }
                catch (Exception xception)
                {
                    await context.PostAsync("Error...could not add utterance to LUIS, xception message = " + xception.Message);
                }
                if (CommonDialog.debugMessages) await context.PostAsync("DEBUG : End of Method CheckUserAnswerAsync()");  
                context.Done(Tuple.Create(initialPhrase,tuple.Item2));  
            }
            else
            {
                await context.PostAsync("OK, I didn't get that, could you please rephrase it for me?");
                ++numberOfTries;
                context.Wait(this.MessageReceived);
            }
        }


        private async Task DoubleCheck(IDialogContext context,string mostLikelyIntent,string secondMostLikely)
        {
            string friendly1 = humanFriendlyIntent[mostLikelyIntent].Item1;
            string friendly2  ;         
            string textForUser = string.Concat("I am not sure I followed that\r\n",
                                   "I think you meant you ", friendly1);
            Activity reply, lastMessage = (Activity) context.Activity;
            Attachment imageAttachment = new Attachment()
            {
                ContentUrl = "http://madcalm.com/wp-content/uploads/2018/06/MADCALM-CONFUSED.png", // "https://image.freepik.com/free-vector/businessman-with-doubts_23-2147618177.jpg",
                ContentType = "image/png",
                Name = "Bender_Rodriguez.png"  
            };                    
            SuggestedActions suggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {   
                    new CardAction(){ Title = "Yes", Type=ActionTypes.ImBack, Value="Yes" /*, Image = "https://emojipedia-us.s3.amazonaws.com/thumbs/120/emoji-one/104/thumbs-up-sign_1f44d.png"*/ },
                    new CardAction(){ Title = "No", Type=ActionTypes.ImBack, Value="No" , /*Image = "https://emojipedia-us.s3.amazonaws.com/thumbs/120/apple/129/thumbs-down-sign_1f44e.png"*/}
                }
            };
            if (secondMostLikely != null)
            {   
                friendly2 = humanFriendlyIntent[secondMostLikely].Item1;
                textForUser += string.Concat("\r\n but maybe you'd rather ", friendly2, "\r\n, could you please select the correct one?");
                suggestedActions.Actions[0].Title += ",I " + friendly1;
                suggestedActions.Actions[1].Title += ",I " + friendly2;
                suggestedActions.Actions[0].Value = mostLikelyIntent;      
                suggestedActions.Actions[1].Value = secondMostLikely;
                suggestedActions.Actions.Add(new CardAction() { Title = "Neither", Type = ActionTypes.ImBack, Value = "Neither" });
            }
            else
            {
                textForUser += ", but, again, I'm not sure..  could you please agree?";    
                suggestedActions.Actions[0].Value = mostLikelyIntent;
            }
            reply = lastMessage.CreateReply(textForUser);
            reply.SuggestedActions = suggestedActions;
            reply.Attachments.Add(imageAttachment);
            reply.AttachmentLayout = "carousel";
            await context.PostAsync(reply);
               
            context.Wait(CheckUserAnswerAsync);
        }

        private string TyposInformation(LuisResult result) 
        {
            //   System.Text.StringBuilder sb = new System.Text.StringBuilder();
            string correctedQuery; 
              
            if (result.AlteredQuery == null)
                return null;
            correctedQuery = Miscellany.QueryCompare(result.Query, result.AlteredQuery);
            return $"You typed \"{result.Query}\", did you mean \"{correctedQuery}\"?";
        }
        private void CheckSpelling(IDialogContext context, LuisResult result)
        {
            if (CommonDialog.debugMessages) context.PostAsync("Beginning of CheckSpelling() method");
            if (result.AlteredQuery == null)
                context.Done(Tuple.Create<string, EIntent>(result.Query, EIntent.None));
            else
                context.Done(Tuple.Create<string, EIntent>($"{result.Query}:{result.AlteredQuery}", EIntent.HandSet));
        }
    }
}