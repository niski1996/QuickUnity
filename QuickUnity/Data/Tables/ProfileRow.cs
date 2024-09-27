using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickUnity.Data.Tables;

public class ProfileRow
{
    public Guid Id { get; set; }
    public DateOnly JoinDate { get; set; }
    public string Name { get; set; } = "";
    public string City { get; set; } = "";
    public string Club { get; set; } = "";
    public string LastName { get; set; } = "";

    public TrainerRow? TrainerRow { get; set; }
    public string ApplicationUserId { get; set; } = null!;
    public virtual ApplicationUser ApplicationUser { get; set; } = null!;
    public ICollection<VideoRow> Videos { get; set; } = new List<VideoRow>();
}