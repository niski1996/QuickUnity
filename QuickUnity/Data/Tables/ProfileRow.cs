using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickUnity.Data.Tables;

public class ProfileRow
{
    public Guid Id { get; set; }

    public DateOnly JoinDate { get; set; }

    public string Name { get; set; } = "";

    public string LastName { get; set; } = "";
    public string ApplicationUserId { get; set; }
    public virtual ApplicationUser ApplicationUser { get; set; } = null!;
}