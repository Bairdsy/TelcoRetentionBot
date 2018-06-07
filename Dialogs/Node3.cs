namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class Node3 : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync($"That’s ok.  Enter a time or day for me to remind you, or if you'd rather, just type 'continue' in this conversation whenever you are ready.");
            context.Wait(this.ContinueResponse);
        }

        public virtual async Task ContinueResponse(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (String.Compare(message.Text, "Continue", StringComparison.OrdinalIgnoreCase) == 0)
            {
                context.Call(new Node2(), this.ResumeAfterOptionDialog);
            }
            else
            {
                DateTime dateTime2;
                if (DateTime.TryParse(message.Text, out dateTime2))
                {
                    context.Call(new Node6(), this.ResumeAfterOptionDialog);
                }
                else
                {
                    switch (message.Text)
                    {
                        case "Monday":
                        case "monday":
                        case "Mon":
                        case "mon":
                        case "Tuesday":
                        case "tuesday":
                        case "Tue":
                        case "tue":
                        case "Tues":
                        case "tues":
                        case "Wednesday":
                        case "wednesday":
                        case "Wed":
                        case "wed":
                        case "Thursday":
                        case "thursday":
                        case "Thurs":
                        case "thurs":
                        case "Thur":
                        case "thur":
                        case "Thu":
                        case "thu":
                        case "Friday":
                        case "friday":
                        case "Fri":
                        case "fri":
                        case "Saturday":
                        case "saturday":
                        case "Sat":
                        case "sat":
                        case "Sunday":
                        case "sunday":
                        case "Sun":
                        case "sun":
                            context.Call(new Node6(), this.ResumeAfterOptionDialog);
                            break;
                        default:
                            context.Call(new Node7(), this.ResumeAfterOptionDialog);
                            break;
                    }
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
                context.Done(3);
            }
        }

    }
}