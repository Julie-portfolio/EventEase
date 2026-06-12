using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public enum EventType
    {
        Concert,
        Conference,
        Workshop,
        Meetup,
        Other
    }

    public class Event
    {
        public int EventId { get; set; }

        [Required]
        [Display(Name = "Event Name")]
        public string EventName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Event Date")]
        public DateTime EventDate { get; set; }

        public string? Description { get; set; }

        [Required]
        [Display(Name = "Venue")]
        public int VenueId { get; set; }

        [Display(Name = "Image")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Event Type")]
        public EventType EventType { get; set; } = EventType.Other;

        public Venue? Venue { get; set; }
        public ICollection<Booking>? Bookings { get; set; }
    }
}
