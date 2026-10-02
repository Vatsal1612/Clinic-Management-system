using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Clinic_Appointment_Management.Models;

[Table("t_patient")]
public class Patient
{
    public int PatientId { get; set; }

    [MaxLength(150)]
    [Required]
    public string Name { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Password { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Gender { get; set; } = string.Empty;

    [Phone]
    [MaxLength(15)]
    public string Mobile { get; set; } = string.Empty;

    [Column("c_stateId")]
    public int? StateId { get; set; }

    [Column("c_cityId")]
    public int? CityId { get; set; }

    [NotMapped]
    [MaxLength(100)]
    public string State { get; set; } = string.Empty;

    [NotMapped]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Image { get; set; }


    public string? Role { get; set; } = "Patient";
}
