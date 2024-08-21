namespace Rolled_metal_products.Models.ViewModels
{
    public class FilterProductsRequestVM
    {
        public Dictionary<string, List<string>> SelectedParameters { get; set; }
        public int CategoryId { get; set; }
    }
}
