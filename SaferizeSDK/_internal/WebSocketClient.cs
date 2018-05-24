using System;
using System.Collections.Generic;
using WebSocketSharp;
using System.Threading;

namespace SaferizeSDK
{
    class WebSocketClient
    {
        WebSocket _socket;
        private String webSockerUrl;
        private SaferizeConnection saferizeConnection;
		private DateTime lastPingResponseTime;
		private int maxPingResponseWaitTime;
		private Timer timer;

        public WebSocketClient(String websocketUrl, SaferizeConnection saferizeConnection)
        {
			this.webSockerUrl = websocketUrl;
			this.saferizeConnection = saferizeConnection;
			_socket = new WebSocket(webSockerUrl);
			_socket.EmitOnPing = true;
			_socket.CustomHeaders = new Dictionary<string, string> {
				{"Authorization", "Bearer " + saferizeConnection.GetJWTSignature()},
			};

			maxPingResponseWaitTime = 5;
			timer = new Timer(checkSocketState, this, maxPingResponseWaitTime * 1000, maxPingResponseWaitTime * 1000);
        }
        
		private void checkSocketState(object state)
		{
			if(lastPingResponseTime != DateTime.MinValue && DateTime.Now.Subtract(lastPingResponseTime).TotalSeconds > maxPingResponseWaitTime)
			{
				_socket.Close(WebSocketSharp.CloseStatusCode.Away, "SocketWaitTimeExceeded");
				timer.Dispose();
			}
		}

        public void CloseConnection()
        {
            _socket.Close();
        }
        
        public void OpenConnection()
        {
			_socket.Connect();
        }
      
		public void SubscribeToEvent(Action<WebSocketSharp.MessageEventArgs> action)
        {
			_socket.OnMessage += ((object sender, WebSocketSharp.MessageEventArgs e) =>
            {
				if(e.IsPing){
					lastPingResponseTime = DateTime.Now;
				}

                action(e);
            });
        }

		public void SetConnectionClose(Action<WebSocketSharp.CloseEventArgs> action)
        {
			_socket.OnClose += ((object sender, WebSocketSharp.CloseEventArgs e) => {
                action(e);
            });
        }
        
    }
}
