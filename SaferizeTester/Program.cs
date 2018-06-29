using System;
using System.Configuration;
using SaferizeSDK;
using System.Net.NetworkInformation;

namespace SaferizeTester {
    
    class MainClass {
		static string PRIVATE_KEY = ConfigurationManager.AppSettings["PRIVATE_KEY"];
        static string SAFERIZE_URL = ConfigurationManager.AppSettings["SAFERIZE_URL"];
        static string WEBSOCKET_URL = ConfigurationManager.AppSettings["WEBSOCKET_URL"];
        static string API_KEY = ConfigurationManager.AppSettings["API_KEY"];

        public static void Main (string[] args) {
            
            Saferize saferize = new Saferize(PRIVATE_KEY, SAFERIZE_URL, WEBSOCKET_URL, API_KEY);
			Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

			string mockToken = "eric" + unixTimestamp;
			Console.WriteLine(mockToken);

			saferize.OnOfflineStart += delegate {
				Console.WriteLine("entering offline mode");
			};

			saferize.OnOfflineEnd += delegate {
				Console.WriteLine("exiting offline mode");
			};

            saferize.OnPause += delegate{
                Console.WriteLine("paused");
            };

            saferize.OnTimeIsUp += delegate {
                Console.WriteLine("time is up");            
            };

            saferize.OnResume += delegate
            {
                Console.WriteLine("on resume");
            };

            saferize.OnRevoke += delegate {
                Console.WriteLine("on reject");
            };

			saferize.OnPINChange += ((string pinHash) =>
			{
				Console.WriteLine("we got the pin change and the pinHash is: " + pinHash);
			});


			saferize.OnAppFeaturesChange += ((AppFeature[] appFeatures) =>
			{
				Console.WriteLine("Ads are " + appFeatures);
			});
				
			Approval approval = saferize.Signup(mockToken + "@saferize.com", mockToken);
            
			// saferize.Signup automatically calls connect user
            // saferize.ConnectUser is used as a 'login' for a returning user
			// saferize.ConnectUser(mockToken);
            
			Console.WriteLine("approval.id: " + approval.Id);
            Console.WriteLine("AppUser id: " + approval.AppUser.Id);
            Console.WriteLine("app Id: " + approval.AppUser.App.Id);
            Console.WriteLine(approval.AppUser);

            Console.ReadLine();
        }
	},, 
}
