using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HOTBOOK.Data;
using HOTBOOK.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HOTBOOK.Controllers
{
    public class RoomsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoomsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Rooms
        public async Task<IActionResult> Index()
        {
            var rooms = await _context.Rooms.Include(r => r.RoomType).ToListAsync();
            return View(rooms);
        }

        // GET: Rooms/Create
        public IActionResult Create()
        {
            PopulateRoomTypesDropDownList(null);
            return View();
        }

        // POST: Rooms/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Room room)
        {
            System.Diagnostics.Debug.WriteLine($"RoomTypeId: {room.RoomTypeId}");

            if (ModelState.IsValid)
            {
                try
                {
                    room.IsAvailable = true;
                    _context.Add(room);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error occurred while saving: " + ex.Message);
                }
            }

            foreach (var key in ModelState.Keys)
            {
                var errors = ModelState[key].Errors;
                foreach (var error in errors)
                {
                    System.Diagnostics.Debug.WriteLine($"{key}: {error.ErrorMessage}");
                }
            }

            PopulateRoomTypesDropDownList(room.RoomTypeId);
            return View(room);
        }

        private void PopulateRoomTypesDropDownList(object selectedRoomType)
        {
            var roomTypes = _context.RoomTypes
                .OrderBy(rt => rt.Name)
                .ToList();

            ViewBag.RoomTypes = new SelectList(roomTypes, "Id", "Name", selectedRoomType);
        }



        // GET: Rooms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();

            PopulateRoomTypesDropDownList(room.RoomTypeId);
            return View(room);
        }

        // POST: Rooms/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Room room)
        {
            if (id != room.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(room);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Rooms.Any(e => e.Id == room.Id)) return NotFound();
                    throw;
                }
            }

            PopulateRoomTypesDropDownList(room.RoomTypeId);
            return View(room);
        }

        // GET: Rooms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var room = await _context.Rooms
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (room == null) return NotFound();

            return View(room);
        }

        // POST: Rooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Available(DateTime? checkIn, DateTime? checkOut)
        {
            if (!checkIn.HasValue || !checkOut.HasValue)
            {
                checkIn = DateTime.Today;
                checkOut = DateTime.Today.AddDays(1);
            }

            var availableRooms = await _context.Rooms
                .Include(r => r.RoomType)
                .Where(r => r.IsAvailable)
                .ToListAsync();

            ViewBag.CheckIn = checkIn;
            ViewBag.CheckOut = checkOut;
            return View(availableRooms);
        }
    }
}
