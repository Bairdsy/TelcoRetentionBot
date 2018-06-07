namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class Node29 : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            string ReminderDay;
            string ReminderTime;

            context.ConversationData.TryGetValue("ReminderDay", out ReminderDay);
            context.ConversationData.TryGetValue("ReminderTime", out ReminderTime);
            await context.PostAsync($"Great.  I will remind you {ReminderDay}, {ReminderTime}.");
            context.Done(29);
        }
    }
}