using System.Diagnostics;
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
    public string GetAvatarRelativePatch(string userId)=> Path.Combine(_storageFolderName, userId, "image", "avatar.png");
    private string GetTempAvatarRelativePatch(string userId)=> Path.Combine(_storageFolderName, userId, "image", $"{TmpPrefix}avatar.png");
    private string GetTempVideoRelativePatch(string userId,string videoId)=> Path.Combine(_storageFolderName, userId, "video", $"{videoId}", $"{TmpPrefix}{videoId}.mp4");
    public string GetVideoRelativePatch(string userId,string videoId)=> Path.Combine(_storageFolderName, userId, "video", $"{videoId}", $"{videoId}.mp4");
    #endregion
        
    #region AbsolutePaths
    private readonly string _uploadPath;
    private string GetAvatarPatch(string userId)=> Path.Combine(_uploadPath, userId, "image", "avatar.png");
    private string GetTempAvatarPatch(string userId)=> Path.Combine(_uploadPath, userId, "image", $"{TmpPrefix}avatar.png");
    private string GetTempVideoPatch(string userId,string videoId)=> Path.Combine(_uploadPath, userId, "video", $"{videoId}", $"{TmpPrefix}{videoId}.mp4");
    public string GetVideoPatch(string userId,string videoId)=> Path.Combine(_uploadPath, userId, "video", $"{videoId}", $"{videoId}.mp4");
    #endregion
        
        
    private const string TmpPrefix = "_tmp_";
    private const int slidesAmount = 10;
    private const int MaxAvatarSize = 10 * 1024 * 1024; // Max file size of 10 MB
    private const int MaxVideoSize = 500 * 1024 * 1024; // Max file size of 500 MB
    private static readonly List<string> AcceptedImageExtensions = [".png", ".jpg", ".jpeg"];
    private static readonly List<string> AcceptedVideoExtensions= [".mp4"];

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

    private async Task<KeyValuePair<string, Action<bool>>> PostponedSaveVideoWithPreviewAsync(MediaSaveRequest mediaSaveRequest)
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

    private async Task<KeyValuePair<string, Action<bool>>> PostponedSaveImageWithPreviewAsync(MediaSaveRequest mediaSaveRequest)
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
            await GenerateSlides(filePath);
        }
        else
                Directory.Delete( Path.GetDirectoryName(filePath)??throw new InvalidOperationException("File path is null"), true);
        
    }
    private static async Task SaveFileAsync(string filePath, FileInfo file, int maxFileSize)
    {
        await using var stream = file.OpenReadStream(maxFileSize);
        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await stream.CopyToAsync(fileStream);
    }

    private static bool IsValidImageFormat(string fileName, IEnumerable<string> validFileExtensions) =>
        validFileExtensions.Contains(Path.GetExtension(fileName).ToLower());

    private static async Task GenerateSlides(string filePath)
    {
        var slidesPath = Path.Combine(
            Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException("File path is null"),
            "slides");

        if (Directory.Exists(slidesPath))
            Directory.Delete(slidesPath, true); // Clear old slides

        Directory.CreateDirectory(slidesPath);
        string outputThumbnailPath = Path.Combine(slidesPath, "thumbnail.png"); // Save thumbnail in slides folder

        var videoProcessor = new VideoProcessor();
        try
        {
            // Extract the frame and save it as a thumbnail
            await videoProcessor.ExtractFrame(filePath, outputThumbnailPath);
            Console.WriteLine($"Thumbnail saved at: {outputThumbnailPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        // Optionally, you can implement slide generation logic here
        // For example, generating additional slides from the video
    }


}
public class VideoProcessor
{
    public async Task ExtractFrame(string videoFilePath, string outputThumbnailPath)
    {
        if (!File.Exists(videoFilePath))
            throw new ApplicationException("Video file does not exist. Please provide a valid path.");

        string ffmpegPath = "ffmpeg"; // Ensure ffmpeg is in PATH
        var startInfo = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = $"-i \"{videoFilePath}\" -ss 00:00:01 -vframes 1 \"{outputThumbnailPath}\"", // Extract frame at 1 second
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        // Read output from the process
        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new ApplicationException($"Error extracting frame: {error}");
        }
    }
}