using Microsoft.AspNetCore.Mvc.Rendering;

namespace Rolled_metal_products.Models.ViewModels
{
    public class CatalogVM
    {
        public List<Category> ParentCategories { get; set; }
        public List<Category> ChildCategories { get; set; }

        public string? ParentCategoryName { get; set; }

        public List<Category>? BreadcrumbCategories { get; set; }
    }
}
