using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HOTBOOK.Data;
using HOTBOOK.Models;

namespace HOTBOOK.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bookings
        public async Task<IActionResult> Index()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Guest)
                .Include(b => b.Room)
                .ThenInclude(r => r.RoomType)
                .ToListAsync();
            return View(bookings);
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Guest)
                .Include(b => b.Room)
                .ThenInclude(r => r.RoomType)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            ViewBag.Guests = _context.Guests.ToList();
            ViewBag.Rooms = _context.Rooms.Where(r => r.IsAvailable).ToList();
            return View();
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GuestId,RoomId,CheckInDate,CheckOutDate,NumberOfGuests,SpecialRequests")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                var room = await _context.Rooms.FindAsync(booking.RoomId);
                if (room == null)
                {
                    return NotFound();
                }

                // Calculate total price
                var nights = (booking.CheckOutDate - booking.CheckInDate).Days;
                booking.TotalPrice = room.PricePerNight * nights;
                booking.Status = "Pending";
                booking.BookingDate = DateTime.Now;

                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Guests = _context.Guests.ToList();
            ViewBag.Rooms = _context.Rooms.Where(r => r.IsAvailable).ToList();
            return View(booking);
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            ViewBag.Guests = _context.Guests.ToList();
            ViewBag.Rooms = _context.Rooms.ToList();
            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,GuestId,RoomId,CheckInDate,CheckOutDate,TotalPrice,Status,NumberOfGuests,SpecialRequests")] Booking booking)
        {
            if (id != booking.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Guests = _context.Guests.ToList();
            ViewBag.Rooms = _context.Rooms.ToList();
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Guest)
                .Include(b => b.Room)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Bookings/Confirm/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            booking.Status = "Confirmed";
            var room = await _context.Rooms.FindAsync(booking.RoomId);
            if (room != null)
            {
                room.IsAvailable = false;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Bookings/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            booking.Status = "Cancelled";
            var room = await _context.Rooms.FindAsync(booking.RoomId);
            if (room != null)
            {
                room.IsAvailable = true;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.Id == id);
        }
    }
} 