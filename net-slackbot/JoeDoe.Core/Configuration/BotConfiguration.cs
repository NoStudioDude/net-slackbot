using System.Collections.Generic;
using System.Linq;

using JoeDoe.Core.Models.Interfaces;
using JoeDoe.Core.Responders;

namespace JoeDoe.Core.Configuration
{
    public interface IBotConfiguration
    {
        IEnumerable<string> GetAliases();
        IEnumerable<IResponder> GetResponders();
    }

    public class BotConfiguration : IBotConfiguration
    {
        public IEnumerable<string> GetAliases()
        {
            return new List<string>() {"bot", "JoeDoe", "Joe", "Doe"};
        }

        public IEnumerable<IResponder> GetResponders()
        {
            var responders = new List<IResponder>();
            responders.Add(new NineGagMeResponder());

            return responders;
        }
    }
}