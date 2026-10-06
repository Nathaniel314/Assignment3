using System.ComponentModel.DataAnnotations;

namespace Rider_and_Driver.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength (100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage ="Please select a role")]
        [Display(Name = "I am a")]
        public string Role { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Relationship with the driver and rider
        public Rider? Rider { get; set; }
        public Driver? Driver { get; set; }
    }
}
