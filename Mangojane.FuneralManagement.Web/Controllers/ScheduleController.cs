using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mangojane.FuneralManagement.Web.Services;

namespace Mangojane.FuneralManagement.Web.Controllers;

[Authorize]
public class ScheduleController : Controller
{
    private readonly ISupabaseService _supabase;

    public ScheduleController(ISupabaseService supabase)
    {
        _supabase = supabase;
    }

    [HttpGet]
    public async Task<IActionResult> DailySchedule()
    {
        var bookings = await _supabase.GetBookingsAsync();
        return View(bookings);
    }
}