using System;

namespace SaferizeSDK
{
    [Serializable]
    class AppUserChangedAppFeaturesEvent : SaferizeEvent
    {
		public AppUser Entity;
		public AppFeature[] Features;
    }
}