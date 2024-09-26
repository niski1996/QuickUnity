namespace QuickUnity.Data.Tables;

public class VideoRow
{
    
    public string Id { get; set; } 
    public DateOnly InsertDate { get; set; } 
    public string Description { get; set; } 
    public string Name { get; set; } 
    public string Path { get; set; } 
    // public string ProfileRowId { get; set; }
    public ProfileRow ProfileRow { get; set; } = null!;
}