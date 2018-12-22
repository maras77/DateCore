using System;
using DateCore.API.Models;

namespace DateCore.API.DTOs
{
    public class MessageForCreationDTO
    {
        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public string Content { get; set; }
        public DateTime MessageSent { get; set; }
        public MessageForCreationDTO()
        {
            MessageSent = DateTime.UtcNow;
        }
    }
}