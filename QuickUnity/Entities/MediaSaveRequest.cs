using QuickUnity.Entities.Enums;
using FileInfo = Radzen.FileInfo;

namespace QuickUnity.Entities;

public class MediaSaveRequest
{
    public string MediaId;
    public MultimediaType mediaType { get; set; }
    public FileInfo fileContent { get; set; }
    public string OwnerId { get; set; }
}