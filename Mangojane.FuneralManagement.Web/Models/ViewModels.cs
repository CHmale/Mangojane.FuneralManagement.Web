namespace Mangojane.FuneralManagement.Web.Models;

public class DashboardViewModel
{
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int UpcomingCount { get; set; }
    public int ResourceCount { get; set; }
    public List<Booking> RecentBookings { get; set; } = new();
}

public class ResourceAllocationViewModel
{
    public string BookingId { get; set; } = string.Empty;
    public string DeceasedName { get; set; } = string.Empty;
    public DateTime BurialDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public int SelectedHearseId { get; set; }
    public string SelectedDriver { get; set; } = string.Empty;
    public string EquipmentDetails { get; set; } = "Funeral Marquee Tent & 50 Chairs";
    public List<VehicleAsset> AvailableHearses { get; set; } = new();
    public List<string> AvailableDrivers { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class RecordPaymentViewModel
{
    public string PolicyNumber { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.Today;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = "EFT / Cash at Front Desk";
}