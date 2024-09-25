using QuickUnity.Entities;
using QuickUnity.Entities.Enums;

namespace QuickUnity.Services;

public class MediaStorageService
{
    private readonly string _uploadPath;
    
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
                await SaveVideoAsync(mediaSaveRequest);
                break;
            default: throw new Exception($"Unknown media type: {mediaSaveRequest.mediaType}");
                
        }
    }

    private async Task SaveVideoAsync(MediaSaveRequest mediaSaveRequest)
    {
        var userStoragePath = Path.Combine(_uploadPath, mediaSaveRequest.OwnerId.ToString(), "video");
        var filePath = Path.Combine(userStoragePath, $"{mediaSaveRequest.MediaId}.mp4");

        await File.WriteAllBytesAsync(filePath, mediaSaveRequest.fileContent);
    }

    private async Task<KeyValuePair<string, Action<bool>>>  PostponedSaveImageWithPreviewAsync(MediaSaveRequest mediaSaveRequest)
    {
        //check if image in good format
        //change format to png
        //resize to 400x400
        var userStoragePath = Path.Combine(_uploadPath, mediaSaveRequest.OwnerId.ToString(), "image");
        //check if in path exist file _tmp_avatar.png if yes, then delete
        var filePath = Path.Combine(userStoragePath, "_tmp_avatar.png");

        await File.WriteAllBytesAsync(filePath, mediaSaveRequest.fileContent);
    }
    
}