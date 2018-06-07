namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class Node30 : IDialog<object>
    {
        int fail_count = 0;
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync($"Fantastic!  First, let me remind you that Im actually a bot.  If at any point you'd rather talk to someone real, just type 'call me back' and Ill get someone to give you a call.");

            await context.PostAsync($"Now I just need to go through some authentication to make sure Ive got the right person.");

            await context.PostAsync($"Can you tell me your complete Vodafone mobile number matching 0876_9_9_7?");
            context.Wait(this.WaitForMobileNumber);
        }


        public virtual async Task WaitForMobileNumber(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if ((String.Compare(message.Text, "385", StringComparison.OrdinalIgnoreCase) == 0)
                 || (String.Compare(message.Text, "0876398957", StringComparison.OrdinalIgnoreCase) == 0)
                 || (String.Compare(message.Text, "3 8 5", StringComparison.OrdinalIgnoreCase) == 0)   )
            {
                await context.PostAsync($"Perfect.  ");
            }
            else
            {
                if (fail_count < 2)
                {
                    await context.PostAsync($"I'm sorry - that's incorrect.  Why dont you try one more time?  You can type just the 3 missing digits or just type the whole mobile number. ");
                    fail_count++;
                }
                else
                {
                    context.Call(new Node30(), this.ResumeAfterOptionDialog);
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
                context.Done(0);
            }
        }


    }
}