using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rider_and_Driver.Models
{
    public class Trip
    {
        [Key]
        public int TripId { get; set; }

        [Required]
        public int DriverUserId { get; set; }

        [ForeignKey("DriverUserId")]
        public User? Driver { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Departure Date")]
        public DateTime DepartureDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Departure Time")]
        public TimeSpan DepartureTime { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Depart From")]
        public string FromLocation { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Going to")]
        public string ToLocation { get; set; }

        [Required]
        [Range(1, 10, ErrorMessage = "Seats must be between 1 and 10")]
        [Display(Name = "Seats Available")]
        public int SeatsAvailable { get; set; }

        [Required]
        [Range(0.01, 25, ErrorMessage = "Cost must be between $0.01 and $25.00")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name= "Cost per Seat($)")]
        public decimal Cost { get; set; }

        [Required]
        [StringLength(10)]
        [Display(Name = "Number Plate")]
        public string NumberPlate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";
    }
}
    

