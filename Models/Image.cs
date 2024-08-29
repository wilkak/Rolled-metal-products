using System.ComponentModel.DataAnnotations;

namespace Rolled_metal_products.Models
{
    public class Image
    {
        [Key]
        public int Id { get; set; } // Уникальный идентификатор изображения
        public string FileName { get; set; } // Имя файла изображения
        public string ContentType { get; set; } // Тип содержимого изображения (например, image/jpeg)
        public byte[] Data { get; set; } // Данные изображения в виде байтового массива
    }
}
