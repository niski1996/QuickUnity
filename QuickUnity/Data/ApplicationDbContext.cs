using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuickUnity.Data.Tables;

namespace QuickUnity.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<ProfileRow>(entity =>
        {
            entity.HasKey(e => e.Id);
            // entity.Property(e => e.ApplicationUser).IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.JoinDate).HasDefaultValueSql("NOW()").ValueGeneratedOnAdd();
            // entity.HasOne(e=>e.ApplicationUser).WithOne(e=>e.Profile).OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(builder);
    }
}