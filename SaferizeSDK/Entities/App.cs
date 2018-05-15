using System;

namespace SaferizeSDK
{
    [Serializable]
    public class App
    {

        public enum StatusEnum
        {
            DRAFT, PUBLISHED, IN_REVIEW
        }

        public enum TimeRestrictionEnum
        {
            ENABLED, DISABLED
        }

        public int Age;

        public int Id;

        public string Name;

        public StatusEnum Status;

        public TimeRestrictionEnum TimeRestriction;
    }

}
