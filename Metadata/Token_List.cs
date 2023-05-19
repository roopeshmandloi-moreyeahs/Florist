using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Metadata
{
    public class Token_List
    {
        public string state { get; set; }
        public string token { get; set; }
        public string response { get; set; }
        public DateTime token_Expiry { get; set; }
        public Token_List(string state, string token, DateTime token_Expiry, string response)
        {
            this.state = state;
            this.token = token;
            this.response = response;
            this.token_Expiry = token_Expiry;
        }
    }
}
