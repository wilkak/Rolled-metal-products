namespace Rolled_metal_products.Models.ViewModels
{
    public class CreateCategoryVM
    {
        public Category Category { get; set; }
        public List<string> Parameters { get; set; } = new List<string>();
    }
}
