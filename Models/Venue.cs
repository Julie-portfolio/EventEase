using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Venue
    {
        public int VenueId { get; set; }

        [Required]
        [Display(Name = "Venue Name")]
        public string VenueName { get; set; } = string.Empty;

        [Required]
        public string Location { get; set; } = string.Empty;

        [Required]
        public int Capacity { get; set; }

        [Display(Name = "Image")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Available")]
        public bool IsAvailable { get; set; } = true;

        public ICollection<Event>? Events { get; set; }
        public ICollection<Booking>? Bookings { get; set; }
    }
}