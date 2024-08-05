using Microsoft.AspNetCore.Identity;

namespace Rolled_metal_products.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

    }
}
