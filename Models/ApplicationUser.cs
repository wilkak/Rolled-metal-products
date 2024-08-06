using Microsoft.AspNetCore.Identity;

namespace Rolled_metal_products.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }

    }
}
