using Microsoft.EntityFrameworkCore;

namespace WebApplicationASP01.App;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Person> Persons => Set<Person>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Person>(entity =>
        {
            entity.ToTable("persons");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Jmeno).HasColumnName("jmeno").IsRequired().HasMaxLength(150);
            entity.Property(e => e.DatumNarozeni).HasColumnName("datum_narozeni").IsRequired();
            entity.Property(e => e.TrvalaAdresa).HasColumnName("trvala_adresa").IsRequired().HasMaxLength(250);
            entity.Property(e => e.RodneCislo).HasColumnName("rodne_cislo").IsRequired().HasMaxLength(20);
            entity.Property(e => e.Telefon).HasColumnName("telefon").IsRequired().HasMaxLength(30);
            entity.Property(e => e.Email).HasColumnName("email").IsRequired().HasMaxLength(150);
        });
    }
}
