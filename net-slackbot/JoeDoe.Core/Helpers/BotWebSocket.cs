using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JoeDoe.Core.Helpers
{
    public interface IBotWebSocket
    {
        Task Connect(string uri);
        Task Connect(Uri uri);
        Task Disconnect();
        Task Send(string message);
        void Dispose();
    }

    public delegate void BotWebSocketMessageReceivedEventHandler(object sender, string message);

    public class BotWebSocket : IBotWebSocket
    {
        private static readonly UTF8Encoding _encoding = new UTF8Encoding();
        ClientWebSocket _webSocket;

        public async Task Connect(string uri)
        {
            await Connect(new Uri(uri));
        }

        public async Task Connect(Uri uri)
        {
            _webSocket?.Dispose();
            _webSocket = new ClientWebSocket();

            await _webSocket.ConnectAsync(uri, CancellationToken.None);
            var task = Task.Run(async () => { await Listen(); });

            OnOpen?.Invoke(this, EventArgs.Empty);
        }

        public async Task Disconnect()
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal closure", CancellationToken.None);
            _webSocket.Dispose();
        }

        public async Task Send(string message)
        {
            await
                _webSocket.SendAsync(new ArraySegment<byte>(_encoding.GetBytes(message)),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
        }

        public event BotWebSocketMessageReceivedEventHandler OnMessage;
        public event EventHandler OnOpen;
        public event EventHandler OnClose;

        private async Task Listen()
        {
            byte[] buffer = new byte[1024];

            while(_webSocket.State == WebSocketState.Open)
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if(result.MessageType == WebSocketMessageType.Close)
                {
                    await
                        _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else
                {
                    var data = _encoding.GetString(buffer);
                    // TODO: should i be doing this, and if not, what should i do instead?
                    data = TrimStuffIDontKnowWhatItEvenIs(data);
                    OnMessage?.Invoke(this, data);
                }
            }
        }

        private string TrimStuffIDontKnowWhatItEvenIs(string input)
        {
            var openBraceCount = 0;
            var indexOfLastBrace = 0;

            for(var i = 0;i < input.Length;i++)
            {
                if(input[i] == '{')
                {
                    openBraceCount++;
                }
                else if(input[i] == '}')
                {
                    openBraceCount--;
                }

                if(openBraceCount == 0)
                {
                    indexOfLastBrace = i;
                    break;
                }
            }

            return input.Substring(0, indexOfLastBrace + 1);
        }

        public void Dispose()
        {
            _webSocket?.Dispose();
        }
    }
}