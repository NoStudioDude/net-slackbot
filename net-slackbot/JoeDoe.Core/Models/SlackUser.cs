namespace JoeDoe.Core.Models
{
    public class SlackUser
    {
        public string Id { get; set; }

        public string FormattedUserId
        {
            get
            {
                if(!string.IsNullOrEmpty(Id))
                {
                    return "<@" + Id + ">";
                }
                return string.Empty;
            }
        }

        public bool IsSlackbot => Id == "USLACKBOT";
    }
}