using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Mangojane.FuneralManagement.Web.Models;

namespace Mangojane.FuneralManagement.Web.Services;

public class SupabaseService : ISupabaseService
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly string _authUrl;
    private readonly string _apiKey;

    private static readonly List<VehicleAsset> _fallbackVehicles = new()
    {
        new VehicleAsset { VehicleId = 1, RegistrationNumber = "FS 24 BB GP", VehicleType = "Hearse", Description = "Mercedes Funeral Vehicle 01", OperationalStatus = "Available" },
        new VehicleAsset { VehicleId = 2, RegistrationNumber = "FS 88 LK GP", VehicleType = "Hearse", Description = "Executive Lincoln Hearse 02", OperationalStatus = "Available" },
        new VehicleAsset { VehicleId = 3, RegistrationNumber = "FS 12 QW GP", VehicleType = "Hearse", Description = "Toyota Quantum Hearse 03", OperationalStatus = "Available" },
        new VehicleAsset { VehicleId = 4, RegistrationNumber = "FS 99 MM GP", VehicleType = "Limousine", Description = "Mercedes Family Limousine Set", OperationalStatus = "Available" },
    };

    public SupabaseService(IConfiguration config)
    {
        _baseUrl = config["Supabase:Url"] ?? "https://gfxooglajripoikobnec.supabase.co/rest/v1";
        _authUrl = "https://gfxooglajripoikobnec.supabase.co/auth/v1";
        _apiKey = config["Supabase:ApiKey"] ?? string.Empty;

        _http = new HttpClient();
        _http.DefaultRequestHeaders.Add("apikey", _apiKey);
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    // --- REAL ADMIN AUTHENTICATION VIA SUPABASE AUTH ---

    public async Task<(bool Success, string? FullName, string? Error)> SignInAdminAsync(string email, string password)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new { email, password });
            var response = await _http.PostAsync($"{_authUrl}/token?grant_type=password", new StringContent(payload, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                return (false, null, "Invalid credentials or unauthorized account.");
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            string fullName = "Administrator";
            if (root.TryGetProperty("user", out var user) && user.TryGetProperty("user_metadata", out var meta))
            {
                if (meta.TryGetProperty("full_name", out var fn))
                    fullName = fn.GetString() ?? "Administrator";
            }

            return (true, fullName, null);
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> RegisterAdminAsync(string email, string password, string fullName, string phone, string branch)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new
            {
                email,
                password,
                data = new
                {
                    full_name = fullName,
                    phone = phone,
                    branch = branch,
                    role = "Administrator"
                }
            });

            var response = await _http.PostAsync($"{_authUrl}/signup", new StringContent(payload, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                return (false, err);
            }
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // --- BOOKINGS CRUD ---

    public async Task<List<Booking>> GetBookingsAsync()
    {
        try
        {
            var response = await _http.GetAsync($"{_baseUrl}/funeral_bookings?select=*&order=created_at.desc");
            if (!response.IsSuccessStatusCode) return new List<Booking>();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Booking>>(json) ?? new();
        }
        catch
        {
            return new List<Booking>();
        }
    }

    public async Task<Booking?> GetBookingByIdAsync(string bookingId)
    {
        try
        {
            var response = await _http.GetAsync($"{_baseUrl}/funeral_bookings?booking_id=eq.{bookingId}&select=*");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            var list = JsonSerializer.Deserialize<List<Booking>>(json);
            return list?.FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> CreateBookingAsync(Booking booking)
    {
        try
        {
            var content = new StringContent(JsonSerializer.Serialize(booking), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/funeral_bookings") { Content = content };
            request.Headers.Add("Prefer", "return=representation");
            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateBookingAsync(Booking booking)
    {
        try
        {
            var content = new StringContent(JsonSerializer.Serialize(booking), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{_baseUrl}/funeral_bookings?booking_id=eq.{booking.BookingId}")
            {
                Content = content
            };
            request.Headers.Add("Prefer", "return=representation");
            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteBookingAsync(string bookingId)
    {
        try
        {
            var response = await _http.DeleteAsync($"{_baseUrl}/funeral_bookings?booking_id=eq.{bookingId}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateBookingStatusAsync(string bookingId, string status)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new { status });
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{_baseUrl}/funeral_bookings?booking_id=eq.{bookingId}")
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Prefer", "return=representation");
            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CheckHearseConflictAsync(int hearseId, DateTime burialDate, string excludeBookingId)
    {
        try
        {
            var dateStr = burialDate.ToString("yyyy-MM-dd");
            var response = await _http.GetAsync($"{_baseUrl}/funeral_bookings?hearse_id=eq.{hearseId}&burial_date=eq.{dateStr}&booking_id=neq.{excludeBookingId}&status=eq.Approved");
            if (!response.IsSuccessStatusCode) return true;
            var json = await response.Content.ReadAsStringAsync();
            var list = JsonSerializer.Deserialize<List<Booking>>(json);
            return list == null || list.Count == 0;
        }
        catch
        {
            return true;
        }
    }

    public async Task<bool> AllocateResourcesAsync(string bookingId, int hearseId, string hearseName, string driver, string equipment)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new
            {
                status = "Approved",
                hearse_id = hearseId,
                hearse_assigned = hearseName,
                driver_assigned = driver,
                equipment_assigned = equipment
            });

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{_baseUrl}/funeral_bookings?booking_id=eq.{bookingId}")
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Prefer", "return=representation");
            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // --- RESOURCES & VEHICLES CRUD ---

    public async Task<List<VehicleAsset>> GetHearsesAsync()
    {
        try
        {
            var response = await _http.GetAsync($"{_baseUrl}/vehicle_assets?select=*&order=vehicle_id.asc");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var list = JsonSerializer.Deserialize<List<VehicleAsset>>(json);
                if (list != null && list.Count > 0)
                {
                    return list;
                }
            }
        }
        catch
        {
        }

        // Resilient fallback so the list is never blank
        return _fallbackVehicles;
    }

    public async Task<VehicleAsset?> GetVehicleByIdAsync(int id)
    {
        try
        {
            var response = await _http.GetAsync($"{_baseUrl}/vehicle_assets?vehicle_id=eq.{id}&select=*");
            if (!response.IsSuccessStatusCode)
            {
                return _fallbackVehicles.FirstOrDefault(v => v.VehicleId == id);
            }
            var json = await response.Content.ReadAsStringAsync();
            var list = JsonSerializer.Deserialize<List<VehicleAsset>>(json);
            return list?.FirstOrDefault() ?? _fallbackVehicles.FirstOrDefault(v => v.VehicleId == id);
        }
        catch
        {
            return _fallbackVehicles.FirstOrDefault(v => v.VehicleId == id);
        }
    }

    public async Task<bool> CreateVehicleAsync(VehicleAsset vehicle)
    {
        try
        {
            var existing = await GetHearsesAsync();
            int nextId = existing.Any() ? existing.Max(v => v.VehicleId) + 1 : 1;

            var payload = JsonSerializer.Serialize(new
            {
                vehicle_id = nextId,
                registration_number = vehicle.RegistrationNumber.Trim(),
                vehicle_type = vehicle.VehicleType,
                description = vehicle.Description,
                operational_status = vehicle.OperationalStatus
            });

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/vehicle_assets")
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Prefer", "return=representation");

            var response = await _http.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                vehicle.VehicleId = nextId;
                _fallbackVehicles.Add(vehicle);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateVehicleAsync(VehicleAsset vehicle)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new
            {
                registration_number = vehicle.RegistrationNumber.Trim(),
                vehicle_type = vehicle.VehicleType,
                description = vehicle.Description,
                operational_status = vehicle.OperationalStatus
            });

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{_baseUrl}/vehicle_assets?vehicle_id=eq.{vehicle.VehicleId}")
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Prefer", "return=representation");

            var response = await _http.SendAsync(request);

            var local = _fallbackVehicles.FirstOrDefault(v => v.VehicleId == vehicle.VehicleId);
            if (local != null)
            {
                local.RegistrationNumber = vehicle.RegistrationNumber;
                local.VehicleType = vehicle.VehicleType;
                local.Description = vehicle.Description;
                local.OperationalStatus = vehicle.OperationalStatus;
            }

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteVehicleAsync(int id)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{_baseUrl}/vehicle_assets?vehicle_id=eq.{id}");
            request.Headers.Add("Prefer", "return=representation");

            var response = await _http.SendAsync(request);

            _fallbackVehicles.RemoveAll(v => v.VehicleId == id);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // --- DASHBOARD METRICS & POLICIES ---

    public async Task<DashboardViewModel> GetDashboardMetricsAsync()
    {
        var bookings = await GetBookingsAsync();
        var hearses = await GetHearsesAsync();

        return new DashboardViewModel
        {
            PendingCount = bookings.Count(b => b.Status == "Pending"),
            ApprovedCount = bookings.Count(b => b.Status == "Approved"),
            UpcomingCount = bookings.Count(b => b.Status == "Approved" && b.BurialDate >= DateTime.Today),
            ResourceCount = hearses.Count(h => h.OperationalStatus == "Available"),
            RecentBookings = bookings.Take(5).ToList()
        };
    }

    public async Task<Policy?> SearchPolicyAsync(string query)
    {
        try
        {
            var response = await _http.GetAsync($"{_baseUrl}/policies?or=(policy_number.ilike.*{query}*,client_name.ilike.*{query}*)&select=*");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            var list = JsonSerializer.Deserialize<List<Policy>>(json);
            return list?.FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<PolicyPayment>> GetPaymentsForPolicyAsync(string policyNumber)
    {
        try
        {
            var response = await _http.GetAsync($"{_baseUrl}/policy_payments?policy_number=eq.{policyNumber}&order=payment_date.desc");
            if (!response.IsSuccessStatusCode) return new List<PolicyPayment>();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<PolicyPayment>>(json) ?? new();
        }
        catch
        {
            return new List<PolicyPayment>();
        }
    }

    public async Task<bool> InsertPaymentAsync(PolicyPayment payment)
    {
        try
        {
            var payload = JsonSerializer.Serialize(payment);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync($"{_baseUrl}/policy_payments", content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}