﻿namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class Node11111 : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync($"Node 11 not implemented yet.");
            context.Done(4);
        }
    }
}