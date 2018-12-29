using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;
        public AdminController(
            DataContext context, 
            UserManager<User> userManager,
            IOptions<CloudinarySettings> cloudinaryConfig
            )
        {
            _userManager = userManager;
            _cloudinaryConfig = cloudinaryConfig;
            _context = context;

            //cloudinary account
            //cloudinary is needed to reject photos
            Account acc = new Account(
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
        }

        //policy name is declared in Startup.cs
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("usersWithRoles")]
        public async Task<IActionResult> GetUsersWithRoles()
        {
            //using Linq expression query instead of Linq liambda queries.

            var userList = await (from user in _context.Users
                                  orderby user.UserName
                                  select new //creates objects with properties you need from uses table 
                                  {
                                      Id = user.Id,
                                      UserName = user.UserName,
                                      Roles = (from userRole in user.UserRoles
                                               join role in _context.Roles
                                               on userRole.RoleId equals role.Id
                                               select role.Name).ToList()
                                  }).ToListAsync();


            return Ok(userList);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("editRoles/{UserName}")]
        //userName comes from router and roledto - from the body of http post
        public async Task<IActionResult> EditRoles(string userName, RoleEditDto roleEditDto)
        {
            var user = await _userManager.FindByNameAsync(userName);
            var userRoles = await _userManager.GetRolesAsync(user); //retrieves role names

            var selectedRoles = roleEditDto.RoleNames;

            // selected = selectedRoles != null ? selectedRoles : new string[]{}
            selectedRoles = selectedRoles ?? new string[] {}; //null coalescing operator
            
            //add new roles to the user
            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
        
            if (!result.Succeeded)
                return BadRequest("Failed to add to roles");

            //remove existing roles from the user
            result =  await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
            if (!result.Succeeded)  
                return BadRequest("Failed to delete from roles");
            return Ok(await _userManager.GetRolesAsync(user)); //return list of roles
        }
        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photosForModeration")]
        public async Task<IActionResult> GetPhotosForModeration()
        {
            var photos = await _context.Photos
                    .Include(p => p.User)
                    .IgnoreQueryFilters()
                    .Where(p => p.isApproved == false)
                    .Select(u => new
                    {
                        id = u.Id,
                        url = u.Url,
                        UserName = u.User.UserName,
                        isApproved = u.isApproved
                    }).ToListAsync();

            return Ok(photos);
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("approvePhoto/{photoId}")]
        public async Task<IActionResult> ApprovePhoto(int photoId)
        {
            //get photo from a database
            var photo = await _context.Photos
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == photoId);

            //approve it
            photo.isApproved = true;

            //save it back to database
            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("rejectPhoto/{photoId}")]
        public async Task<IActionResult> RejectPhoto(int photoId)
        {
            //get photo from a database
            var photo = await _context.Photos
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == photoId);

            if (photo.IsMain)
               return BadRequest("You cannot delete your main photo");

            //publicId s generated by claudinary
            if (photo.PublicId != null)
            {
                //delete from claudinary
                var deleteParams = new DeletionParams(photo.PublicId);
                var result = _cloudinary.Destroy(deleteParams);
                if (result.Result == "ok")
                {
                    _context.Photos.Remove(photo);
                }
            }
            else {
                 _context.Photos.Remove(photo);
            }
            //save it back to database
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}