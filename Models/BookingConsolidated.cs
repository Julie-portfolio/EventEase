using System;

namespace EventEase.Models
{
    // This model maps to a SQL view (vw_BookingConsolidated) and has no key.
    public class BookingConsolidated
    {
        public int BookingId { get; set; }
        public int EventId { get; set; }
        public string? EventName { get; set; }
        public int VenueId { get; set; }
        public string? VenueName { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime? EventDate { get; set; }
        // EventType stored as int in the DB
        public int? EventType { get; set; }
    }
}