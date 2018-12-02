using System;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dtos
{
    public class UserForRegisterDto
    {   [Required]
        public string Username { get; set; }
        [Required]
        [StringLength(8, MinimumLength=4, ErrorMessage="You must specify the password between 4 a and 8 charcters")]
        public string UserPassword { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required]
        public string KnownAs { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Country { get; set; }
        public DateTime CreatedMyProperty { get; set; }
        public DateTime LastActive { get; set; }

        public UserForRegisterDto()
        {
            LastActive = DateTime.Now;
            CreatedMyProperty = DateTime.Now;
        }
    }
}