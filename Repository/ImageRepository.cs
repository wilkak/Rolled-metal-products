using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Rolled_metal_products.Repository.IRepository;
using System.IO;
using System.Threading.Tasks;

namespace Rolled_metal_products.Repository
{
    public class ImageRepository : IImageRepository
    {
        private readonly IGridFSBucket _gridFS;

        public ImageRepository(IMongoDatabase mongoDatabase)
        {
            _gridFS = new GridFSBucket(mongoDatabase);
        }

        public async Task<string> UploadImageAsync(Stream imageStream, string fileName)
        {
            var options = new GridFSUploadOptions
            {
                Metadata = new BsonDocument { { "FileName", fileName } }
            };
            var fileId = await _gridFS.UploadFromStreamAsync(fileName, imageStream, options);
            return fileId.ToString();
        }

        public async Task<Stream> DownloadImageAsync(string fileId)
        {
            var objectId = new ObjectId(fileId);
            return await _gridFS.OpenDownloadStreamAsync(objectId);
        }
    }
}
