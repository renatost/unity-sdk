using System;

namespace SaferizeSDK
{
    [Serializable]
    public class AppFeature
    {
		public enum AppFeatureStatus{
			ENABLED,
            DISABLED
		}

		public enum AppFeatureNameEnum
		{
			ADVERTISING,
			CHAT,
			COMMENTS,
			DATA_COLLECTION,
			IN_APP_PURCHASES,
			LOCATION_SHARING,
			PAID_APP,
			PUSH_NOTIFICATIONS,
			SOCIAL_INTERACTION,
			SUBSCRIPTION
		};

		public AppFeatureNameEnum appFeature;

		public Boolean enabled;
    }
}