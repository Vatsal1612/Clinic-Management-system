using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace MVC.Models
{
    public class PatientModel
    {
        public int c_PatientId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string? c_Name { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email")]
        public string? c_Email { get; set; }

        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\!\@\#\$\%\^\&\*]).{8,}$",
        ErrorMessage = "Password must contain 8 characters, uppercase, lowercase, number and special character")]
        public string? c_Password { get; set; }

        [Required]
        public string? c_Gender { get; set; }

        [Required]
        [Phone]
        public string? c_Mobile { get; set; }

        [Required]
        public int c_StateId { get; set; }

        [Required]
        public int c_CityId { get; set; }

        public string? c_Image { get; set; }

        public string? c_Role {get;set;}
        // [StringLength(4000)]
        [NotMapped]
        public IFormFile? c_ImageFile { get; set; }

        public string? c_StateName {get;set;}

        public string? c_CityName {get;set;}
    }
}