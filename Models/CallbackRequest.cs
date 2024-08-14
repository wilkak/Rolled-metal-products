using Microsoft.Build.Framework;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Rolled_metal_products.Models
{
    public class CallbackRequest
    {
        [Key]
        public int Id { get; set; }
        
        public string PhoneNumber { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }
   
        public string Text { get; set; }
        [DisplayName("Дата")]
        public DateTime Date { get; set; }
    }
}
