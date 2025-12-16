using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using TaviLi.Application.Common.Interfaces;

namespace TaviLi.Infrastructure.Services
{
    public class CloudinaryService : IImageService
    {
        private readonly IConfiguration _configuration;
        private Cloudinary? _cloudinary;

        public CloudinaryService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private Cloudinary GetCloudinary()
        {
            if (_cloudinary != null) return _cloudinary;

            // קריאת המפתחות לפי השמות שהוגדרו בשרת (CloudinarySettings__...)
            var cloudName = _configuration["CloudinarySettings:CloudName"];
            var apiKey = _configuration["CloudinarySettings:ApiKey"];
            var apiSecret = _configuration["CloudinarySettings:ApiSecret"];

            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                throw new Exception("Cloudinary credentials are missing from configuration. Please set CloudName, ApiKey, and ApiSecret via User Secrets.");
            }

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
            return _cloudinary;
        }

        public async Task<string> UploadImageAsync(Stream imageStream, string fileName, CancellationToken cancellationToken = default)
        {
            var cloudinary = GetCloudinary();

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(fileName, imageStream),
                Transformation = new Transformation().Quality("auto").FetchFormat("auto")
            };

            var uploadResult = await cloudinary.UploadAsync(uploadParams, cancellationToken);
            return uploadResult.SecureUrl.ToString();
        }
    }
}
