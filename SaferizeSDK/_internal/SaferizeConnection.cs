using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using WebSocket4Net;

namespace SaferizeSDK
{
    
    class SaferizeConnection
    {

        private string saferizeUrl;
        private string apiKey;

        private RSACryptoServiceProvider rsa;

        public SaferizeConnection(String privateKey, String saferizeUrl, String apiKey)
        {
            this.saferizeUrl = saferizeUrl;
			this.apiKey = apiKey;
		    
			string strippedPrivateKey = StripPrivateKeyHeading(privateKey);
            rsa = CypherUtils.DecodeRSAPrivateKey(System.Convert.FromBase64String(strippedPrivateKey));
        }

		private string StripPrivateKeyHeading(String privateKey){
			var pattern = @"(-+.+KEY-+)";
			var replaced = Regex.Replace(privateKey, pattern, "");

		    return replaced;
		}

        public string GetJWTSignature()
        {

            String jwtHeader = @"{""alg"": ""RS256"", ""typ"": ""JWT""}";

            int expiry = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds + 60;

            String jwtBody = @"{""aud"": ""https://saferize.com/principal"", ""sub"":" + this.apiKey + ", 'exp': " + expiry + "}";            


            String nonSigned = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jwtHeader)) + "." +
                               System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jwtBody));
            byte[] signature = rsa.SignData(System.Text.Encoding.UTF8.GetBytes(nonSigned), "SHA256");

            String jwt = nonSigned + "." + System.Convert.ToBase64String(signature);
            jwt = jwt.TrimEnd('=').Replace('+', '-').Replace('/', '_');
            return jwt;
        }

        private void SetHeaders(WebClient client)
        {
            string jwt = GetJWTSignature();
            client.Headers.Add("Content-Type", "application/json");
            client.Headers.Add("Accept", "application/vnd.saferize.com+json;version=1");
            client.Headers.Add("Authorization", "Bearer " + jwt);
        }


        public string Post(string path, string data)
        {
            using (WebClient client = new WebClient())
            {
                SetHeaders(client);
                return client.UploadString(saferizeUrl + path, data);
            }
        }

        public string Put(string path, string data)
        {
            using (WebClient client = new WebClient())
            {
                SetHeaders(client);
                return client.UploadString(saferizeUrl + path, "PUT", data);
            }
        }

        public WebSocketClient CreateWebSocketConnection(String websocketUrl)
        {         
            WebSocketClient client = new WebSocketClient(websocketUrl, this);
            return client;
        }
    }
}
