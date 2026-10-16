using Mangojane.FuneralManagement.Web.Models;

namespace Mangojane.FuneralManagement.Web.Services;

public interface ISupabaseService
{
    // Auth
    Task<(bool Success, string? FullName, string? Error)> SignInAdminAsync(string email, string password);
    Task<(bool Success, string? Error)> RegisterAdminAsync(string email, string password, string fullName, string phone, string branch);

    // Bookings CRUD
    Task<List<Booking>> GetBookingsAsync();
    Task<Booking?> GetBookingByIdAsync(string bookingId);
    Task<bool> CreateBookingAsync(Booking booking);
    Task<bool> UpdateBookingAsync(Booking booking);
    Task<bool> DeleteBookingAsync(string bookingId);
    Task<bool> UpdateBookingStatusAsync(string bookingId, string status);
    Task<bool> CheckHearseConflictAsync(int hearseId, DateTime burialDate, string excludeBookingId);
    Task<bool> AllocateResourcesAsync(string bookingId, int hearseId, string hearseName, string driver, string equipment);

    // Resources / Fleet CRUD
    Task<List<VehicleAsset>> GetHearsesAsync();
    Task<VehicleAsset?> GetVehicleByIdAsync(int id);
    Task<bool> CreateVehicleAsync(VehicleAsset vehicle);
    Task<bool> UpdateVehicleAsync(VehicleAsset vehicle);
    Task<bool> DeleteVehicleAsync(int id);

    // Dashboard & Policies
    Task<DashboardViewModel> GetDashboardMetricsAsync();
    Task<Policy?> SearchPolicyAsync(string query);
    Task<List<PolicyPayment>> GetPaymentsForPolicyAsync(string policyNumber);
    Task<bool> InsertPaymentAsync(PolicyPayment payment);
}