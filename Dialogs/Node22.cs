namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class Node22 : IDialog<object>
    {

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync($"Okay.  Im checking to see if I have anything cheaper that I can offer you.");
            await Task.Delay(3000);

            var cost_FBP = 0;
            var cost_12M = 0;
            context.ConversationData.TryGetValue("FBP_OfferRev", out cost_FBP);
            if (context.ConversationData.TryGetValue("12M_OfferRev", out cost_12M))
            {
                if ( (float)cost_FBP > (float)cost_12M)
                {
                    await context.PostAsync($"We do have a plan which can save you some money, but we cant offer you a handset with it.  Let me show it to you now.");
                    await Task.Delay(3000);
                    context.Call(new Node10(), ResumeAfterOptionDialog);                    
                }
                else
                {
                    await context.PostAsync($"I'm sorry but I can't help you with this any further.  I will ask a Vodafone agent to give you a call later today.");
                    await context.PostAsync($"NOTE:  This message has been shown because the 12 Month SIMO offer is more expensive than the current offer.  The bot could be upgraded to show another Full Bill Pay offer here which has a lower commitment and less included value.");
                    context.Done(22);
                }
            }
            else
            {
                await context.PostAsync($"I'm sorry but I can't help you with this any further.  I will ask a Vodafone agent to give you a call later today.");
                await context.PostAsync($"NOTE:  This message has been shown because the 12 Month SIMO offer is Ineligible.  The bot could be upgraded to show another Full Bill Pay offer here which has a lower commitment and less included value.");
                context.Done(22);
            }

        }


        private async Task ResumeAfterOptionDialog(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                var message = await result;
            }
            catch (Exception ex)
            {
                await context.PostAsync($"Failed with message: {ex.Message}");
            }
            finally
            {
                context.Done(22);
            }
        }
    }
}