using Microsoft.EntityFrameworkCore;
using WebApplicationASP01.Models;

namespace WebApplicationASP01.App;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Note> Notes => Set<Note>();
    public DbSet<LinkEntry> SharedLinks => Set<LinkEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Note>(entity =>
        {
            entity.ToTable("notes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.Content).HasMaxLength(5000);
        });

        modelBuilder.Entity<LinkEntry>(entity =>
        {
            entity.ToTable("shared_links");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(4000);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.CreatedAt).IsDescending(); // pro rychlé řazení od nejnovějšího
        });
    }
}
