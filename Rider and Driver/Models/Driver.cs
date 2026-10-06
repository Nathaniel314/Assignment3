using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rider_and_Driver.Models
{
    public class Driver
    {
        [Key]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(100)]
        public string? Address { get; set; }

        [Required]
        [StringLength(20)]
        public string? LicenseNumber { get; set; }

        public User? User { get; set; }
    }
}