using System;
using System.Collections.Generic;
using WebSocket4Net;

namespace SaferizeSDK
{
    class WebSocketClient
    {
        WebSocket _socket;
        private String webSockerUrl;
        private SaferizeConnection saferizeConnection;
        public WebSocketClient(String websocketUrl, SaferizeConnection saferizeConnection)
        {
            this.saferizeConnection = saferizeConnection;
            this.webSockerUrl = websocketUrl;
            List<KeyValuePair<string, string>> customHeaders = new List<KeyValuePair<string, string>>();

            customHeaders.Add(new KeyValuePair<string, string>("Authorization", "Bearer " + saferizeConnection.GetJWTSignature()));

            _socket = new WebSocket(this.webSockerUrl, null, null, customHeaders, null, WebSocketVersion.Rfc6455);

        }

        public void CloseConnection()
        {
            _socket.Close();
        }

        public void OpenConnection()
        {
            _socket.Open();
        }

        public void SendMessage(string message)
        {
            _socket.Send(message);
        }

        public void SubscribeToEvent(Action<MessageReceivedEventArgs> action)
        {
            _socket.MessageReceived += new EventHandler<WebSocket4Net.MessageReceivedEventArgs>((object sender, MessageReceivedEventArgs e) =>
            {
                action(e);
            });
        }

        public void SetConnectionClose(Action<EventArgs> action)
        {
            _socket.Closed += new EventHandler((object sender, EventArgs e) => {
                action(e);
            });
        }

        public void SetErrorHandler(Action<SuperSocket.ClientEngine.ErrorEventArgs> action)
        {
            _socket.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>((object sender, SuperSocket.ClientEngine.ErrorEventArgs e) =>
            {
                action(e);
            });
        }
    }
}
