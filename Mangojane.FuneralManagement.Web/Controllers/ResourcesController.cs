using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mangojane.FuneralManagement.Web.Models;
using Mangojane.FuneralManagement.Web.Services;

namespace Mangojane.FuneralManagement.Web.Controllers;

[Authorize]
public class ResourcesController : Controller
{
    private readonly ISupabaseService _supabase;

    public ResourcesController(ISupabaseService supabase)
    {
        _supabase = supabase;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var resources = await _supabase.GetHearsesAsync();
        return View(resources);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new VehicleAsset());
    }

    [HttpPost]
    public async Task<IActionResult> Create(VehicleAsset vehicle)
    {
        var success = await _supabase.CreateVehicleAsync(vehicle);
        if (success)
            TempData["Success"] = $"Asset {vehicle.RegistrationNumber} registered successfully.";
        else
            TempData["Error"] = "Failed to save resource to Supabase. Check database permissions.";

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var vehicle = await _supabase.GetVehicleByIdAsync(id);
        if (vehicle == null) return NotFound();
        return View(vehicle);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(VehicleAsset vehicle)
    {
        var success = await _supabase.UpdateVehicleAsync(vehicle);
        if (success)
            TempData["Success"] = $"Asset {vehicle.RegistrationNumber} updated successfully.";
        else
            TempData["Error"] = "Failed to update resource in Supabase.";

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _supabase.DeleteVehicleAsync(id);
        if (success)
            TempData["Success"] = "Asset removed from database successfully.";
        else
            TempData["Error"] = "Failed to delete resource from Supabase.";

        return RedirectToAction("Index");
    }
}