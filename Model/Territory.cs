namespace NorthwindConsole.Model;

using System.ComponentModel.DataAnnotations;

public partial class Territory
{
    [Required(ErrorMessage = "Territory ID is required.")]
    public string TerritoryId { get; set; } = null!;

    [Required(ErrorMessage = "Territory description is required.")]
    public string TerritoryDescription { get; set; } = null!;

    [Required(ErrorMessage = "Region is required.")]
    public int RegionId { get; set; }

    public virtual Region Region { get; set; } = null!;

    public virtual ICollection<Employee> Employees { get; set; } = [];
}
