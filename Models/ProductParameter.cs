using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rolled_metal_products.Models
{
    public class ProductParameter
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        public int CategoryParameterId { get; set; }

        [ForeignKey("CategoryParameterId")]
        public virtual CategoryParameter CategoryParameter { get; set; }
       // public string Name { get; set; }
        public string Value { get; set; }
    }
}
