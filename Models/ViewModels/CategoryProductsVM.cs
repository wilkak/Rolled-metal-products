namespace Rolled_metal_products.Models.ViewModels
{
    public class CategoryProductsVM
    {
        public Category Category { get; set; }
        public List<ProductVM> Products { get; set; }
        public Dictionary<string, List<string>> CategoryParameters { get; set; }
        public List<Category> BreadcrumbCategories { get; set; }
    }
}
