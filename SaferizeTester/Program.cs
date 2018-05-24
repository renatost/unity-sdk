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

			string mockToken = "token" + unixTimestamp;
			Console.WriteLine(mockToken);

			Approval approval  = saferize.Signup (mockToken + "@email.com", mockToken);

            Console.WriteLine("approval.id: " + approval.Id);
            Console.WriteLine("AppUser id: " + approval.AppUser.Id);
            Console.WriteLine("app Id: " + approval.AppUser.App.Id);
            Console.WriteLine(approval.AppUser);

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

			saferize.ConnectUser(mockToken);
            
            Console.ReadLine();
        }
	}
}
