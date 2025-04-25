namespace NorthwindConsole.Model;

using System.ComponentModel.DataAnnotations;

public partial class Order
{
    public int OrderId { get; set; }

    [Required(ErrorMessage = "Customer is required.")]
    public int? CustomerId { get; set; }

    [Required(ErrorMessage = "Employee is required.")]
    public int? EmployeeId { get; set; }

    [DataType(DataType.Date)]
    public DateTime? OrderDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? RequiredDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? ShippedDate { get; set; }

    public int? ShipVia { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Freight must be non-negative.")]
    public decimal? Freight { get; set; }

    public string? ShipName { get; set; }

    public string? ShipAddress { get; set; }

    public string? ShipCity { get; set; }

    public string? ShipRegion { get; set; }

    public string? ShipPostalCode { get; set; }

    public string? ShipCountry { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = [];

    public virtual Shipper? ShipViaNavigation { get; set; }
}
