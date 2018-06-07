namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using com.esendex.sdk.messaging;

    [Serializable]
    public class Node2 : IDialog<object>
    {
        private const string DDebit     = "Direct Debit";
        private const string BTransfer  = "Bank Transfer";
        private const string CCard      = "Credit Card";
        int fail_count = 0;
        string subsno;

        public async Task StartAsync(IDialogContext context)
        {
            if (context.ConversationData.TryGetValue("SubsNumber", out subsno))
            {
                string authSubsNo = "0"+subsno.Substring(0, 3) + "_" + subsno[4] + "_" + subsno[6] + "_" + subsno.Substring(8, subsno.Length - 8);

                await context.PostAsync($"Fantastic!  First, let me remind you that Im actually a bot.  If at any point you'd rather talk to someone real, just type 'call me back' and Ill get someone to give you a call.");
                await Task.Delay(3000);

                await context.PostAsync($"Now I just need to go through some authentication to make sure Ive got the right person.");
                await Task.Delay(3000);

                await context.PostAsync($"Can you tell me your complete Vodafone mobile number matching {authSubsNo}?");
                context.Wait(this.WaitForMobileNumber);
            }
            else
            {
                await context.PostAsync($"Hmmm.  Seems I couldnt get the subscriber number.");
            }
        }


        public virtual async Task WaitForMobileNumber(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            string valid1 = ""+subsno[3]+subsno[5]+subsno[7];
            string valid2 = subsno[3] + " " + subsno[5] + " " + subsno[7];
            string valid3 = subsno.Insert(0, "0");

            //await context.PostAsync($"Valid answers are [{subsno}][{valid1}][{valid2}][{valid3}].");

            if ((String.Compare(message.Text, valid1, StringComparison.OrdinalIgnoreCase) == 0)
                 || (String.Compare(message.Text, subsno, StringComparison.OrdinalIgnoreCase) == 0)
                 || (String.Compare(message.Text, valid3, StringComparison.OrdinalIgnoreCase) == 0)   
                 || (String.Compare(message.Text, valid2, StringComparison.OrdinalIgnoreCase) == 0)   )
            {
                await context.PostAsync($"Perfect.");

                PromptDialog.Choice(context, this.AuthOptionSelected, new List<string>() { DDebit, BTransfer, CCard }, "Now can you tell me how you paid your most recent bill?", "Not a valid option", 3);
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


        private async Task AuthOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                switch (optionSelected)
                {
                    case DDebit:
                        context.Call(new Node8(), this.ResumeAfterOptionDialog);
                        break;

                    case BTransfer:
                        await context.PostAsync($"I'm sorry - that's incorrect, but dont worry.  Im going to send you a code via SMS instead.  Just enter the code here once you receive it.");
                        fail_count = 0;
                        var messagingService = new MessagingService("ryan@madcalm.com", "Mervyn2009");
                        messagingService.SendMessage(new SmsMessage("353876398957", "Hello!", "EX0233821"));
                        context.Wait(this.textmsg);
                        break;

                    case CCard:
                        await context.PostAsync($"I'm sorry - that's incorrect, but dont worry.  Im going to send you a code via SMS instead.  Just enter the code here once you receive it.");
                        fail_count = 0;
                        context.Wait(this.textmsg);
                        break;
                }
            }
            catch (TooManyAttemptsException ex)
            {
                await context.PostAsync($"Ooops! Too many attemps :(. But don't worry, I'm handling that exception and you can try again!");

                context.Done(0);
            }
        }

        public virtual async Task textmsg(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if ((String.Compare(message.Text, "12345", StringComparison.OrdinalIgnoreCase) == 0))
            {
                context.Call(new Node8(), this.ResumeAfterOptionDialog);
            }
            else
            {
                if (fail_count < 2)
                {
                    await context.PostAsync($"I'm sorry - that's incorrect.  Why dont you try one more time just in case you made a typo?  I need just the code from the SMS with no spaces or other characters.");
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
                context.Done(2);
            }
        }
    }
}