using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Autofac;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Scorables;
using Microsoft.Bot.Builder.Scorables.Internals;

using MultiDialogsBot.Scorables;

namespace MultiDialogsBot
{
    public class GlobalMessageHandlersBotModule : Module 
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
         //   Dialogs.CommonDialog.debugMessages = true;
            builder
                .Register(c => new DebugScorable(c.Resolve<IDialogTask>()))   
                .As<IScorable<IActivity, double>>()
                .InstancePerLifetimeScope();
            builder
                .Register(c => new SecuredScorable(c.Resolve<IDialogTask>()))
                .As<IScorable<IActivity, double>>()
                .InstancePerLifetimeScope();
        }
    }
}