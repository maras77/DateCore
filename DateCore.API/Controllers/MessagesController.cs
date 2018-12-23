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
    [Route("api/users/{userId}/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;

        public MessagesController(IDatingRepository repo, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
        }

        [HttpGet("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage(Guid userId, Guid id)
        {
            if(userId != Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messageFromRepo = await _repo.GetMessage(id);
            if(messageFromRepo == null)
                return NotFound();

            return Ok(messageFromRepo);
        }

        [HttpGet]
        public async Task<IActionResult> GetMessagesForUser(Guid userId, [FromQuery]MessageParams messageParams)
        {
            if(userId != Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            messageParams.UserId = userId;
            var messageFromRepo = await _repo.GetMessagesForUser(messageParams);
            var messages = _mapper.Map<IEnumerable<MessageToReturnDTO>>(messageFromRepo);
            
            Response.AddPagination(messageFromRepo.CurrentPage, messageFromRepo.PageSize, messageFromRepo.TotalCount, messageFromRepo.TotalPages);

            return Ok(messages);
        }

        [HttpGet("thread/{recipientId}")]
        public async Task<IActionResult> GetMessageThread(Guid userId, Guid recipientId)
        {
            if(userId != Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messageFromRepo = await _repo.GetMessageThread(userId, recipientId);
            var messages = _mapper.Map<IEnumerable<MessageToReturnDTO>>(messageFromRepo);
            
            return Ok(messages);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(Guid userId, MessageForCreationDTO messageForCreationDTO)
        {
            
            if(userId != Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            messageForCreationDTO.SenderId = userId;
            var recipient = await _repo.GetUser(messageForCreationDTO.RecipientId);

            if(recipient == null)
                return BadRequest("Could not find user");
            
            var message = _mapper.Map<Message>(messageForCreationDTO);
            _repo.Add(message);
            
            if(await _repo.SaveAll())
            {
                var sender = await _repo.GetUser(userId); // automapper will use sender props to fill MessageToReturnDTO
                var messageToReturn = _mapper.Map<MessageToReturnDTO>(message);
                return CreatedAtRoute("GetMessage", new { id = message.Id}, messageToReturn);
            }
                

            throw new Exception("Creating the message failed on save");
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> DeleteMessage(Guid id, Guid userId)
        {          
            if(userId != Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messageFromRepo = await _repo.GetMessage(id);
            if(messageFromRepo.SenderId == userId)
                messageFromRepo.IsSenderDeleted = true;
            if(messageFromRepo.RecipientId == userId)
                messageFromRepo.IsRecipientDeleted = true;
            if(messageFromRepo.IsSenderDeleted && messageFromRepo.IsRecipientDeleted)
                _repo.Delete(messageFromRepo);
            
            if(await _repo.SaveAll())
            {
                return NoContent();
            }
                
            throw new Exception("Error deleting the message");
            
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkMessageAsRead(Guid id, Guid userId)
        { 
            if(userId != Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            
            var messageFromRepo = await _repo.GetMessage(id);
            if(messageFromRepo.RecipientId != userId)
                return Unauthorized();

            messageFromRepo.IsRead = true;
            messageFromRepo.DateRead = DateTime.UtcNow;
            if(await _repo.SaveAll())
            {
                return NoContent();
            }                
            throw new Exception("Error marking the message as read");
        }

    }
}
