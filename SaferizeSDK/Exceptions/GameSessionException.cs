using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SaferizeSDK
{

    [Serializable]
    public class GameSessionException
    {
        public string exceptionType;

        public string message;
    }
}
