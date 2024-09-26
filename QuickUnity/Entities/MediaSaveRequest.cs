using QuickUnity.Entities.Enums;
using FileInfo = Radzen.FileInfo;

namespace QuickUnity.Entities;

public class MediaSaveRequest
{
    public string MediaId;
    public MultimediaType MediaType { get; set; }
    public FileInfo FileContent { get; set; }
    public string OwnerId { get; set; }
}