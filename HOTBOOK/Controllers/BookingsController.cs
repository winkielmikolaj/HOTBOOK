using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HOTBOOK.Data;
using HOTBOOK.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace HOTBOOK.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            ILogger<BookingsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Bookings
        [Authorize(Roles = "Admin,Staff,Guest")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdminOrStaff = User.IsInRole("Admin") || User.IsInRole("Staff");

            var bookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                .ThenInclude(r => r.RoomType)
                .Where(b => isAdminOrStaff || b.UserId == userId)
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
                .Include(b => b.User)
                .Include(b => b.Room)
                .ThenInclude(r => r.RoomType)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            // Check if user is authorized to view this booking
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (booking.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                return Forbid();
            }

            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create(int? roomId, DateTime? checkIn, DateTime? checkOut)
        {
            ViewBag.Rooms = _context.Rooms.Where(r => r.IsAvailable).ToList();
            ViewBag.SelectedRoomId = roomId;
            ViewBag.CheckIn = checkIn;
            ViewBag.CheckOut = checkOut;
            return View();
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoomId,CheckInDate,CheckOutDate,NumberOfGuests,SpecialRequests")] Booking booking)
        {
            _logger.LogInformation("Starting booking creation process");
            
            try
            {
                // Get user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogError("User ID not found in claims");
                    return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Create", "Bookings") });
                }

                // Create new booking with all required fields
                var newBooking = new Booking
                {
                    UserId = userId,
                    RoomId = booking.RoomId,
                    CheckInDate = booking.CheckInDate,
                    CheckOutDate = booking.CheckOutDate,
                    NumberOfGuests = booking.NumberOfGuests,
                    SpecialRequests = booking.SpecialRequests,
                    Status = "Pending",
                    BookingDate = DateTime.Now
                };

                // Validate dates
                if (newBooking.CheckInDate < DateTime.Today)
                {
                    ModelState.AddModelError("CheckInDate", "Check-in date cannot be in the past.");
                    ViewBag.Rooms = _context.Rooms.Where(r => r.IsAvailable).ToList();
                    return View(booking);
                }

                if (newBooking.CheckOutDate <= newBooking.CheckInDate)
                {
                    ModelState.AddModelError("CheckOutDate", "Check-out date must be after check-in date.");
                    ViewBag.Rooms = _context.Rooms.Where(r => r.IsAvailable).ToList();
                    return View(booking);
                }

                // Get room and validate
                var room = await _context.Rooms.FindAsync(newBooking.RoomId);
                if (room == null)
                {
                    _logger.LogWarning("Room not found with ID: {RoomId}", newBooking.RoomId);
                    ModelState.AddModelError("RoomId", "Selected room does not exist.");
                    ViewBag.Rooms = _context.Rooms.Where(r => r.IsAvailable).ToList();
                    return View(booking);
                }

                // Check if room is available for selected dates
                var conflictingBooking = await _context.Bookings
                    .Where(b => b.RoomId == newBooking.RoomId && b.Status != "Cancelled" &&
                        ((b.CheckInDate <= newBooking.CheckInDate && b.CheckOutDate > newBooking.CheckInDate) ||
                         (b.CheckInDate < newBooking.CheckOutDate && b.CheckOutDate >= newBooking.CheckOutDate) ||
                         (b.CheckInDate >= newBooking.CheckInDate && b.CheckOutDate <= newBooking.CheckOutDate)))
                    .FirstOrDefaultAsync();

                if (conflictingBooking != null)
                {
                    ModelState.AddModelError("", "The room is not available for the selected dates.");
                    ViewBag.Rooms = _context.Rooms.Where(r => r.IsAvailable).ToList();
                    return View(booking);
                }

                // Calculate total price
                var nights = (newBooking.CheckOutDate - newBooking.CheckInDate).Days;
                newBooking.TotalPrice = room.PricePerNight * nights;

                _logger.LogInformation("Adding booking to context: {@Booking}", newBooking);
                _context.Add(newBooking);
                
                var result = await _context.SaveChangesAsync();
                _logger.LogInformation("SaveChanges result: {Result}", result);

                if (result > 0)
                {
                    return RedirectToAction(nameof(Details), new { id = newBooking.Id });
                }
                else
                {
                    ModelState.AddModelError("", "Failed to save the booking. Please try again.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                ModelState.AddModelError("", "An error occurred while creating the booking. Please try again.");
            }

            ViewBag.Rooms = _context.Rooms.Where(r => r.IsAvailable).ToList();
            return View(booking);
        }

        // GET: Bookings/Edit/5
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            ViewBag.Rooms = await _context.Rooms.ToListAsync();
            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,RoomId,CheckInDate,CheckOutDate,NumberOfGuests,TotalPrice,Status,SpecialRequests")] Booking booking)
        {
            if (id != booking.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBooking = await _context.Bookings
                        .Include(b => b.Room)
                        .FirstOrDefaultAsync(b => b.Id == id);

                    if (existingBooking == null)
                    {
                        return NotFound();
                    }

                    // Update only the fields that should be editable
                    existingBooking.RoomId = booking.RoomId;
                    existingBooking.CheckInDate = booking.CheckInDate;
                    existingBooking.CheckOutDate = booking.CheckOutDate;
                    existingBooking.TotalPrice = booking.TotalPrice;
                    existingBooking.Status = booking.Status;
                    existingBooking.NumberOfGuests = booking.NumberOfGuests;
                    existingBooking.SpecialRequests = booking.SpecialRequests;

                    // Update room availability based on status
                    var room = await _context.Rooms.FindAsync(booking.RoomId);
                    if (room != null)
                    {
                        room.IsAvailable = booking.Status != "Confirmed";
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
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
            }

            ViewBag.Rooms = await _context.Rooms.ToListAsync();
            return View(booking);
        }

        // GET: Bookings/Delete/5
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                .ThenInclude(r => r.RoomType)
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
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            // Update room availability if the booking was confirmed
            if (booking.Status == "Confirmed" && booking.Room != null)
            {
                booking.Room.IsAvailable = true;
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Bookings/Confirm/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
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
        [Authorize(Roles = "Admin,Staff")]
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