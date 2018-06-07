using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Scorables.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;


using MultiDialogsBot.Dialogs;

namespace MultiDialogsBot.Scorables
{
    public class DebugScorable : ScorableBase<IActivity,string,double>
    {
        readonly IDialogTask task;
        bool? turnOn;

        public DebugScorable() {  }
        public DebugScorable(IDialogTask task)
        {
            SetField.NotNull(out this.task, nameof(task), task);
        }

        protected override async Task<string> PrepareAsync(IActivity activity,CancellationToken token)
        {
            var message = activity as IMessageActivity;

            if ((message != null) && !String.IsNullOrWhiteSpace(message.Text))
            {
                var msg = message.Text.ToLowerInvariant();


                if (msg.StartsWith("turn debugging"))
                {
                    if (msg.EndsWith("off"))
                        turnOn = false;
                    else
                        turnOn =/* CommonDialog.debugMessages =*/ true;
                }
                else
                    turnOn = null;
            }

            if (turnOn != null)
                return message.Text;
            else
                return null;
        }

        protected override bool HasScore(IActivity item, string state)
        {
            return (turnOn != null);
        }

        protected override double GetScore(IActivity item, string state)
        {
            return 1.0;
        }

        protected override async Task PostAsync(IActivity item, string state, CancellationToken token)
        {
            CommonDialog.debugMessages = turnOn ?? false;
            var commonResponsesDialog = new CommonResponsesDialog(CommonDialog.debugMessages ? "debugging is on" : "debugging is off");
            var interruption = commonResponsesDialog.Void<object, IMessageActivity>();

            this.task.Call(interruption, null);
            await task.PollAsync(token);
        }

        protected override Task DoneAsync(IActivity item, string state, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}