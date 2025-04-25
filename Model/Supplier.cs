namespace NorthwindConsole.Model;
using System.ComponentModel.DataAnnotations;

public partial class Supplier
{
    public int SupplierId { get; set; }

    [Required(ErrorMessage = "Company name is required.")]
    public string CompanyName { get; set; } = null!;

    public string? ContactName { get; set; }

    public string? ContactTitle { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Region { get; set; }

    public string? PostalCode { get; set; }

    public string? Country { get; set; }

    [Phone(ErrorMessage = "Invalid phone number.")]
    public string? Phone { get; set; }

    [Phone(ErrorMessage = "Invalid fax number.")]
    public string? Fax { get; set; }

    public virtual ICollection<Product> Products { get; set; } = [];
}
