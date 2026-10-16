using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mangojane.FuneralManagement.Web.Models;
using Mangojane.FuneralManagement.Web.Services;

namespace Mangojane.FuneralManagement.Web.Controllers;

[Authorize]
public class PoliciesController : Controller
{
    private readonly ISupabaseService _supabase;

    public PoliciesController(ISupabaseService supabase)
    {
        _supabase = supabase;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var query = string.IsNullOrWhiteSpace(search) ? "MFP-2026-001" : search;
        var policy = await _supabase.SearchPolicyAsync(query);
        ViewBag.Payments = policy != null ? await _supabase.GetPaymentsForPolicyAsync(policy.PolicyNumber) : new();
        return View(policy);
    }

    [HttpGet]
    public async Task<IActionResult> RecordPayment(string policyNumber)
    {
        var policy = await _supabase.SearchPolicyAsync(policyNumber);
        if (policy == null) return NotFound();

        return View(new RecordPaymentViewModel
        {
            PolicyNumber = policy.PolicyNumber,
            ClientName = policy.ClientName,
            Amount = policy.MonthlyPremium,
            ReferenceNumber = $"PAY-{new Random().Next(10000, 99999)}"
        });
    }

    [HttpPost]
    public async Task<IActionResult> RecordPayment(RecordPaymentViewModel vm)
    {
        var payment = new PolicyPayment
        {
            PolicyNumber = vm.PolicyNumber,
            ReferenceNumber = vm.ReferenceNumber,
            Amount = vm.Amount,
            PaymentDate = vm.PaymentDate,
            PaymentMethod = vm.PaymentMethod,
            ReceivedBy = "Mangojane Staff"
        };

        await _supabase.InsertPaymentAsync(payment);
        return RedirectToAction("PaymentConfirmation", new { policyNumber = vm.PolicyNumber });
    }

    public IActionResult PaymentConfirmation(string policyNumber)
    {
        ViewBag.PolicyNumber = policyNumber;
        return View();
    }
}