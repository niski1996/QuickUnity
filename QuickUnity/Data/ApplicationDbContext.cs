using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
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
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ApplicationUserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Club).HasMaxLength(100);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.JoinDate).HasDefaultValueSql("NOW()").ValueGeneratedOnAdd();
            
            entity.HasOne(p => p.TrainerRow)
                .WithOne(t => t.ProfileRow)
                .HasForeignKey<TrainerRow>(t => t.ProfileRowId); 
        });
        builder.Entity<VideoRow>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Id).IsRequired().ValueGeneratedOnAdd();
            entity.Property(e => e.InsertDate).HasDefaultValueSql("NOW()").ValueGeneratedOnAdd();

        });
        builder.Entity<TrainerRow>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProfileRowId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Id).ValueGeneratedOnAdd().HasDefaultValueSql("gen_random_uuid()");
        });

        base.OnModelCreating(builder);
    }
}