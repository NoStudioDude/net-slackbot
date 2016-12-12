using JoeDoe.Core.Configuration;
using JoeDoe.Core.Helpers;

using Microsoft.Practices.Unity;

namespace JoeDoe.Core.Bootstrapping
{
    public class Bootstrapper : UnityContainerExtension
    {
        protected override void Initialize()
        {
            Container.RegisterType<IBotConfiguration, BotConfiguration>();
            Container.RegisterType<IBotNameRegexComposer, BotNameRegexComposer>();
            Container.RegisterType<IBotWebSocket, BotWebSocket>();

            Container.RegisterType<IBot, Bot>(new ContainerControlledLifetimeManager());
        }
    }
}