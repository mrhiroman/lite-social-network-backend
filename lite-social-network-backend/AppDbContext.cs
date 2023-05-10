using lite_social_network_backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace lite_social_network_backend;

public class AppDbContext: DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Friend> Friends { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }
        
        modelBuilder.Entity<Friend>()
            .HasOne(f => f.User)
            .WithMany(u => u.Friends)
            .HasForeignKey(f => f.UserId);
        modelBuilder.Entity<Friend>()
            .HasOne(f=> f.FriendUser)
            .WithMany(u => u.FriendedBy)
            .HasForeignKey(f=> f.FriendUserId);
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
}