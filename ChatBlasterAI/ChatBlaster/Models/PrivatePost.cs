using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBlaster.Models
{
    public class PrivatePost
    {
        public List<string> photoPaths { get; set; }
        private static readonly object _lock = new();
        public string _createIteamUrl { get; set; }
        public string _title { get; set; }
        public string Id { get; set; }
        public string _postText { get; set; }
    }
}
