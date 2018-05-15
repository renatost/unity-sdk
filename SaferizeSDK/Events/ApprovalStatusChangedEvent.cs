using System;

namespace SaferizeSDK
{
    [Serializable]
    class ApprovalStateChangedEvent : SaferizeEvent
    {
        public Approval Entity;
    }
}