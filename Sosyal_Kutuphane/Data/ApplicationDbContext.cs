using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sosyal_Kutuphane.Models;

namespace Sosyal_Kutuphane.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    
    public DbSet<Review> Reviews { get; set; }
    
    public DbSet<Rating> Ratings { get; set; }
    
    public DbSet<Like> Likes { get; set; }
    
    public DbSet<Follow> Follow { get; set; }
    
    public DbSet<UserMedia> UserMedia { get; set; }
    
    public DbSet<CustomList> CustomList { get; set; }
    
    public DbSet<CustomListItem> CustomListItem { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Follow>()
            .HasOne(f => f.Follower)
            .WithMany(u => u.Following)
            .HasForeignKey(f => f.FollowerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Follow>()
            .HasOne(f => f.Following)
            .WithMany(u => u.Followers)
            .HasForeignKey(f => f.FollowingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}