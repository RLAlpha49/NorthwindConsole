namespace NorthwindConsole.Model;

using System.ComponentModel.DataAnnotations;

public partial class Employee
{
    public int EmployeeId { get; set; }

    [Required(ErrorMessage = "Last name is required.")]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "First name is required.")]
    public string FirstName { get; set; } = null!;

    public string? Title { get; set; }

    public string? TitleOfCourtesy { get; set; }

    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? HireDate { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Region { get; set; }

    public string? PostalCode { get; set; }

    public string? Country { get; set; }

    [Phone(ErrorMessage = "Invalid home phone number.")]
    public string? HomePhone { get; set; }

    public string? Extension { get; set; }

    public int? ReportsTo { get; set; }

    public virtual ICollection<Employee> InverseReportsToNavigation { get; set; } = [];

    public virtual ICollection<Order> Orders { get; set; } = [];

    public virtual Employee? ReportsToNavigation { get; set; }

    public virtual ICollection<Territory> Territories { get; set; } = [];
}
