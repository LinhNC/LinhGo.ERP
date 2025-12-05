using LinhGo.ERP.Domain.Common;

namespace LinhGo.ERP.Domain.Inventory.Entities;

public class ProductCategory : TenantEntity
{
    public required string Name { get; set; }
    public required string Code { get; set; }
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public int Level { get; set; } = 0;
    public string? Path { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
    
    public virtual ProductCategory? ParentCategory { get; set; }
    public virtual ICollection<ProductCategory>? SubCategories { get; set; }
    public virtual ICollection<Product>? Products { get; set; }
}

