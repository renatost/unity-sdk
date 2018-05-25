using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SaferizeSDK
{
    public class Saferize
    {
        private SaferizeConnection connection;
        private WebSocketClient socket;
		private AppUsageSession session;

        private string websocketUrl;
		private int reconnectTryCount;
		private int maxRetryCount;

        public delegate void PauseEventDelegate();
        public delegate void ResumeEventDelegate();
        public delegate void TimeIsUpEventDelegate();
        public delegate void RevokeEventDelegate();
		public delegate void OfflineWorkflowStartEventDelegate();
		public delegate void OfflineWorkflowEndEventDelegate();

        public event PauseEventDelegate OnPause;
        public event ResumeEventDelegate OnResume;
        public event TimeIsUpEventDelegate OnTimeIsUp;
        public event RevokeEventDelegate OnRevoke;
		public event OfflineWorkflowStartEventDelegate OnOfflineStart;
		public event OfflineWorkflowEndEventDelegate OnOfflineEnd;

        public Saferize(String privateKey, String saferizeUrl, String websocketUrl, String apiKey)
        {
            connection = new SaferizeConnection(privateKey, saferizeUrl, apiKey);
            this.websocketUrl = websocketUrl;
			this.reconnectTryCount = 0;
			this.maxRetryCount = 10;
        }

        public Approval Signup(String parentEmail, String token)
        {

            JObject approvalRequest = new JObject();
            approvalRequest.Add("user", new JObject());
            (approvalRequest["user"] as JObject).Add("token", new JObject());
            approvalRequest["user"]["token"] = token;
            approvalRequest.Add("parent", new JObject());
            (approvalRequest["parent"] as JObject).Add("email", parentEmail);
            
            try{
                string jsonResponse = connection.Post("/approval", approvalRequest.ToString());
                Approval approval = JsonConvert.DeserializeObject<Approval>(jsonResponse);
                return approval;
            }catch(WebException exception){
				HandleSignupWebException(exception);
                return null;
            }
        }

        public void ConnectUser(string usertoken)
        {
            try
            {
				
                CreateSession(usertoken);
                socket = connection.CreateWebSocketConnection(websocketUrl + "?id=" + session.Id);
                socket.SubscribeToEvent(ReceiveMessage);
				socket.SetConnectionClose(CloseConnectionCallback);
				socket.SetConnectionOpen(OpenConnectionCallback);
				socket.OpenConnection();
            }
            catch (WebException exception)
            {
                HandleConnectUserWebException(exception);
            }
        }

        private void CreateSession(String token)
        {
            string jsonResponse = connection.Post("/session/app/" + token, "");
            this.session = JsonConvert.DeserializeObject<AppUsageSession>(jsonResponse);

            if(session.Approval.CurrentState == Approval.StateEnum.PAUSED){
                OnPause?.Invoke();
            }
        }

		private void ReceiveMessage(WebSocketSharp.MessageEventArgs e)
        {
			if("&".Equals(e.Data))
			{
				return;
			}

			SaferizeEvent evt =  JsonConvert.DeserializeObject<SaferizeEvent>(e.Data);
            
            switch (evt.EventType)
            {
                case "ApprovalStatusChangedEvent":
					HandleApprovalStatusChangedEvent(JsonConvert.DeserializeObject<ApprovalStatusChangedEvent>(e.Data));
                    break;
                case "ApprovalStateChangedEvent":
                    HandleApprovalStateChangedEvent(JsonConvert.DeserializeObject<ApprovalStateChangedEvent>(e.Data));
                    break;
                case "UsageTimerTimeIsUpEvent":
                    OnTimeIsUp?.Invoke();
                    break;
                default:
                    break;
            }
        }
        
		private void OpenConnectionCallback(System.EventArgs eventArgs)
		{
			Console.WriteLine("we successfully opened a connection");
			reconnectTryCount = 0;
			OnOfflineEnd?.Invoke();
		}

		private void CloseConnectionCallback(WebSocketSharp.CloseEventArgs e)
		{
			if (e.Reason == "SocketWaitTimeExceeded" && reconnectTryCount < maxRetryCount)
            {
				ConnectUser(session.Approval.AppUser.Token);
				reconnectTryCount++;
			}else{
				//show the pinworkflow
				Console.WriteLine("directing to the pin workflow");
			}
		}
      
        private void HandleApprovalStatusChangedEvent(ApprovalStatusChangedEvent evt)
        {
            switch (evt.Entity.Status)
            {
                case Approval.StatusEnum.APPROVED:
                    OnResume?.Invoke();
                    break;
                case Approval.StatusEnum.REJECTED:
                    OnRevoke?.Invoke();
                    socket.CloseConnection();
                    break;
            }
        }

        private void HandleApprovalStateChangedEvent(ApprovalStateChangedEvent evt)
        {
            switch (evt.Entity.CurrentState)
            {
                case Approval.StateEnum.ACTIVE:
                    OnResume?.Invoke();
                    break;
                case Approval.StateEnum.PAUSED:
                    OnPause?.Invoke();
                    break;
            }
        }

		private void HandleSignupWebException(WebException exception)
		{
			if(exception.Response == null)
			{
				return;
			}
		}

		private void HandleConnectUserWebException(WebException exception)
        {
			if(exception.Response == null)
			{
				Console.WriteLine("connection timed out");
				Console.WriteLine("direct to pinworkflow");

				//this should only be run if this is off of failing to ConnectUser; SignUp is a different flow
				OnOfflineStart?.Invoke();
				return;
			}

            var resp = new StreamReader(exception.Response.GetResponseStream()).ReadToEnd();

            GameSessionException gameSessionException = JsonConvert.DeserializeObject<GameSessionException>(resp);
            switch (gameSessionException.exceptionType)
            {
                case "com.saferize.core.entities.appusagesession.ApprovalPendingException":
                    OnPause?.Invoke();
                    break;
                case "com.saferize.core.entities.appusagesession.ApprovalRejectedException":
                    OnRevoke?.Invoke();
                    break;
                case "com.saferize.core.entities.appusagesession.UsageTimerTimeIsUpException":
                    OnTimeIsUp?.Invoke();
                    break;
                case "com.saferize.core.entities.appusagesession.IllegalApprovalStateException":
                    break;
                case "com.saferize.core.entities.approval.ApprovalNotFoundException":
                    break;
                case "com.saferize.core.entities.app.IllegalAppStateException":
                    break;
                case "com.saferize.core.security.PrincipalNotFoundException":
                    break;
            }
        }
    }
}

