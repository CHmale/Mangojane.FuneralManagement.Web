using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mangojane.FuneralManagement.Web.Services;

namespace Mangojane.FuneralManagement.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ISupabaseService _supabase;

    public HomeController(ISupabaseService supabase)
    {
        _supabase = supabase;
    }

    public async Task<IActionResult> Dashboard()
    {
        var vm = await _supabase.GetDashboardMetricsAsync();
        return View(vm);
    }
}