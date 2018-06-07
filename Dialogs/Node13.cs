namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class Node13 : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync($"OK.  What handset are you using now?");
            context.Wait(this.HandsetResponse);
        }


        public virtual async Task HandsetResponse(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            var response = message.Text;

            if (response.ToUpper().Contains("SAMSUNG"))
            {
                context.ConversationData.SetValue("HandsetMakerKey", "Samsung");
                context.ConversationData.SetValue("HSet_Brand_Show", "Samsung");
                context.Call(new Node12(), this.ResumeAfterOptionDialog);
            }
            else
            {
                if (response.ToUpper().Contains("APPLE"))
                {
                    context.ConversationData.SetValue("HandsetMakerKey", "Apple");
                    context.ConversationData.SetValue("HSet_Brand_Show", "Apple");
                    context.Call(new Node12(), this.ResumeAfterOptionDialog);
                }
                else
                {
                    await context.PostAsync($"Hmmm.   I'm sorry but I don't recognise that handset.  I will eventually need to ask you to select from a list of manufacturers and models, but for now can you just type Apple or Samsung?");
                }
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
                context.Done(13);
            }
        }

    }
}