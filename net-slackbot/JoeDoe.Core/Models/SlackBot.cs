using System.Collections.Generic;

namespace JoeDoe.Core.Models
{
    public class SlackBot
    {
        public Self self { get; set; }
        public Team team { get; set; }
        public List<Users> users { get; set; }
        public List<Channels> channels { get; set; }
        public List<Ims> ims { get; set; }
        public string url { get; set; }
    }

    public class Self
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Team
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Users
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Channels
    {
        public string id { get; set; }
        public string name { get; set; }
        public bool is_archived { get; set; }
        public bool is_member { get; set; }
    }

    public class Ims
    {
        public string id { get; set; }
        public string user { get; set; }

    }
}