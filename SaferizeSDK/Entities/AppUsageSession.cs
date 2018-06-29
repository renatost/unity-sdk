using System;

namespace SaferizeSDK
{
    [Serializable]
    public class AppUsageSession
    {
        public int Id;

        public Approval Approval;

		public AppFeature[] Features;
    }
}