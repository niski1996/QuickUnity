using QuickUnity.Entities;
using QuickUnity.Entities.Enums;

namespace QuickUnity.Services;

public class MediaStorageService
{
    private readonly string _uploadPath;
    
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

    private void InitializeUserStorage(Guid userId)
    {
        var userStoragePath = Path.Combine(_uploadPath, userId.ToString());
        if(Directory.Exists(userStoragePath)) return;
        Directory.CreateDirectory(userStoragePath);
        var imageStoragePath = Path.Combine(userStoragePath, "image");
        var videoStoragePath = Path.Combine(userStoragePath, "video");
        Directory.CreateDirectory(imageStoragePath);
        Directory.CreateDirectory(videoStoragePath);
    }
    
    
    public async Task SaveFileAsync(MediaSaveRequest mediaSaveRequest)
    {
        InitializeUserStorage(mediaSaveRequest.OwnerId);
        switch (mediaSaveRequest.mediaType)
        {
            case MultimediaType.Image:
                await SaveImageAsync(mediaSaveRequest);
                break;
            case MultimediaType.Video:
                await SaveVideoAsync(mediaSaveRequest);
                break;
            default: throw new Exception($"Unknown media type: {mediaSaveRequest.mediaType}");
                
        }
    }

    private async Task SaveVideoAsync(MediaSaveRequest mediaSaveRequest)
    {
        var userStoragePath = Path.Combine(_uploadPath, mediaSaveRequest.OwnerId.ToString(), "video");
        var filePath = Path.Combine(userStoragePath, $"{mediaSaveRequest.Id}.mp4");

        await File.WriteAllBytesAsync(filePath, mediaSaveRequest.fileContent);
    }

    private async Task SaveImageAsync(MediaSaveRequest mediaSaveRequest)
    {
        var userStoragePath = Path.Combine(_uploadPath, mediaSaveRequest.OwnerId.ToString(), "image");
        var filePath = Path.Combine(userStoragePath, $"{mediaSaveRequest.Id}.png");

        await File.WriteAllBytesAsync(filePath, mediaSaveRequest.fileContent);
    }
    
}