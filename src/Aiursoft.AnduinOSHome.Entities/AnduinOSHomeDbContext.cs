using System.Diagnostics.CodeAnalysis;
using Aiursoft.DbTools;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.AnduinOSHome.Entities;

[ExcludeFromCodeCoverage]

public abstract class AnduinOSHomeDbContext(DbContextOptions options) : IdentityDbContext<User>(options), ICanMigrate
{
    public DbSet<GlobalSetting> GlobalSettings => Set<GlobalSetting>();
    public DbSet<Hardware> Hardware => Set<Hardware>();
    public DbSet<HardwareTranslation> HardwareTranslations => Set<HardwareTranslation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Hardware>().HasIndex(x => x.Slug).IsUnique();
        builder.Entity<HardwareTranslation>().HasIndex(x => new { x.HardwareId, x.Culture }).IsUnique();
        builder.Entity<Hardware>().HasMany(x => x.Translations).WithOne()
            .HasForeignKey(x => x.HardwareId).OnDelete(DeleteBehavior.Cascade);
    }

    public virtual  Task MigrateAsync(CancellationToken cancellationToken) =>
        Database.MigrateAsync(cancellationToken);

    public virtual  Task<bool> CanConnectAsync() =>
        Database.CanConnectAsync();
}
