using System.Collections.Generic;
using System.Linq;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace DatingApp.API.Data
{
    public class Seed
    {
        //private readonly DataContext _context;
        private readonly UserManager<User> _userManger;
        private readonly RoleManager<Role> _roleManger;

        //public Seed(DataContext context) //replaced with UserManager from Identity framework
        public Seed(UserManager<User> userManager, RoleManager<Role> roleManger)
        {
           //_context = context;
           _userManger = userManager;
           _roleManger = roleManger;
        }

        public RoleManager<Role> RoleManger { get; }

        public void SeedUsers()
        {
            if (!_userManger.Users.Any())
            {
                var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");
                var users = JsonConvert.DeserializeObject<List<Models.User>>(userData);

                var roles = new List<Role>
                    {
                        new Role{Name = "Member"},
                        new Role{Name = "Admin"},
                        new Role{Name = "Moderator"},
                        new Role{Name = "VIP"}
                    };

                foreach(var role in roles)
                {
                    _roleManger.CreateAsync(role).Wait(); //create roles in a database
                }  

                foreach (var user in users)
                {
                    //this was replaced with Identity framework
                    //byte[] passwordHash, passwordSalt;
                    //CreatePasswordHash("password", out passwordHash, out passwordSalt);
                    //replaced with Identity framework
                    //user.PasswordHash = passwordHash;
                    //user.PasswordSalt = passwordSalt;
                    //user.UserName = user.UserName.ToLower();
                    //_context.Users.Add(user);

                    //identity code goes here

                    user.Photos.SingleOrDefault().isApproved = true;
                    _userManger.CreateAsync(user, "password").Wait(); //WAit() is neede because it is async method
                    //note: weak password "password" is possible due to config in setup.cs
                    _userManger.AddToRoleAsync(user, "Member").Wait();
                }
                //create admin user

                var adminUser = new User
                {
                    UserName = "Admin"
                };

                //save admin to a database
                IdentityResult result = _userManger.CreateAsync(adminUser, "password").Result;
                if (result.Succeeded)
                {
                    var admin = _userManger.FindByNameAsync("Admin").Result; //this is user object
                    _userManger.AddToRolesAsync(admin, new[] {"Admin", "Moderator"}).Wait();
                }
                //_context.SaveChanges();
            }
        }
        //this function is no londer needed since it has been replaced with Identity framework
        /*  private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
       {
           //this will call Dispose method
           using( var hmac = new System.Security.Cryptography.HMACSHA512())
           {
               passwordSalt = hmac.Key; //returns random key
               passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
           }
       } */
    }
}