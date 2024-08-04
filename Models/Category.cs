using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Rolled_metal_products.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [DisplayName("Название")]
        [Required(ErrorMessage = "Название не указано")]
        public string Name { get; set; }
        [DisplayName("Порядок")]
        [Required(ErrorMessage = "Порядокне указан")]
        [Range(1, int.MaxValue, ErrorMessage = "Параметр должен быть больше нуля")]
        public int DisplayOrder { get; set; }
    }
}
