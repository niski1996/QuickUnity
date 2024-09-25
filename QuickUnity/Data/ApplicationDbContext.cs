using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuickUnity.Data.Tables;

namespace QuickUnity.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<ProfileRow>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e=>e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.JoinDate).HasDefaultValueSql("NOW()").ValueGeneratedOnAdd();

        });


        base.OnModelCreating(builder);
    }
}