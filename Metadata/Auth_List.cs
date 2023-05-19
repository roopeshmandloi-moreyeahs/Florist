using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Metadata
{
    public class Auth_List
    {
        public string state { get; set; }
        public string code { get; set; }
        public DateTime auth_Expiry { get; set; }
        public Auth_List(string state, string code, DateTime auth_Expiry)
        {
            this.state = state;
            this.code = code;
            this.auth_Expiry = auth_Expiry;
        }
    }
}
