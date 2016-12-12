using System.Threading.Tasks;

namespace JoeDoe.Core.Models.Interfaces
{
    public interface IResponder
    {
        bool CanRespond(ResponseContext context);
        BotMessage GetResponse(ResponseContext context);
        Task<BotMessage> GetResponseAsync(ResponseContext context);
    }
}