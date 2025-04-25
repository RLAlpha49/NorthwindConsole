namespace NorthwindConsole.Model;

using System.ComponentModel.DataAnnotations;

public partial class Customer
{
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "Company name is required.")]
    public string CompanyName { get; set; } = null!;

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Region { get; set; }

    public string? PostalCode { get; set; }

    public string? Country { get; set; }

    [Phone(ErrorMessage = "Invalid phone number.")]
    public string? Phone { get; set; }

    [Phone(ErrorMessage = "Invalid fax number.")]
    public string? Fax { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string? Email { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = [];
}
