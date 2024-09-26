using MediaToolkit;
using MediaToolkit.Model;
using QuickUnity.Entities;
using QuickUnity.Entities.Enums;
using FileInfo = Radzen.FileInfo;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


namespace QuickUnity.Services;

public class MediaStorageService
{
    public bool AvatarAvailable(string userId) => File.Exists(GetAvatarPatch(userId));

    private readonly string _storageFolderName;

    #region RelativePaths

    public string GetAvatarRelativePatch(string userId) =>
        Path.Combine(_storageFolderName, userId, "image", "avatar.png");

    private string GetTempAvatarRelativePatch(string userId) =>
        Path.Combine(_storageFolderName, userId, "image", $"{TmpPrefix}avatar.png");

    private string GetTempVideoRelativePatch(string userId, string videoId) => Path.Combine(_storageFolderName, userId,
        "video", $"{videoId}", $"{TmpPrefix}{videoId}.mp4");

    public string GetVideoRelativePatch(string userId, string videoId) =>
        Path.Combine(_storageFolderName, userId, "video", $"{videoId}", $"{videoId}.mp4");

    #endregion

    #region AbsolutePaths

    private readonly string _uploadPath;
    private string GetAvatarPatch(string userId) => Path.Combine(_uploadPath, userId, "image", "avatar.png");

    private string GetTempAvatarPatch(string userId) =>
        Path.Combine(_uploadPath, userId, "image", $"{TmpPrefix}avatar.png");

    private string GetTempVideoPatch(string userId, string videoId) => Path.Combine(_uploadPath, userId, "video",
        $"{videoId}", $"{TmpPrefix}{videoId}.mp4");

    public string GetVideoPatch(string userId, string videoId) =>
        Path.Combine(_uploadPath, userId, "video", $"{videoId}", $"{videoId}.mp4");

    #endregion


    private const string TmpPrefix = "_tmp_";
    private const int slidesAmount = 10;
    private const int MaxAvatarSize = 10 * 1024 * 1024; // Max file size of 10 MB
    private const int MaxVideoSize = 500 * 1024 * 1024; // Max file size of 500 MB
    private static readonly List<string> AcceptedImageExtensions = [".png", ".jpg", ".jpeg"];
    private static readonly List<string> AcceptedVideoExtensions = [".mp4"];

    public MediaStorageService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _storageFolderName = configuration["FileStorage:StorageFolderName"]
                             ?? throw new NullReferenceException("Provide file storage name in appsettings");

        _uploadPath = Path.Combine(environment.WebRootPath, _storageFolderName);

        if (!Directory.Exists(_uploadPath))
            Directory.CreateDirectory(_uploadPath);

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
        return mediaSaveRequest.mediaType switch
        {
            MultimediaType.Image => await PostponedSaveImageWithPreviewAsync(mediaSaveRequest),
            MultimediaType.Video => await PostponedSaveVideoWithPreviewAsync(mediaSaveRequest),
            _ => throw new Exception($"Unknown media type: {mediaSaveRequest.mediaType}")
        };
    }

    private async Task<KeyValuePair<string, Action<bool>>> PostponedSaveVideoWithPreviewAsync(
        MediaSaveRequest mediaSaveRequest)
    {
        if (!IsValidImageFormat(mediaSaveRequest.fileContent.Name, AcceptedVideoExtensions))
            throw new Exception("Invalid video format.");
        var userId = mediaSaveRequest.OwnerId;
        var videoId = mediaSaveRequest.MediaId;
        Directory.CreateDirectory(Path.Combine(_uploadPath, userId, "video", videoId));
        await SaveFileAsync(GetTempVideoPatch(userId, videoId), mediaSaveRequest.fileContent, MaxVideoSize);

        return new KeyValuePair<string, Action<bool>>(GetTempVideoRelativePatch(userId, videoId), confirmed
            => HandleTempVideo(GetTempVideoPatch(userId, videoId), GetVideoPatch(userId, videoId), confirmed));
    }

    private async Task<KeyValuePair<string, Action<bool>>> PostponedSaveImageWithPreviewAsync(
        MediaSaveRequest mediaSaveRequest)
    {
        if (!IsValidImageFormat(mediaSaveRequest.fileContent.Name, AcceptedImageExtensions))
            throw new Exception("Invalid image format.");
        var userId = mediaSaveRequest.OwnerId;
        await SaveFileAsync(GetTempAvatarPatch(userId), mediaSaveRequest.fileContent, MaxAvatarSize);

        await ResizeImageAsync(GetTempAvatarPatch(userId), 400, 400);
        return new KeyValuePair<string, Action<bool>>(GetTempAvatarRelativePatch(userId), confirmed
            => HandleTempAvatars(GetTempAvatarPatch(userId), GetAvatarPatch(userId), confirmed));
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
        var x = (width - image.Width) / 2;
        var y = (height - image.Height) / 2;

        // Draw the resized image on top of the black background
        outputImage.Mutate(ctx => ctx.DrawImage(image, new Point(x, y), 1f));

        // Save the resulting image to the same file (overwrite the original)
        await outputImage.SaveAsync(inputFilePath);
    }

    private static void HandleTempAvatars(string tempFilePath, string filePath, bool saveConfirmed)
    {
        if (saveConfirmed)
        {
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

    private static async Task HandleTempVideo(string tempFilePath, string filePath, bool saveConfirmed)
    {
        if (saveConfirmed)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
            File.Move(tempFilePath, filePath);
            await GenerateSlides(filePath, 10);
        }
        else
            Directory.Delete(
                Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException("File path is null"), true);

    }

    private static async Task SaveFileAsync(string filePath, FileInfo file, int maxFileSize)
    {
        await using var stream = file.OpenReadStream(maxFileSize);
        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await stream.CopyToAsync(fileStream);
    }

    private static bool IsValidImageFormat(string fileName, IEnumerable<string> validFileExtensions) =>
        validFileExtensions.Contains(Path.GetExtension(fileName).ToLower());

    private static async Task GenerateSlides(string filePath, int thumbnailCount)
    {
        var slidesPath = Path.Combine(
            Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException("File path is null"),
            "slides");

        if (Directory.Exists(slidesPath))
            Directory.Delete(slidesPath, true); // Clear old slides

        Directory.CreateDirectory(slidesPath);

        var videoProcessor = new VideoProcessor();
        try
        {
            // Get the duration of the video
            double videoDuration = videoProcessor.GetLengthInSeconds(filePath);
            var interval = videoDuration / thumbnailCount+1; // Calculate interval

            for (var i = 0; i < thumbnailCount; i++)
            {
                var outputThumbnailPath =
                    Path.Combine(slidesPath, $"{i}.png"); 
                await videoProcessor.ExtractFrame(filePath, outputThumbnailPath,
                    (int)(i * interval)); // Extract at equal intervals
                Console.WriteLine($"Thumbnail saved at: {outputThumbnailPath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}