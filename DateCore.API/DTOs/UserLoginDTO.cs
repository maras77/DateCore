using System.ComponentModel.DataAnnotations;

namespace DateCore.API.DTOs
{
    public class UserLoginDTO
    {
        [Required]
        public string Username { get; set; }
        [Required]
        [StringLength(16, MinimumLength=8, ErrorMessage="You must specify password between 8 and 16 characters.")]
        public string Password { get; set; }
    }
}