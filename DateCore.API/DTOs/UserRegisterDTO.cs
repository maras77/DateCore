using System;
using System.ComponentModel.DataAnnotations;

namespace DateCore.API.DTOs
{
    public class UserRegisterDTO
    {
        [Required]
        public string Username { get; set; }
        [Required]
        [StringLength(16, MinimumLength=8, ErrorMessage="You must specify password between 8 and 16 characters.")]
        public string Password { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string KnownAs { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Country { get; set; }

        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }

        public UserRegisterDTO()
        {
            Created = DateTime.UtcNow;
            LastActive = DateTime.UtcNow;
        }
    }
}