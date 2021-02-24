using System.ComponentModel.DataAnnotations;
namespace API.DTOs
{
    public class AccountRegisterDto
    {
        [Required]
        public string username { get; set; }

        [Required]
        public string password { get; set; }
    }
}