namespace LinhGo.ERP.Application.DTOs.Products;

public class ProductDetailsDto : ProductDto
{
    public List<ProductVariantDto> Variants { get; set; } = new();
    public List<ProductStockDto> Stocks { get; set; } = new();
}

