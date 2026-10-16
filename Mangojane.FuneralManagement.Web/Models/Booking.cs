using System.Text.Json.Serialization;

namespace Mangojane.FuneralManagement.Web.Models;

public class Booking
{
    [JsonPropertyName("booking_id")]
    public string BookingId { get; set; } = string.Empty;

    [JsonPropertyName("client_name")]
    public string ClientName { get; set; } = string.Empty;

    [JsonPropertyName("client_phone")]
    public string ClientPhone { get; set; } = string.Empty;

    [JsonPropertyName("deceased_name")]
    public string DeceasedName { get; set; } = string.Empty;

    [JsonPropertyName("deceased_id_number")]
    public string DeceasedIdNumber { get; set; } = string.Empty;

    [JsonPropertyName("date_of_death")]
    public DateTime DateOfDeath { get; set; }

    [JsonPropertyName("burial_date")]
    public DateTime BurialDate { get; set; }

    [JsonPropertyName("package_name")]
    public string PackageName { get; set; } = string.Empty;

    [JsonPropertyName("casket_model")]
    public string CasketModel { get; set; } = string.Empty;

    [JsonPropertyName("pickup_address")]
    public string PickupAddress { get; set; } = string.Empty;

    [JsonPropertyName("cemetery_name")]
    public string CemeteryName { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = "Pending";

    [JsonPropertyName("hearse_id")]
    public int? HearseId { get; set; }

    [JsonPropertyName("hearse_assigned")]
    public string? HearseAssigned { get; set; }

    [JsonPropertyName("driver_assigned")]
    public string? DriverAssigned { get; set; }

    [JsonPropertyName("equipment_assigned")]
    public string? EquipmentAssigned { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}