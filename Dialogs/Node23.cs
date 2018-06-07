namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class Node23 : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync($"Okay.  Im sorry I can't help you with this any further.  I will ask a Vodafone agent to give you a call later today.");
            await context.PostAsync($"NOTE:  The bot could be upgraded to show another offer here which has a lower commitment and less included value.");
            context.Done(23);
        }
    }
}