using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    // url: http://localhost:5000/api/users
    //only authorized user can get access
    [ServiceFilter(typeof(LogUserActivity))] //this will use this class to update activity date whenever any method is called fom this controller
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;

        public UsersController(IDatingRepository repo, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
        }

        [HttpGet]
        //userParams is data sent by the angular with the query string
        //Note: the client may not send anything, but page size and items per page will 
        //still be deafulted and returned back to the client
        //example of the call: http://localhost:5000/api/users?pageNumber=1&pageSize=10
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
        {
            //currently logged in user - from the token
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userFromRepo = await _repo.GetUser(currentUserId);
            userParams.UserId = currentUserId;
            //if gender wasn't passed, then take gender from the repo object and set it to the opposite sex
            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = (userFromRepo.Gender == "male" ? "female" : "male");
            }

            //users is PagedList<User>, which conatin the users based on page number and size
            var users = await _repo.GetUsers(userParams); //PagedList users
            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);
            //add pagination header to response using REsponse extension method
           

            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
            return Ok(usersToReturn);
        }
        [HttpGet("{id}", Name="GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _repo.GetUser(id);
            // will map source, which is user to UserForDetailedDto
            var userToReturn = _mapper.Map<UserForDetailedDto>(user);
            return Ok(userToReturn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto) {
            //if id passed does not match user id in a token - return unauthorized.
           
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
               return Unauthorized();

            var userFromRepo = await _repo.GetUser(id);  
            _mapper.Map(userForUpdateDto, userFromRepo); //will map(update) user from repo (database) with user dto

            if (await _repo.SaveAll())
                return NoContent();
            else
                throw new System.Exception($"Updating user {id} failed on save");
        }
        [HttpPost("{id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser(int id, int recipientId)
        {
            //if id passed does not match user id in a token - return unauthorized.
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
               return Unauthorized();

            var like = await _repo.GetLike(id, recipientId);  

            if (like != null)
                return BadRequest("You already liked this user");

            //recipient does not exist    
            if (await _repo.GetUser(recipientId) == null)
                return NotFound();

            like = new Like
            {
                LikerId = id,
                LikeeId = recipientId
            };
             _repo.Add<Like>(like); //not async method, just adding to memory

             if (await _repo.SaveAll())
               return Ok();

             return BadRequest("Failed to like user");    
        }
    }
}