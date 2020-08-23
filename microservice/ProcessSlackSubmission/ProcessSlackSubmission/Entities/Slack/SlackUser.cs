
namespace ProcessSlackSubmission.Entities
{
    public class SlackUser
    {
        public class Profile
        {
            public string email { get; set; }
        }
        public class RootObject
        {
            public bool ok { get; set; }
            public Profile profile { get; set; }
        }
    }
}