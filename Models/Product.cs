using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rolled_metal_products.Models
{
    public class Product
    {
        public Product()
        {
            TempSqFt = 1;
        }

        [Key]
        public int Id { get; set; }
        [DisplayName("Название")]
        [Required(ErrorMessage = "Не указано имя")]
        public string Name { get; set; }
        [DisplayName("Описание")]
        [Required(ErrorMessage = "Не указано описание")]
        public string Description { get; set; }
        [DisplayName("Цена")]
        public double Price { get; set; }
        [DisplayName("Прошлая цена")]
        public double? OldPrice { get; set; }
        public string? ImageName { get; set; }
        [Display(Name = "Тип категории")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }
        public ICollection<ProductParameter>? ProductParameters { get; set; }

        [NotMapped]
        [Range(1, 10000, ErrorMessage = "Sqft must be greater than 0.")]
        public int TempSqFt { get; set; }
    }
}
