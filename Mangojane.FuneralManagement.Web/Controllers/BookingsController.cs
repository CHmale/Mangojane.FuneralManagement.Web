using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mangojane.FuneralManagement.Web.Models;
using Mangojane.FuneralManagement.Web.Services;

namespace Mangojane.FuneralManagement.Web.Controllers;

[Authorize]
public class BookingsController : Controller
{
    private readonly ISupabaseService _supabase;

    public BookingsController(ISupabaseService supabase)
    {
        _supabase = supabase;
    }

    // READ: Triage Queue
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var bookings = await _supabase.GetBookingsAsync();
        return View(bookings);
    }

    // READ: View Single Booking Details
    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        var booking = await _supabase.GetBookingByIdAsync(id);
        if (booking == null) return NotFound();
        return View(booking);
    }

    // CREATE: Walk-in Client Intake Form (GET)
    [HttpGet]
    public IActionResult Create()
    {
        return View(new Booking
        {
            BookingId = $"MFS-{new Random().Next(100, 999)}",
            BurialDate = DateTime.Today.AddDays(4),
            DateOfDeath = DateTime.Today.AddDays(-2),
            Status = "Pending"
        });
    }

    // CREATE: Save Walk-in Client Booking to Supabase (POST)
    [HttpPost]
    public async Task<IActionResult> Create(Booking booking)
    {
        await _supabase.CreateBookingAsync(booking);
        TempData["Success"] = $"Walk-in booking {booking.BookingId} created successfully!";
        return RedirectToAction("Index");
    }

    // UPDATE: Quick Approve Booking
    [HttpPost]
    public async Task<IActionResult> Approve(string id)
    {
        await _supabase.UpdateBookingStatusAsync(id, "Approved");
        return RedirectToAction("Allocate", new { id });
    }

    // UPDATE: Quick Reject Booking
    [HttpPost]
    public async Task<IActionResult> Reject(string id)
    {
        await _supabase.UpdateBookingStatusAsync(id, "Rejected");
        return RedirectToAction("Index");
    }

    // UPDATE: Resource Allocation Form (GET)
    [HttpGet]
    public async Task<IActionResult> Allocate(string id)
    {
        var booking = await _supabase.GetBookingByIdAsync(id);
        if (booking == null) return NotFound();

        var hearses = await _supabase.GetHearsesAsync();
        var vm = new ResourceAllocationViewModel
        {
            BookingId = booking.BookingId,
            DeceasedName = booking.DeceasedName,
            BurialDate = booking.BurialDate,
            Location = booking.CemeteryName,
            PackageName = booking.PackageName,
            AvailableHearses = hearses,
            AvailableDrivers = new() { "T. Sithole", "K. Malejane", "S. Mokoena", "P. Modise" }
        };

        return View(vm);
    }

    // UPDATE: Save Resource Allocation (POST)
    [HttpPost]
    public async Task<IActionResult> Allocate(ResourceAllocationViewModel vm)
    {
        // 1. Conflict Collision Check
        bool isAvailable = await _supabase.CheckHearseConflictAsync(vm.SelectedHearseId, vm.BurialDate, vm.BookingId);
        if (!isAvailable)
        {
            vm.ErrorMessage = "LOGISTICAL COLLISION: Selected hearse is already booked for this burial date. Please select another vehicle.";
            vm.AvailableHearses = await _supabase.GetHearsesAsync();
            vm.AvailableDrivers = new() { "T. Sithole", "K. Malejane", "S. Mokoena", "P. Modise" };
            return View(vm);
        }

        // 2. Save Live to Supabase
        var hearses = await _supabase.GetHearsesAsync();
        var selectedHearse = hearses.FirstOrDefault(h => h.VehicleId == vm.SelectedHearseId);

        await _supabase.AllocateResourcesAsync(
            vm.BookingId,
            vm.SelectedHearseId,
            selectedHearse?.Description ?? "Assigned Hearse",
            vm.SelectedDriver,
            vm.EquipmentDetails);

        TempData["Success"] = $"Booking {vm.BookingId} approved and resources allocated successfully!";
        return RedirectToAction("Index");
    }

    // DELETE: Cancel / Remove Booking (POST)
    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        await _supabase.DeleteBookingAsync(id);
        TempData["Success"] = $"Booking {id} cancelled and deleted.";
        return RedirectToAction("Index");
    }
}