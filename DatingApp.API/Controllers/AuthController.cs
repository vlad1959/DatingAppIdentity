using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace DatingApp.API.Controllers
{
    [AllowAnonymous] //have to add this beacuse global authorization is in use with identity framework
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController: ControllerBase
    {
         //private readonly IAuthRepository _repo; //not needed since Identity framework is used
         private readonly IConfiguration _config;
         private readonly IMapper _mapper;
         private UserManager<User> _userManager;
         private SignInManager<User> _signInManager;

       // public AuthController(IAuthRepository repo, IConfiguration config, IMapper mapper)
        //Identity framework
        public AuthController(IConfiguration config, IMapper mapper, 
               UserManager<User> userManager,
               SignInManager<User> signInManager)
        {
            //_repo = repo;
            _config = config;
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        

        //since we're usin [ApiController], Register() method will automatically infer to take user 
        //from http request body. So [FromBody] annotation is not necessary
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto) {
            //validate request

            //if annotation [ApiController] is used, then code below is not needed, since bad
            //request will be return automatically  
            //if (!ModelState.IsValid)
            //    return BadRequest(ModelState);

            //replaced by Identity core
            //userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

            //if (await _repo.UserExists(userForRegisterDto.Username)) 
            //    return BadRequest("User Already exists");

            var userToCreate = _mapper.Map<User>(userForRegisterDto);

            var result = await _userManager.CreateAsync(userToCreate, userForRegisterDto.UserPassword);

            //destination - source; dto won't have password and salt 
            var userToReturn = _mapper.Map<UserForDetailedDto>(userToCreate);

            if (result.Succeeded)
            {
                return CreatedAtRoute("GetUser", new {controller="Users", id=userToCreate.Id}, userToReturn);
            }

            //replaced by identity framework
            //var createdUser = await _repo.Register(userToCreate, userForRegisterDto.UserPassword);

            return BadRequest(result.Errors);
         
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            // throw new Exception("Computer said no"); //testing exception
            //var userFromRepo = await _repo.Login(userForLoginDto.UserName.ToLower(), userForLoginDto.UserPassword);
            //replaced with Identity framework
            //if (userFromRepo == null)
            //   return Unauthorized();

            //Identity framework starts here
            //note you can use FindById, but it takes string wherars we use integers
            var user = await _userManager.FindByNameAsync(userForLoginDto.UserName);
            var result = await _signInManager.CheckPasswordSignInAsync(user, userForLoginDto.UserPassword, false);

            if (result.Succeeded)
            {
                //note: when using userManger, it won't bring back photos collection
                //for the user, so it has to be pulled back expicitly
                var appuser = await _userManager.Users.Include(p => p.Photos)
                .FirstOrDefaultAsync(u => u.NormalizedUserName == userForLoginDto.UserName.ToUpper());
                var userToReturn = _mapper.Map<UserForListDto>(appuser);
                //write token to Response to the client and user objec to pull main photo url 
                //to display in nav
                return Ok(new
                {
                    token = GenerateJwtToken(appuser).Result,
                    user = userToReturn
                });
            }

            return Unauthorized();
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            //build a JWT token that contains userID and user name

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName) //THIS IS NOW COMMING FROM UserIdentity class
            };

            //get user Roles

            var roles = await _userManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            //key to sign token with
            //appsettings.json
            //in real app, key AppSettings:Token, should be randomly generated, and not stored in appsettings.json
            var key = new SymmetricSecurityKey(Encoding.UTF8
                          .GetBytes(_config.GetSection("AppSettings:Token").Value));

            //create signing credentials, this will encrypt the key and the payload of the token

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            //this is token payload
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);

        }

    }
}