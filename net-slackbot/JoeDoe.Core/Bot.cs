using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Bazam.Http;

using JoeDoe.Core.Configuration;
using JoeDoe.Core.Helpers;
using JoeDoe.Core.Models;
using JoeDoe.Core.Models.Enums;
using JoeDoe.Core.Models.Interfaces;

using Newtonsoft.Json;

namespace JoeDoe.Core
{
    public interface IBot
    {
        void Disconnect();
        Task Connect(string slackKey);
        Task Say(BotMessage message);
        Task SendIsTyping(SlackChatHub chatHub);
    }

    public delegate void MessageReceivedEventHandler(string messageText);

    public class Bot : IBot
    {
        private string BotNameRegex
            => _botNameRegexComposer.ComposeFor(_slackBot.self.name, _slackBot.self.id, _botConfiguration.GetAliases());

        private Dictionary<string, string> UserNameCache { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, object> ResponseContext { get; } = new Dictionary<string, object>();

        private Dictionary<string, SlackChatHub> ConnectedHubs { get; set; }
        private IEnumerable<IResponder> _responders => _botConfiguration.GetResponders();
        private bool IsConnected => _connectedSince != null;

        private readonly IBotConfiguration _botConfiguration;
        private readonly IBotNameRegexComposer _botNameRegexComposer;
        private BotWebSocket _botWebSocket;

        private DateTime? _connectedSince;

        private SlackBot _slackBot;
        private string _slackKey;

        public Bot(IBotNameRegexComposer botNameRegexComposer,
                   IBotConfiguration botConfiguration)
        {
            _botNameRegexComposer = botNameRegexComposer;
            _botConfiguration = botConfiguration;
        }

        public async Task Connect(string slackKey)
        {
            _slackKey = slackKey;

            Disconnect();

            var httpClient = new HttpClient();
            var json = await httpClient.GetStringAsync($"https://slack.com/api/rtm.start?token={_slackKey}");

            _slackBot = JsonConvert.DeserializeObject<SlackBot>(json);

            UserNameCache.Clear();
            foreach(var userObject in _slackBot.users)
                UserNameCache.Add(userObject.id, userObject.name);

            Dictionary<string, SlackChatHub> hubs = new Dictionary<string, SlackChatHub>();
            ConnectedHubs = hubs;

            if(_slackBot.channels != null)
            {
                foreach(var channelData in _slackBot.channels)
                {
                    if(!channelData.is_archived && channelData.is_member)
                    {
                        var channel = new SlackChatHub
                                      {
                                          Id = channelData.id,
                                          Name = "#" + channelData.name,
                                          Type = SlackChatHubType.Channel
                                      };
                        hubs.Add(channel.Id, channel);
                    }
                }
            }

            if(_slackBot.ims != null)
            {
                foreach(var dmData in _slackBot.ims)
                {
                    var userId = dmData.user;
                    var dm = new SlackChatHub
                             {
                                 Id = dmData.id,
                                 Name = "@" + (UserNameCache.ContainsKey(userId) ? UserNameCache[userId] : userId),
                                 Type = SlackChatHubType.Dm
                             };
                    hubs.Add(dm.Id, dm);
                }
            }

            _botWebSocket = new BotWebSocket();
            _botWebSocket.OnOpen += (sender, e) => { _connectedSince = DateTime.Now; };
            _botWebSocket.OnMessage += async (sender, message) => { await ListenTo(message); };
            _botWebSocket.OnClose += (sender, e) =>
            {
                _connectedSince = null;
                _slackBot = null;
            };

            await _botWebSocket.Connect(_slackBot.url);
        }

        public void Disconnect()
        {
            _botWebSocket?.Dispose();
        }

        public async Task Say(BotMessage message)
        {
            await Say(message, null);
        }

        public async Task SendIsTyping(SlackChatHub chatHub)
        {
            if(!IsConnected)
            {
                throw new InvalidOperationException(
                          @"Can't send the ""Bot typing"" indicator when the bot is disconnected.");
            }

            var message = new
                          {
                              type = "typing",
                              channel = chatHub.Id,
                              user = _slackBot.self.id
                          };

            await _botWebSocket.Send(JsonConvert.SerializeObject(message));
        }

        public event MessageReceivedEventHandler MessageReceived;

        private void RaiseMessageReceived(string debugText)
        {
            MessageReceived?.Invoke(debugText);
        }

        private async Task ListenTo(string json)
        {
            var slackListenMessage = JsonConvert.DeserializeObject<SlackListenMessage>(json);

            if(slackListenMessage != null)
            {
                if(slackListenMessage.Type == "message")
                {
                    string channelId = slackListenMessage.ChannelId;
                    SlackChatHub hub = null;

                    if(ConnectedHubs.ContainsKey(channelId))
                    {
                        hub = ConnectedHubs[channelId];
                    }
                    else
                    {
                        hub = SlackChatHub.FromId(channelId);
                        var hubs =
                            new Dictionary<string, SlackChatHub>(ConnectedHubs.ToDictionary(kvp => kvp.Key,
                                kvp => kvp.Value)) {{hub.Id, hub}};

                        ConnectedHubs = hubs;
                    }

                    string messageText = slackListenMessage.Message;
                    SlackUser slackUser = slackListenMessage.UserId != null ? new SlackUser() {Id = slackListenMessage.UserId} : null;
                    
                    var message = new SlackMessage
                    {
                        ChatHub = hub,
                        MentionsBot = (messageText != null && Regex.IsMatch(messageText, BotNameRegex, RegexOptions.IgnoreCase)),
                        RawData = json,
                        Text = messageText,
                        User = slackUser
                    };

                    var context = new ResponseContext
                    {
                        BotHasResponded = false,
                        BotUserID = _slackBot.self.id,
                        BotUserName = _slackBot.self.name,
                        Message = message,
                        TeamID = _slackBot.team.id,
                        UserNameCache = new ReadOnlyDictionary<string, string>(UserNameCache)
                    };

                    if(ResponseContext != null)
                    {
                        foreach(string key in ResponseContext.Keys)
                        {
                            context.Set(key, ResponseContext[key]);
                        }
                    }

                    if(message.User != null && message.User.Id != _slackBot.self.id && message.Text != null)
                    {
                        foreach(var responder in _responders)
                        {
                            if(responder != null && responder.CanRespond(context))
                            {
                                await SendIsTyping(message.ChatHub);

                                BotMessage response = responder.GetResponse(context) ?? 
                                    await responder.GetResponseAsync(context);

                                await Say(response, context);
                                context.BotHasResponded = true;
                            }
                        }
                    }
                }
            }

            RaiseMessageReceived(json);
        }

        private async Task Say(BotMessage message, ResponseContext context)
        {
            string chatHubID = null;
            if(message == null)
            {
                return;
            }

            if(message.ChatHub != null)
            {
                chatHubID = message.ChatHub.Id;
            }
            else if(context?.Message.ChatHub != null)
            {
                chatHubID = context.Message.ChatHub.Id;
            }

            if(chatHubID == null)
            {
                throw new ArgumentException(
                          $"When calling the {nameof(Say)}() method, the {nameof(message)} parameter must have its " +
                          $"{nameof(message.ChatHub)} property set.");
            }

            var client = new NoobWebClient();
            var values = new List<string>
                         {
                             "token", _slackKey,
                             "channel", chatHubID,
                             "text", message.Text,
                             "as_user", "true"
                         };

            if(message.Attachments.Count > 0)
            {
                values.Add("attachments");
                values.Add(JsonConvert.SerializeObject(message.Attachments));
            }

            var result = await client.DownloadString(
                "https://slack.com/api/chat.postMessage",
                RequestMethod.Post,
                values.ToArray());
        }
    }
}