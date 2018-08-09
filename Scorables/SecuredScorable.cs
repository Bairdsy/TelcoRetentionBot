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

using MultiDialogsBot.Helper;
using MultiDialogsBot.Dialogs;

namespace MultiDialogsBot.Scorables
{
    public class SecuredScorable : ScorableBase<IActivity, string, double>
    {
        readonly IDialogTask task;
        bool? turnOn;

        public SecuredScorable() { }
        public SecuredScorable(IDialogTask task)
        {
            SetField.NotNull(out this.task, nameof(task), task);
        }

        protected override async Task<string> PrepareAsync(IActivity activity, CancellationToken token)
        {
            var message = activity as IMessageActivity;

            if ((message != null) && !String.IsNullOrWhiteSpace(message.Text))
            {
                var msg = Miscellany.RemoveSpaces( message.Text.ToLowerInvariant());
                var keyPhrase = Miscellany.RemoveSpaces(BotConstants.KEY_PHRASE.ToLowerInvariant());

                if (msg.StartsWith(keyPhrase))
                {
                    if (msg.EndsWith(BotConstants.PASSWORD1.ToLowerInvariant()) || msg.EndsWith(BotConstants.PASSWORD2.ToLowerInvariant()))
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
            CommonDialog.locked = turnOn ?? false;
            var commonResponsesDialog = new CommonResponsesDialog(CommonDialog.locked ? "locking is on" : "locking is off");
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