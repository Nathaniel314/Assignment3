using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rider_and_Driver.Models
{
    public class TripSearch
    {
        public string? FromLocation { get; set; }

        public string? ToLocation { get; set; }

        public DateTime? DepartureDate { get; set; }

        public string? SortBy { get; set; }

        public string? SortOrder { get; set; }

        public List<Trip> Trips { get; set; } = new();
    }
}
