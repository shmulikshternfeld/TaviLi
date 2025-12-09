using System.IO;
using System.Threading.Tasks;

namespace TaviLi.Application.Common.Interfaces
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(Stream imageStream, string fileName, CancellationToken cancellationToken = default);
    }
}
