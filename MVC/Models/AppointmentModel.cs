using System.ComponentModel.DataAnnotations;

namespace MVC.Models
{
    public class AppointmentModel
    {
        public int c_AppointmentId { get; set; }

        [Required]
        public int c_PatientId{get;set;}

        [Required]
        public int c_DepartmentId { get; set; }

        [Required]
        public DateTime c_Date { get; set; }

        [Required]
        public string? c_Time { get; set; }

        public string? c_Status { get; set; }  // Confirmed / Pending / Cancelled
    }
}