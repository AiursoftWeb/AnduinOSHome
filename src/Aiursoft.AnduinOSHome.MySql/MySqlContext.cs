using System.Diagnostics.CodeAnalysis;
using Aiursoft.AnduinOSHome.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.AnduinOSHome.MySql;

[ExcludeFromCodeCoverage]

public class MySqlContext(DbContextOptions<MySqlContext> options) : AnduinOSHomeDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // MySQL counts the maximum byte length of VARCHAR columns toward its row-size limit.
        // Keep the UI's 2000-character validation while storing long descriptions off-row.
        var translations = builder.Entity<HardwareTranslation>();
        translations.Property(x => x.DisplayDetail).HasColumnType("text");
        translations.Property(x => x.GraphicsDetail).HasColumnType("text");
        translations.Property(x => x.InstallationDetail).HasColumnType("text");
        translations.Property(x => x.PerformanceDetail).HasColumnType("text");
        translations.Property(x => x.SecureBootDetail).HasColumnType("text");
        translations.Property(x => x.VirtualizationDetail).HasColumnType("text");
        translations.Property(x => x.WifiDetail).HasColumnType("text");
    }
}
