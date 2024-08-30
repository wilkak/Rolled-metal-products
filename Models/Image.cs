using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace Rolled_metal_products.Models
{
    public class ImageCategory
    {
        public ObjectId Id { get; set; }
        public int CategoryId { get; set; } // Идентификатор категории
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public byte[] Data { get; set; }
    }

    public class ImageProduct
    {
        public ObjectId Id { get; set; }
        public int ProductId { get; set; } // Идентификатор товара
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public byte[] Data { get; set; }
    }
}
