using System.ComponentModel.DataAnnotations;

namespace NorthwindConsole.Model;

public partial class Product
{
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Product name is required.")]
    public string ProductName { get; set; } = null!;

    [Required(ErrorMessage = "Supplier is required.")]
    public int? SupplierId { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    public int? CategoryId { get; set; }

    public string? QuantityPerUnit { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Unit price must be non-negative.")]
    public decimal? UnitPrice { get; set; }

    [Range(0, short.MaxValue, ErrorMessage = "Units in stock must be non-negative.")]
    public short? UnitsInStock { get; set; }

    [Range(0, short.MaxValue, ErrorMessage = "Units on order must be non-negative.")]
    public short? UnitsOnOrder { get; set; }

    [Range(0, short.MaxValue, ErrorMessage = "Reorder level must be non-negative.")]
    public short? ReorderLevel { get; set; }

    public bool Discontinued { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = [];

    public virtual Supplier? Supplier { get; set; }
}
