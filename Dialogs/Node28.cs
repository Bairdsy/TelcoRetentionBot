namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class Node28 : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            string ReminderDay;
            context.ConversationData.TryGetValue("ReminderDay", out ReminderDay);
            PromptDialog.Choice(context, this.AuthOptionSelected, new List<string>() { "Early Morning", "Late Morning", "Midday", "Afternoon", "Early Evening", "Late Evening" }, "Great.  And roughly what time on {ReminderDay}?", "Not a valid option", 6);

        }
        

        private async Task AuthOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                switch (optionSelected)
                {
                    case "Early Morning":
                    case "Late Morning":
                    case "Midday":
                    case "Afternoon":
                    case "Early Evening":
                    case "Late Evening":
                        context.ConversationData.SetValue("ReminderTime", optionSelected);

                        context.Call(new Node29(), this.ResumeAfterOptionDialog);
                        break;

                }
            }
            catch (TooManyAttemptsException ex)
            {
                await context.PostAsync($"Ooops! Too many attemps :(. But don't worry, I'm handling that exception and you can try again!");

                context.Done(0);
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
                context.Done(2);
            }
        }

    }
}
