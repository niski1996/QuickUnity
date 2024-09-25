using QuickUnity.Entities;
using QuickUnity.Entities.Enums;
using FileInfo = Radzen.FileInfo;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace QuickUnity.Services
{
    public class MediaStorageService
    {
        private readonly string _uploadPath;
        private readonly string _relativeUploadPath;
        private readonly string _storageFolderName;
        private readonly string _tmpPrefix = "_tmp_";
        private const int MaxFileSize = 10 * 1024 * 1024; // Max file size of 10 MB
        
        private static readonly List<string> acceptedImageExtensions = new() { ".png", ".jpg", ".jpeg" };

        public MediaStorageService(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _storageFolderName = configuration["FileStorage:StorageFolderName"]
                                    ?? throw new NullReferenceException("Provide file storage name in appsettings");

            _uploadPath = Path.Combine(environment.WebRootPath, _storageFolderName);
            _relativeUploadPath = Path.Combine(_storageFolderName);

            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public string GetAvatarRelativePatch(string UserId)
        {
            return Path.Combine(_relativeUploadPath, UserId, "image", "avatar.png");
        }
        public string GetAvatarPatch(string UserId)
        {
            return Path.Combine(_uploadPath, UserId, "image", "avatar.png");
        }
        public string GetVideoPatch(string UserId, string VideoId)
        {
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
            if (!IsValidImageFormat(mediaSaveRequest.fileContent.Name))
                throw new Exception("Invalid image format.");
            
            var userStoragePath = Path.Combine(_uploadPath, mediaSaveRequest.OwnerId, "image");

            var tmpFilePath = Path.Combine(userStoragePath, $"{_tmpPrefix}{mediaSaveRequest.MediaId}.png");
            var filePath = Path.Combine(userStoragePath, $"{mediaSaveRequest.MediaId}.png");
            
            var relativeUserStoragePath = Path.Combine(_relativeUploadPath, mediaSaveRequest.OwnerId, "image");
            var relativeTmpFilePath = Path.Combine(relativeUserStoragePath, $"{_tmpPrefix}{mediaSaveRequest.MediaId}.png");
            
            await SaveFileAsync(tmpFilePath, mediaSaveRequest.fileContent, MaxFileSize);
            
            await ResizeImageAsync(tmpFilePath, 400, 400);
            return new KeyValuePair<string, Action<bool>>(relativeTmpFilePath, confirmed => HandleTempFile(tmpFilePath, filePath, confirmed));
        }
        
        private async Task ResizeImageAsync(string inputFilePath, int width, int height)
        {
            using var image = await Image.LoadAsync(inputFilePath);
            
            var resizeOptions = new ResizeOptions
            {
                Size = new Size(width, height),
                Mode = ResizeMode.Max
            };

            image.Mutate(x => x.Resize(resizeOptions));
            
            using var outputImage = new Image<Rgba32>(width, height, Color.Black);

            // Calculate the position to center the resized image on the black background
            int x = (width - image.Width) / 2;
            int y = (height - image.Height) / 2;

            // Draw the resized image on top of the black background
            outputImage.Mutate(ctx => ctx.DrawImage(image, new SixLabors.ImageSharp.Point(x, y), 1f));

            // Save the resulting image to the same file (overwrite the original)
            await outputImage.SaveAsync(inputFilePath);
        }

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
                    File.Delete(filePath);
                
                File.Move(tempFilePath, filePath);
            }
            else
            {
                if (File.Exists(tempFilePath))
                    File.Delete(tempFilePath);
                
            }
        }

        private async Task SaveFileAsync(string filePath, FileInfo file, int maxFileSize)
        {
            using var stream = file.OpenReadStream(maxFileSize);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await stream.CopyToAsync(fileStream);
            }
            Console.WriteLine($"File {file.Name} uploaded successfully.");
        }

        private bool IsValidImageFormat(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();
            return acceptedImageExtensions.Contains(extension);
        }
    }
}
