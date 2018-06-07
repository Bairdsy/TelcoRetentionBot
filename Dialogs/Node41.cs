namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class Node41 : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            string offer_plan;
            string offer_rental;

            context.ConversationData.TryGetValue("FBP_OfferPlan", out offer_plan);
            context.ConversationData.TryGetValue("FBP_OfferRent", out offer_rental);

            PromptDialog.Choice(context, this.OptionSelected, new List<string>() { "Confirm", "Reject", "I need help" }, $"At the end of the term, your Agreement will continue to run on a month to month basis, unless you, the Customer, provide notice of your desire to terminate this agreement. ", "Not a valid option", 3);
        }

        private async Task OptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;
                switch (optionSelected)
                {
                    case "Confirm":
                        context.Call(new Node42(), this.ResumeAfterOptionDialog);
                        break;
                        
                    case "Reject":
                        context.Call(new Node38(), this.ResumeAfterOptionDialog);
                        break;

                    case "I need help":
                        context.Call(new Node5(), this.ResumeAfterOptionDialog);
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