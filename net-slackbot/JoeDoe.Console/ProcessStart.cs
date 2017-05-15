using JoeDoe.Core;

namespace JoeDoe.Console
{
    public class ProcessStart
    {
        private readonly IBot _bot;

        public ProcessStart(IBot bot)
        {
            _bot = bot;
        }

        public void Start()
        {
            _bot.Connect("YOUR_SLACK_BOT_API");
        }

        public void Stop()
        {
            _bot.Disconnect();
        }
    }
}
