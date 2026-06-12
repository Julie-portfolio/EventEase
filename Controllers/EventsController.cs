using EventEase.Data;
using EventEase.Models;
using EventEase.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class EventsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly BlobStorageService _blobStorageService;

        public EventsController(AppDbContext context, BlobStorageService blobStorageService)
        {
            _context = context;
            _blobStorageService = blobStorageService;
        }

        // GET: Events
        public async Task<IActionResult> Index(string eventType, DateTime? startDate, DateTime? endDate, DateTime? availabilityDate)
        {
            var events = _context.Events
                .Include(e => e.Venue)
                .AsQueryable();

            // Filter by event type
            if (!string.IsNullOrEmpty(eventType) && Enum.TryParse<EventType>(eventType, out var et))
            {
                events = events.Where(e => e.EventType == et);
            }

            // Filter by date range
            if (startDate.HasValue)
            {
                events = events.Where(e => e.EventDate >= startDate.Value.Date);
            }
            if (endDate.HasValue)
            {
                events = events.Where(e => e.EventDate <= endDate.Value.Date);
            }

            // Filter by venue availability on a given date (exclude events whose venue has a booking that day)
            if (availabilityDate.HasValue)
            {
                var aStart = availabilityDate.Value.Date;
                var aEnd = aStart.AddDays(1);
                events = events.Where(e => !_context.Bookings
                    .Any(b => b.VenueId == e.VenueId && b.BookingDate >= aStart && b.BookingDate < aEnd));
            }

            ViewData["EventTypes"] = new SelectList(Enum.GetValues(typeof(EventType)));
            ViewData["CurrentEventType"] = eventType;
            ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");
            ViewData["AvailabilityDate"] = availabilityDate?.ToString("yyyy-MM-dd");

            return View(await events.ToListAsync());
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null) return NotFound();
            return View(@event);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName");
            ViewData["EventTypes"] = new SelectList(Enum.GetValues(typeof(EventType)));
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventId,EventName,EventDate,Description,VenueId,ImageUrl,EventType")] Event @event, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fix the validation errors when creating the event.";
                ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", @event.VenueId);
                ViewData["EventTypes"] = new SelectList(Enum.GetValues(typeof(EventType)), @event.EventType);
                return View(@event);
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                @event.ImageUrl = await _blobStorageService.UploadImageAsync(imageFile);
            }

            _context.Add(@event);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events.FindAsync(id);
            if (@event == null) return NotFound();
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", @event.VenueId);
            ViewData["EventTypes"] = new SelectList(Enum.GetValues(typeof(EventType)), @event.EventType);
            return View(@event);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventId,EventName,EventDate,Description,VenueId,ImageUrl,EventType")] Event @event, IFormFile? imageFile)
        {
            if (id != @event.EventId) return NotFound();

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fix the validation errors when editing the event.";
                ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", @event.VenueId);
                ViewData["EventTypes"] = new SelectList(Enum.GetValues(typeof(EventType)), @event.EventType);
                return View(@event);
            }

            try
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    // If existing image exists, delete it
                    var existing = await _context.Events.AsNoTracking().FirstOrDefaultAsync(e => e.EventId == id);
                    if (existing != null && !string.IsNullOrEmpty(existing.ImageUrl))
                    {
                        await _blobStorageService.DeleteImageAsync(existing.ImageUrl);
                    }

                    @event.ImageUrl = await _blobStorageService.UploadImageAsync(imageFile);
                }

                _context.Update(@event);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Events.Any(e => e.EventId == id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Check if event has bookings
            bool hasBookings = await _context.Bookings.AnyAsync(b => b.EventId == id);
            if (hasBookings)
            {
                TempData["ErrorMessage"] = "Cannot delete this event because it has existing bookings!";
                return RedirectToAction(nameof(Index));
            }

            var ev = await _context.Events.FindAsync(id);
            if (ev != null)
            {
                if (!string.IsNullOrEmpty(ev.ImageUrl))
                    await _blobStorageService.DeleteImageAsync(ev.ImageUrl);
                _context.Events.Remove(ev);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}