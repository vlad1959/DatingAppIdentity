//these are values that client appication (angular) will path to GetUsers() method of the Users controller,
//and if nothing is sent by the angular, defaults will be used
namespace DatingApp.API.Helpers
{
    public class UserParams
    {
        private const int MaxPageSize = 50;
        public int PageNumber { get; set; } = 1;
        private int pageSize = 10; //default
        public int PageSize
        {
            get { return pageSize;}
            set { pageSize = (value > MaxPageSize) ? MaxPageSize : value; }
        }
        public int UserId { get; set; } //will be used to filer out user that has been logged in

        public string Gender { get; set; } // will be passed by angular to filter by gender

        public int MinAge { get; set; } = 18;
        public int MaxAge { get; set; } = 99;
        public string OrderBy { get; set; }
        //filters to display users this user liked, or users that liked this user
        public bool Likees { get; set; } = false;
        public bool Likers { get; set; } = false;
    }
}