using System.Text.Json.Serialization;

namespace Mangojane.FuneralManagement.Web.Models;

public class Policy
{
    [JsonPropertyName("policy_number")]
    public string PolicyNumber { get; set; } = string.Empty;

    [JsonPropertyName("client_name")]
    public string ClientName { get; set; } = string.Empty;

    [JsonPropertyName("society_name")]
    public string SocietyName { get; set; } = "Mangojane Funeral Plan";

    [JsonPropertyName("plan_type")]
    public string PlanType { get; set; } = string.Empty;

    [JsonPropertyName("monthly_premium")]
    public decimal MonthlyPremium { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "Active";

    [JsonPropertyName("waiting_period_expiry")]
    public DateTime WaitingPeriodExpiry { get; set; }
}

public class PolicyPayment
{
    [JsonPropertyName("policy_number")]
    public string PolicyNumber { get; set; } = string.Empty;

    [JsonPropertyName("reference_number")]
    public string ReferenceNumber { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("payment_date")]
    public DateTime PaymentDate { get; set; }

    [JsonPropertyName("payment_method")]
    public string PaymentMethod { get; set; } = string.Empty;

    [JsonPropertyName("received_by")]
    public string ReceivedBy { get; set; } = "Mangojane Staff";
}