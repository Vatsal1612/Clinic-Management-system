using System.ComponentModel.DataAnnotations;

namespace MVC.Models;

public class UpdateProfileRequest
{
    public int c_PatientId { get; set; }

    [Required]
    [MaxLength(150)]
    public string c_Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string c_Gender { get; set; } = string.Empty;

    [Required]
    [MaxLength(15)]
    public string c_Mobile { get; set; } = string.Empty;

    public int c_StateId { get; set; }

    public int c_CityId { get; set; }

    // [MaxLength(4000)]
    public string? c_Image { get; set; }
}
