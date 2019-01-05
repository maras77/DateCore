using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DateCore.API.Data;
using DateCore.API.Models;
using DateCore.API.DTOs;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using DateCore.API.Helpers;
using Microsoft.AspNetCore.Identity;
using CloudinaryDotNet;
using Microsoft.Extensions.Options;
using CloudinaryDotNet.Actions;

namespace DateCore.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private readonly UserManager<User> _userManager;
        private Cloudinary _cloudinary;

        public AdminController(DataContext context, UserManager<User> userManager, IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _userManager = userManager;
            _context = context;
            _cloudinaryConfig = cloudinaryConfig;

            Account acc = new Account(
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
        }

        [Authorize(Policy= "RequireAdminRole")]
        [HttpGet("usersWithRoles")]
        public async Task<IActionResult> GetUsersWithRoles([FromQuery]UserParams userParams)
        {
            var userList = await (from user in _context.Users 
                orderby user.UserName
                select new { 
                    Id = user.Id, 
                    UserName = user.UserName, 
                    Roles = (
                        from userRole in user.UserRoles
                        join role in _context.Roles
                        on userRole.RoleId
                        equals role.Id
                        select role.Name
                    ).ToList() 
                }).ToListAsync();
            return Ok(userList);
        }

        [Authorize(Policy= "RequireAdminRole")]
        [HttpPost("editRoles/{userName}")]
        public async Task<IActionResult> EditRoles(string userName, RoleEditDTO roleEditDTO)
        {
            var user = await _userManager.FindByNameAsync(userName);
            var userRoles = await _userManager.GetRolesAsync(user);

            var selectedRoles = roleEditDTO.RoleNames;
            selectedRoles = selectedRoles ?? new string[] {};

            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
            if(!result.Succeeded)
                return BadRequest("Failed to add to roles");
            
            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
            if(!result.Succeeded)
                return BadRequest("Failed to remove the roles");

            return Ok(await _userManager.GetRolesAsync(user));
        }

        [Authorize(Policy= "ModeratePhotoRole")]
        [HttpGet("photosForModeration")]
        public async Task<IActionResult> GetPhotosForModeration([FromQuery]UserParams userParams)
        {
            var photos = await _context.Photos
                .Include(x => x.User)
                .IgnoreQueryFilters()
                .Where(x => x.IsApproved == false)
                .Select(x => new { Id = x.Id, UserName = x.User.UserName, Url = x.Url, IsApproved = x.IsApproved })
                .ToListAsync();

            return Ok(photos);
        }

        [Authorize(Policy= "ModeratePhotoRole")]
        [HttpPost("approvePhoto/{photoId}")]
        public async Task<IActionResult> ApprovePhoto(Guid photoId)
        {
            var photo = await _context.Photos
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == photoId);
            
            photo.IsApproved = true;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Policy= "ModeratePhotoRole")]
        [HttpPost("rejectPhoto/{photoId}")]
        public async Task<IActionResult> RejectPhoto(Guid photoId)
        {
            var photo = await _context.Photos
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == photoId);
            
            if(photo.IsMain)
                return BadRequest("You cannot reject the main photo");
            
            if(photo.PublicId != null)
            {
                var deleteParams = new DeletionParams(photo.PublicId);
                var result = _cloudinary.Destroy(deleteParams);
                if(result.Result == "ok")
                {
                    _context.Photos.Remove(photo);
                }
            }

            if(photo.PublicId == null)
            {
                _context.Photos.Remove(photo);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
        
    }
}
