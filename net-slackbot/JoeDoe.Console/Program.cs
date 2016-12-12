using JoeDoe.Core.Bootstrapping;

using Microsoft.Practices.Unity;

using Topshelf;
using Topshelf.Unity;

namespace JoeDoe.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new UnityContainer();
            container.AddNewExtension<Bootstrapper>();

            HostFactory.New(c =>
            {
                c.UseUnityContainer(container);

                c.SetServiceName("Slack.Bot");
                c.SetDisplayName("JoeDoe Slack Bot");
                c.SetDescription("Slack bot");

                c.EnableServiceRecovery(r =>
                {
                    r.RestartService(0);
                    r.SetResetPeriod(1);
                });

                c.Service<ProcessStart>(s =>
                {
                    s.ConstructUsingUnityContainer();
                    s.WhenStarted(sgs => sgs.Start());
                    s.WhenStopped(sgs => sgs.Stop());
                });
            }).Run();
        }
    }
}