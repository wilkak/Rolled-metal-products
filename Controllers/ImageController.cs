using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Rolled_metal_products.Models;

namespace Rolled_metal_products.Controllers
{
    public class ImageController : Controller
    {
        private readonly IMongoCollection<Image> _imagesCollection;

        public ImageController(IMongoDatabase mongoDatabase)
        {
            _imagesCollection = mongoDatabase.GetCollection<Image>("Images");
        }

        public async Task<IActionResult> GetImage(int imgId)
        {
            var image = await _imagesCollection.Find(img => img.Id == imgId).FirstOrDefaultAsync();
            if (image == null)
            {
                return NotFound();
            }

            return File(image.Data, image.ContentType);
        }
    }
}
