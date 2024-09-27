namespace QuickUnity.Data.Tables;

public class TrainerRow
{
    public string Id { get; set; }
    public ProfileRow ProfileRow { get; set; } = null!;
    public Guid ProfileRowId { get; set;}
    public ICollection<ProfileRow> Apprentices { get; set; }
}