using QuickUnity.Components.Components;
using QuickUnity.Data.Tables;

namespace QuickUnity.Entities;

public class VideoCardModel
{
    public VideoCardModel(VideoRow video)
    {
        Id = video.Id;
        Path = video.Path;
        Description = video.Description;
        Name = video.Name;
        SetOriginalState();
    }
    public string Id { get;}
    public string Path { get; }
    public string Description { get; set; }
    public string Name { get; set; }

    private string OriginalDescription { get; set; }
    private string OriginalName { get; set; }

    public void SetOriginalState() //TODO potencjalnie dwa źródła prawdy, co jeżeli baza odrzuci
    {
        OriginalName = Name;
        OriginalDescription = Description;
    }

    public void RestoreOriginalState()
    {
        Name = OriginalName;
        Description = OriginalDescription;
    }

    public bool StateHasChanged() => OriginalName != Name || OriginalDescription != Description;
    
}