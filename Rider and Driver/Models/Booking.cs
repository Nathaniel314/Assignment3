using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rider_and_Driver.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        public int TripId { get; set; }

        [ForeignKey("TripId")]
        public Trip? Trip { get; set; }

        [Required]
        public int RiderUserId { get; set; }

        [ForeignKey("RiderUserId")]
        public User? Rider { get; set; }

        [Required]
        [Column(TypeName ="decimal(18,2)")]
        [Display(Name = "Total Cost")]
        public decimal TotalCost { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Confirmed";

        public DateTime BookedAt { get; set; } = DateTime.Now;
    }
}
