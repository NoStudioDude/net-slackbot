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
            _bot.Connect("xoxb-112732886693-eyrxVFyMjY3B9JU8QiW4nTSi");
        }

        public void Stop()
        {
            _bot.Disconnect();
        }
    }
}