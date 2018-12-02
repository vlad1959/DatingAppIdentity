namespace DatingApp.API.Models
{
    public class Like
    {
        //id of the user that likes another user (basically user id)
        public int LikerId { get; set; }

        //id of the user being liked by another user (basically ids that user likes)
        public int LikeeId { get; set; }

        //objects below needed to set up enetity framework relationships
        public User Liker { get; set; }
        public User Likee { get; set; }
    }
}