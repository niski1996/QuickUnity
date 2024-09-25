using QuickUnity.Entities;
using QuickUnity.Entities.Enums;
using FileInfo = Radzen.FileInfo;

namespace QuickUnity.Services;

public class MediaStorageService
{
    private readonly string _uploadPath;

    private readonly string tmpPrefix = "_tmp_";
    
    public string GetAvatarPatch(string UserId, string ImageId) => throw 
    public string GetVideoPatch(string UserId, string VideoId) => throw 
    
    const List<string> acceptedImageExtensions = {".png", "jpg"...} 
    
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

    private void InitializeUserStorage(string userId)
    {
        var userStoragePath = Path.Combine(_uploadPath, userId);
        if(Directory.Exists(userStoragePath)) return;
        Directory.CreateDirectory(userStoragePath);
        var imageStoragePath = Path.Combine(userStoragePath, "image");
        var videoStoragePath = Path.Combine(userStoragePath, "video");
        Directory.CreateDirectory(imageStoragePath);
        Directory.CreateDirectory(videoStoragePath);
    }
    
    
    public async Task<KeyValuePair<string, Action<bool>>> SaveFileAsync(MediaSaveRequest mediaSaveRequest)
    {
        InitializeUserStorage(mediaSaveRequest.OwnerId);
        switch (mediaSaveRequest.mediaType)
        {
            case MultimediaType.Image:
                return await PostponedSaveImageWithPreviewAsync(mediaSaveRequest);
            case MultimediaType.Video:
                break;
            default: throw new Exception($"Unknown media type: {mediaSaveRequest.mediaType}");
                
        }
        throw new Exception($"media type not yet handled: {mediaSaveRequest.mediaType}");
    }

    private async Task<KeyValuePair<string, Action<bool>>>  PostponedSaveImageWithPreviewAsync(MediaSaveRequest mediaSaveRequest)
    {
        //check if image in good format
        //change format to png
        //resize to 400x400
        var userStoragePath = Path.Combine(_uploadPath, mediaSaveRequest.OwnerId.ToString(), "image");
        //check if in path exist file _tmp_avatar.png if yes, then delete
        var tmpFilePath = Path.Combine(userStoragePath, $"{tmpPrefix}{mediaSaveRequest.MediaId}.png");
        var FilePath = Path.Combine(userStoragePath, $"{mediaSaveRequest.MediaId}.png");

        

        await SaveFileAsync(tmpFilePath, mediaSaveRequest.fileContent, 10 * 1024 * 1024);
        // await File.WriteAllBytesAsync(filePath, mediaSaveRequest.fileContent);
        return new KeyValuePair<string, Action<bool>>( tmpFilePath, (bool confirmed)=>HandleTempFile(tmpFilePath, FilePath, confirmed));
    }

    private Task HandleTempFile(string tempFilePath, string filepath, bool saveConfirmed)
    {
        if (saveConfirmed)
        {
            //delete file form filepatch if exist
            //remove tmpprefx  from previous file
        }
        else
        {
            //delete tmpfile
        }
    }

    private async Task SaveFileAsync(string filePath, FileInfo file, int MaxFileSize)
    {
        using (var stream = file.OpenReadStream(MaxFileSize))
        {
        
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await stream.CopyToAsync(fileStream);
            }
        
            Console.WriteLine($"File {file.Name} uploaded successfully.");
        }
    }
    
}