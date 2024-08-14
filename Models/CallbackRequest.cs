using Microsoft.Build.Framework;
using System.ComponentModel;

namespace Rolled_metal_products.Models
{
    public class CallbackRequest
    {
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Text { get; set; }
        [DisplayName("Дата")]
        public DateTime Date { get; set; }
    }
}
