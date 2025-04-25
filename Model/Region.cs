namespace NorthwindConsole.Model;
using System.ComponentModel.DataAnnotations;

public partial class Region
{
    public int RegionId { get; set; }

    [Required(ErrorMessage = "Region description is required.")]
    public string RegionDescription { get; set; } = null!;

    public virtual ICollection<Territory> Territories { get; set; } = [];
}
