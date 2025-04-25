namespace NorthwindConsole.Model;

using System.ComponentModel.DataAnnotations;

public partial class Shipper
{
    public int ShipperId { get; set; }

    [Required(ErrorMessage = "Company name is required.")]
    public string CompanyName { get; set; } = null!;

    [Phone(ErrorMessage = "Invalid phone number.")]
    public string? Phone { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = [];
}
