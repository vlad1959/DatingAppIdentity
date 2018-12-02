using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController: ControllerBase
    {
         private readonly IAuthRepository _repo;
         private readonly IConfiguration _config;
         private readonly IMapper _mapper;
        public AuthController(IAuthRepository repo, IConfiguration config, IMapper mapper)
        {
            _repo = repo;
            _config = config;
            _mapper = mapper;
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

            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

            if (await _repo.UserExists(userForRegisterDto.Username)) 
                return BadRequest("User Already exists");

            var userToCreate = _mapper.Map<User>(userForRegisterDto);

            var createdUser = await _repo.Register(userToCreate, userForRegisterDto.UserPassword);

            //destination - source; dto won't have password and salt 
            var userToReturn = _mapper.Map<UserForDetailedDto>(createdUser);
         
            return CreatedAtRoute("GetUser", new {controller="Users", id=createdUser.Id}, userToReturn);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto) 
        {
            // throw new Exception("Computer said no"); //testing exception
            var userFromRepo = await _repo.Login(userForLoginDto.UserName.ToLower(), userForLoginDto.UserPassword);
            if (userFromRepo == null)
               return Unauthorized();

            //build a JWT token that contains userID and user name

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)
            }; 

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

            var user = _mapper.Map<UserForListDto>(userFromRepo);

            //write token to Response to the client and user objec to pull main photo url to display in nav
            return Ok(new {
                token = tokenHandler.WriteToken(token),
                user
            });
        }
    }
}