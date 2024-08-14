namespace Rolled_metal_products.Models.ViewModels
{
    public class DetailsVM
    {
        public DetailsVM() 
        {
            Product = new Product();
        }

        public Product Product { get; set; }
        public bool ExistsInCart { get; set; }
        public List<Category> BreadcrumbCategories { get; set; }
    }
}
