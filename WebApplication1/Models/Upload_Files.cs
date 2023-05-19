using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class Upload_Files
    {
        public string filePath { get; set; }
        public string Base64string { get; set; }
        public string access_token { get; set; }
        public string custType { get; set; }
    }
}
