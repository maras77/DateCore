using System;
using DateCore.API.Models;

namespace DateCore.API.DTOs
{
    public class MessageForCreationDTO
    {
        public Guid SenderId { get; set; }
        public Guid RecipientId { get; set; }
        public string Content { get; set; }
        public DateTime MessageSent { get; set; }
        public MessageForCreationDTO()
        {
            MessageSent = DateTime.UtcNow;
        }
    }
}