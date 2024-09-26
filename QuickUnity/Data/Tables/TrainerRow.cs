namespace QuickUnity.Data.Tables;

public class TrainerRow
{
    public string Id { get; set; }
    public ProfileRow TrainerProfile { get; set; }
    public ICollection<ProfileRow> Apprentices { get; set; }
}