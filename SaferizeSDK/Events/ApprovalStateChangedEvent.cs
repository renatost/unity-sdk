using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SaferizeSDK
{
    [Serializable]
    class ApprovalStatusChangedEvent: SaferizeEvent
    {
        public Approval Entity;
    }
}
