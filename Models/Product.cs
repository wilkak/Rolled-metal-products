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

       // public string ShortDesc { get; set; }

        [DisplayName("Описание")]
        [Required(ErrorMessage = "Не указано описание")]
        public string Description { get; set; }
        [DisplayName("Цена")]
        [Range(1, int.MaxValue)]
        public double Price { get; set; }

        [DisplayName("Прошлая цена")]
        public double? OldPrice { get; set; }

        [DisplayName("ГОСТ")]
        public string? Standard { get; set; }

        [DisplayName("Тип сплава")]
        public string? TypeOfAlloy { get; set; }

        [DisplayName("Марка")]
        public string? Stamp { get; set; }

        [DisplayName("Диаметр")]
        public decimal? Diameter { get; set; }

        [DisplayName("Толщина")]
        public decimal? Thickness { get; set; }

        [DisplayName("Ширина")]
        public decimal? Width { get; set; }

        [DisplayName("Наружный диаметр")]
        public decimal? ExternalDiameter { get; set; }

        public string? ImageName { get; set; }

        [NotMapped]
        [Range(1, 10000, ErrorMessage = "Sqft must be greater than 0.")]
        public int TempSqFt { get; set; }
        [Display(Name = "Тип категории")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }
    }
}
