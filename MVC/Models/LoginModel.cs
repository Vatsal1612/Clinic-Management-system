using System.ComponentModel.DataAnnotations;

namespace MVC.Models
{
    public class LoginModel
    {
        [Required]
        [EmailAddress]
        public string? c_Email { get; set; }

        [Required]
        public string? c_Password { get; set; }

        public string ? c_Role {get;set;}
    }
}