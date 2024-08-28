using System.IO;
using System.Threading.Tasks;

namespace Rolled_metal_products.Repository.IRepository
{
    public interface IImageRepository
    {
        Task<string> UploadImageAsync(Stream imageStream, string fileName);
        Task<Stream> DownloadImageAsync(string fileId);
    }
}
