using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [Required(ErrorMessage = "Порядок не указан")]
        [Range(1, int.MaxValue, ErrorMessage = "Параметр должен быть больше нуля")]
        public int DisplayOrder { get; set; }

        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public virtual Category? ParentCategory { get; set; }

        public virtual ICollection<Category>? SubCategories { get; set; }

        public virtual ICollection<CategoryParameter>? CategoryParameters { get; set; }
    }
}
