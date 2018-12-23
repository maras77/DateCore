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

namespace DateCore.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
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
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
        {
            var currentUserId= Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userFromRepo = await _repo.GetUser(currentUserId);
            userParams.UserId = currentUserId;

            if(string.IsNullOrEmpty(userParams.Gender)) 
            {
                userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";
            }

            var users = await _repo.GetUsers(userParams);
            var usersView = _mapper.Map<IEnumerable<UserForListDTO>>(users);
            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
            return Ok(usersView);
        }

        [HttpGet("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var user = await _repo.GetUser(id);
            var userView = _mapper.Map<UserForDetailedDTO>(user);
            return Ok(userView);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, UserForUpdateDTO userForUpdateDto)
        {
            if(id != Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            
            var userFromRepo = await _repo.GetUser(id);
            _mapper.Map(userForUpdateDto, userFromRepo);
            
            if(await _repo.SaveAll())
                return NoContent();
            
            throw new Exception($"Updating user with {id} failed on save");
        }

        [HttpPost("{id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser(Guid id, Guid recipientId)
        {
            if(id != Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            
            var like = await _repo.GetLike(id, recipientId);
            if(like != null)
                return BadRequest("You already like this user");

            if(await _repo.GetUser(recipientId) == null)
                return NotFound();
            
            like = new Like { LikerId = id, LikeeId = recipientId };
            _repo.Add<Like>(like);

            if(await _repo.SaveAll())
                return Ok();
            
            return BadRequest("Failed to like user");
        }
    }
}
