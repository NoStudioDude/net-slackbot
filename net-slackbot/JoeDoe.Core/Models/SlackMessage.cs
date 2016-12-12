using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoeDoe.Core.Models
{
    public class SlackMessage
    {
        public SlackChatHub ChatHub { get; set; }
        public bool MentionsBot { get; set; }
        public string RawData { get; set; }
        public string Text { get; set; }
        public SlackUser User { get; set; }
    }
}
