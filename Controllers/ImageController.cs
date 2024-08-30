using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Rolled_metal_products.Models;

namespace Rolled_metal_products.Controllers
{
    public class ImageController : Controller
    {
        private readonly IMongoCollection<ImageCategory> _imagesCategoryCollection;
        private readonly IMongoCollection<ImageProduct> _imagesProductCollection;

        public ImageController(IMongoDatabase mongoDatabase)
        {
            _imagesCategoryCollection = mongoDatabase.GetCollection<ImageCategory>("ImagesCategory");
            _imagesProductCollection = mongoDatabase.GetCollection<ImageProduct>("ImagesProduct");
        }

       public async Task<IActionResult> GetImageCategory(int catId)
        {
            var image = await _imagesCategoryCollection.Find(img => img.CategoryId == catId).FirstOrDefaultAsync();
            if (image == null)
            {
                return NotFound();
            }

            return File(image.Data, image.ContentType);
        }

        public async Task<IActionResult> GetImageProduct(int prodId)
        {
            var image = await _imagesProductCollection.Find(img => img.ProductId == prodId).FirstOrDefaultAsync();
            if (image == null)
            {
                return NotFound();
            }

            return File(image.Data, image.ContentType);
        }
    }
}
