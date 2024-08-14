namespace Rolled_metal_products.Models.ViewModels
{
    public class CreateProductVM
    {
        public Product Product { get; set; }
        public List<ProductParameterVM> CategoryParameters { get; set; } = new List<ProductParameterVM>();
    }
}
