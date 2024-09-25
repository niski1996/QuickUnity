using QuickUnity.Entities;
using QuickUnity.Entities.Enums;
using FileInfo = Radzen.FileInfo;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace QuickUnity.Services
{
    public class MediaStorageService
    {
        private readonly string _uploadPath;
        private readonly string tmpPrefix = "_tmp_";
        private const int MaxFileSize = 10 * 1024 * 1024; // Max file size of 10 MB
        
        // Accepted image extensions
        private static readonly List<string> acceptedImageExtensions = new List<string> { ".png", ".jpg", ".jpeg" };

        public MediaStorageService(IConfiguration configuration)
        {
            var storageFolderName = configuration["FileStorage:StorageFolderName"]
                                    ?? throw new NullReferenceException("Provide file storage name in appsettings");

            _uploadPath = Path.Combine(AppContext.BaseDirectory, storageFolderName);

            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public string GetAvatarPatch(string UserId, string ImageId)
        {
            // Implementation for getting avatar path based on UserId and ImageId
            return Path.Combine(_uploadPath, UserId, "image", $"{ImageId}.png");
        }

        public string GetVideoPatch(string UserId, string VideoId)
        {
            // Implementation for getting video path based on UserId and VideoId
            return Path.Combine(_uploadPath, UserId, "video", $"{VideoId}.mp4");
        }

        private void InitializeUserStorage(string userId)
        {
            var userStoragePath = Path.Combine(_uploadPath, userId);
            if (Directory.Exists(userStoragePath)) return;

            Directory.CreateDirectory(userStoragePath);
            Directory.CreateDirectory(Path.Combine(userStoragePath, "image"));
            Directory.CreateDirectory(Path.Combine(userStoragePath, "video"));
        }

        public async Task<KeyValuePair<string, Action<bool>>> SaveFileAsync(MediaSaveRequest mediaSaveRequest)
        {
            InitializeUserStorage(mediaSaveRequest.OwnerId);
            switch (mediaSaveRequest.mediaType)
            {
                case MultimediaType.Image:
                    return await PostponedSaveImageWithPreviewAsync(mediaSaveRequest);
                default:
                    throw new Exception($"Unknown media type: {mediaSaveRequest.mediaType}");
            }
        }

        private async Task<KeyValuePair<string, Action<bool>>> PostponedSaveImageWithPreviewAsync(MediaSaveRequest mediaSaveRequest)
        {
            // Check if image has a valid format
            if (!IsValidImageFormat(mediaSaveRequest.fileContent.Name))
                throw new Exception("Invalid image format.");

            // Path for temporary file
            var userStoragePath = Path.Combine(_uploadPath, mediaSaveRequest.OwnerId.ToString(), "image");
            var tmpFilePath = Path.Combine(userStoragePath, $"{tmpPrefix}{mediaSaveRequest.MediaId}.png");
            var filePath = Path.Combine(userStoragePath, $"{mediaSaveRequest.MediaId}.png");

            // Save the temporary file
            await SaveFileAsync(tmpFilePath, mediaSaveRequest.fileContent, MaxFileSize);
            
            await ResizeImageAsync(tmpFilePath, filePath, 400, 400);
            return new KeyValuePair<string, Action<bool>>(tmpFilePath, confirmed => HandleTempFile(tmpFilePath, filePath, confirmed));
        }
        
        private async Task ResizeImageAsync(string inputFilePath, string outputFilePath, int width, int height)
        {
            using var image = await Image.LoadAsync(inputFilePath);
    
            // Resize the image while maintaining aspect ratio
            var resizeOptions = new ResizeOptions
            {
                Size = new SixLabors.ImageSharp.Size(width, height),
                Mode = ResizeMode.Max // Maintain aspect ratio and fit within the specified size
            };

            image.Mutate(x => x.Resize(resizeOptions));
    
            // Create a new image with black background of the target size
            using var outputImage = new Image<Rgba32>(width, height, SixLabors.ImageSharp.Color.Black);
    
            // Calculate the position to center the resized image on the black background
            int x = (width - image.Width) / 2;
            int y = (height - image.Height) / 2;

            // Draw the resized image on top of the black background
            outputImage.Mutate(ctx => ctx.DrawImage(image, new SixLabors.ImageSharp.Point(x, y), 1f));

            // Save the resulting image
            await outputImage.SaveAsync(outputFilePath);
        }


        // private Size CalculateNewDimensions(int originalWidth, int originalHeight, int targetWidth, int targetHeight)
        // {
        //     var ratioX = (double)targetWidth / originalWidth;
        //     var ratioY = (double)targetHeight / originalHeight;
        //     var ratio = Math.Min(ratioX, ratioY);
        //
        //     var newWidth = (int)(originalWidth * ratio);
        //     var newHeight = (int)(originalHeight * ratio);
        //
        //     return new Size(newWidth, newHeight);
        // }

        private async Task PostponedSaveVideoAsync(MediaSaveRequest mediaSaveRequest)
        {
            // Placeholder for video saving logic
            var userStoragePath = Path.Combine(_uploadPath, mediaSaveRequest.OwnerId.ToString(), "video");
            var videoFilePath = Path.Combine(userStoragePath, $"{mediaSaveRequest.MediaId}.mp4");

            await SaveFileAsync(videoFilePath, mediaSaveRequest.fileContent, MaxFileSize);
        }

        private async Task HandleTempFile(string tempFilePath, string filePath, bool saveConfirmed)
        {
            if (saveConfirmed)
            {
                // If save is confirmed, delete existing file if exists, then rename temp file
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                File.Move(tempFilePath, filePath);
                Console.WriteLine($"File saved to {filePath}");
            }
            else
            {
                // Delete temp file
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                    Console.WriteLine($"Temporary file {tempFilePath} deleted.");
                }
            }
        }

        private async Task SaveFileAsync(string filePath, FileInfo file, int maxFileSize)
        {
            using (var stream = file.OpenReadStream(maxFileSize))
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await stream.CopyToAsync(fileStream);
                }
                Console.WriteLine($"File {file.Name} uploaded successfully.");
            }
        }

        private bool IsValidImageFormat(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();
            return acceptedImageExtensions.Contains(extension);
        }
    }
}
