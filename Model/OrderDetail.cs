namespace NorthwindConsole.Model;
using System.ComponentModel.DataAnnotations;

public partial class OrderDetail
{
    public int OrderDetailsId { get; set; }

    [Required(ErrorMessage = "Order is required.")]
    public int OrderId { get; set; }

    [Required(ErrorMessage = "Product is required.")]
    public int ProductId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Unit price must be non-negative.")]
    public decimal UnitPrice { get; set; }

    [Range(1, short.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public short Quantity { get; set; }

    [Range(0, 1, ErrorMessage = "Discount must be between 0 and 1.")]
    public decimal Discount { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
