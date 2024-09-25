using QuickUnity.Entities.Enums;

namespace QuickUnity.Entities;

public class MediaSaveRequest
{
    public Guid Id { get; set; }
    public MultimediaType mediaType { get; set; }
    public byte[] fileContent { get; set; }
    public Guid OwnerId { get; set; }
}