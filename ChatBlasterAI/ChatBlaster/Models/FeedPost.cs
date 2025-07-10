using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBlaster.Models
{
    public class FeedPost
    {
        public string _title { get; set; }
        public string _urlToPostAdvertiser { get; set; }
        public string _text { get; set; }
        public string _reactionCount { get; set; }
        public string _commentsCount { get; set; }
        public string _sharesCount { get; set; }
        public List<string> _reelsLinkList { get; set; } = new List<string>();
        public List<string> _videosList { get; set; } = new List<string>();

    }
}
