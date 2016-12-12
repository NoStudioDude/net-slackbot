using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using JoeDoe.Core.Models;
using JoeDoe.Core.Models.Enums;
using JoeDoe.Core.Models.Interfaces;

using NineGag;

namespace JoeDoe.Core.Responders
{
    public class NineGagMeResponder : IResponder
    {
        private const string NINE_GAG_URL = "http://9gag.com/gag/";

        private Page _currentPage;
        private string _errorMessage;

        public NineGagMeResponder()
        {
            LoadPages();
        }

        public bool CanRespond(ResponseContext context)
        {
            return (context.Message.ChatHub.Type == SlackChatHubType.Channel
                    && Regex.IsMatch(context.Message.Text, @"\b(9gag)|(9gag me)|(ninegagme)|(9 gag me)|(9gagme)\b", RegexOptions.IgnoreCase));
        }

        public BotMessage GetResponse(ResponseContext context)
        {
            return null;
        }

        private async Task LoadPages()
        {
            using(var nineGagClient = new NineGagClient())
            {
                try
                {
                    if(_currentPage != null)
                        _currentPage = await nineGagClient.GetPostsAsync(_currentPage);
                    else
                        _currentPage = await nineGagClient.GetPostsAsync(PostActuality.Hot);

                }
                catch(NineGagException e)
                {
                    _errorMessage =
                        "Oh man this is embarrassing.. Something at NineGag Api is not working, but here I could grab the error message, " +
                        "maybe the guy who builded me can do something with it!! \n" +
                        $"```{e.Message}```";
                }
            }
        }

        public async Task<BotMessage> GetResponseAsync(ResponseContext context)
        {
            var botMessage = new BotMessage();

            if (_currentPage == null || _currentPage.Posts.Any())
            {
                await LoadPages();
            }

            if (string.IsNullOrEmpty(_errorMessage))
            {
                var possiblePosts = _currentPage.Posts.Where(s => !s.IsNotSafeForWork).ToList();
                var post = possiblePosts[new Random().Next(possiblePosts.Count() - 1)];
                botMessage.Text = $"{NINE_GAG_URL}{post.Id}";

                _currentPage.Posts.ToList().Remove(post);
            }
            else
                botMessage.Text = _errorMessage;

            return botMessage;
        }
    }
}