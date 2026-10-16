using System.Text.Json.Serialization;

namespace Mangojane.FuneralManagement.Web.Models;

public class VehicleAsset
{
    [JsonPropertyName("vehicle_id")]
    public int VehicleId { get; set; }

    [JsonPropertyName("registration_number")]
    public string RegistrationNumber { get; set; } = string.Empty;

    [JsonPropertyName("vehicle_type")]
    public string VehicleType { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("operational_status")]
    public string OperationalStatus { get; set; } = "Available";
}