using System;

namespace SaferizeSDK
{

    [Serializable]
    public class Approval
    {
        public  enum StatusEnum
        {
            PENDING,
            NOTIFIED,
            APPROVED,
            REJECTED,
        }

        public enum StateEnum
        {
            PAUSED,
            ACTIVE
        }

        public int Id;

        public string ParentEmail;

        public StatusEnum Status;

        public AppUser AppUser;

        public StateEnum CurrentState;

    }

}
