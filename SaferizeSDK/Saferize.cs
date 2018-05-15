using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocket4Net;

namespace SaferizeSDK
{
    public class Saferize
    {
        private SaferizeConnection connection;
        private WebSocketClient socket;
        private string websocketUrl;
        private AppUsageSession session;

        public delegate void PauseEventDelegate();
        public delegate void ResumeEventDelegate();
        public delegate void TimeIsUpEventDelegate();
        public delegate void RevokeEventDelegate();

        public event PauseEventDelegate OnPause;
        public event ResumeEventDelegate OnResume;
        public event TimeIsUpEventDelegate OnTimeIsUp;
        public event RevokeEventDelegate OnRevoke;

        public Saferize(String privateKey, String saferizeUrl, String websocketUrl, String apiKey)
        {
            connection = new SaferizeConnection(privateKey, saferizeUrl, apiKey);
            this.websocketUrl = websocketUrl;
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
                HandleWebException(exception);
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
                socket.OpenConnection();
            }
            catch (WebException exception)
            {
                HandleWebException(exception);
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

        private void ReceiveMessage(MessageReceivedEventArgs e)
        {
            SaferizeEvent evt =  JsonConvert.DeserializeObject<SaferizeEvent>(e.Message);

            switch (evt.EventType)
            {
                case "ApprovalStatusChangedEvent":
                    HandleApprovalStatusChangedEvent(JsonConvert.DeserializeObject<ApprovalStatusChangedEvent>(e.Message));
                    break;
                case "ApprovalStateChangedEvent":
                    HandleApprovalStateChangedEvent(JsonConvert.DeserializeObject<ApprovalStateChangedEvent>(e.Message));
                    break;
                case "UsageTimerTimeIsUpEvent":
                    OnTimeIsUp?.Invoke();
                    break;
                default:
                    break;
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

        private void HandleWebException(WebException exception)
        {
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

