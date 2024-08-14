using Microsoft.AspNetCore.Mvc.Rendering;

namespace Rolled_metal_products.Models.ViewModels
{
    public class ProductVM
    {
        public Product Product { get; set; }

        public bool ExistsInCart { get; set; }
    }
}
