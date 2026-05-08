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
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events
                .Include(e => e.Venue)
                .ToListAsync();
            return View(events);
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
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventId,EventName,EventDate,Description,VenueId,ImageUrl")] Event @event)
        {
            if (ModelState.IsValid)
            {
                _context.Add(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", @event.VenueId);
            return View(@event);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events.FindAsync(id);
            if (@event == null) return NotFound();
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", @event.VenueId);
            return View(@event);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventId,EventName,EventDate,Description,VenueId,ImageUrl")] Event @event)
        {
            if (id != @event.EventId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
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
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", @event.VenueId);
            return View(@event);
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