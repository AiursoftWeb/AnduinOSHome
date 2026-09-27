using Aiursoft.AnduinOSHome.Entities;
using Aiursoft.AnduinOSHome.MySql;
using Aiursoft.AnduinOSHome.MySql.Migrations;
using Aiursoft.AnduinOSHome.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Aiursoft.AnduinOSHome.Tests.IntegrationTests;

/// <summary>
/// This test class ensures that the Entity Framework migrations are up-to-date for all supported database providers.
/// If you change the database model (entities), you must create a new migration for both SQLite and MySQL.
/// </summary>
[TestClass]
public class MigrationTests
{
    [TestMethod]
    public void TestSqliteMigrations()
    {
        var options = new DbContextOptionsBuilder<SqliteContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        using var context = new SqliteContext(options);
        var hasPendingChanges = context.Database.HasPendingModelChanges();
        Assert.IsFalse(hasPendingChanges, "There are pending model changes for Sqlite. Please run 'dotnet ef migrations add' for Sqlite.");
    }

    [TestMethod]
    public void TestMySqlMigrations()
    {
        var options = new DbContextOptionsBuilder<MySqlContext>()
            .UseMySql("Server=localhost;Database=test;Uid=root;Pwd=password;", new MySqlServerVersion(new Version(8, 0, 31)))
            .Options;
        using var context = new MySqlContext(options);
        var hasPendingChanges = context.Database.HasPendingModelChanges();
        Assert.IsFalse(hasPendingChanges, "There are pending model changes for MySql. Please run 'dotnet ef migrations add' for MySql.");
    }

    [TestMethod]
    public void TestMySqlHardwareDetailsUseOffRowTextColumns()
    {
        var options = new DbContextOptionsBuilder<MySqlContext>()
            .UseMySql("Server=localhost;Database=test;Uid=root;Pwd=password;", new MySqlServerVersion(new Version(9, 5, 0)))
            .Options;
        using var context = new MySqlContext(options);
        var translation = context.Model.FindEntityType(typeof(HardwareTranslation));
        Assert.IsNotNull(translation);

        foreach (var propertyName in new[]
                 {
                     nameof(HardwareTranslation.DisplayDetail),
                     nameof(HardwareTranslation.GraphicsDetail),
                     nameof(HardwareTranslation.InstallationDetail),
                     nameof(HardwareTranslation.PerformanceDetail),
                     nameof(HardwareTranslation.SecureBootDetail),
                     nameof(HardwareTranslation.VirtualizationDetail),
                     nameof(HardwareTranslation.WifiDetail)
                 })
        {
            Assert.AreEqual("text", translation.FindProperty(propertyName)?.GetColumnType(), propertyName);
        }
    }

    [TestMethod]
    public void TestMySqlHardwareDetailMigrationUsesOffRowTextColumns()
    {
        var addedColumns = new AddHardwareCapabilityDetails().UpOperations
            .OfType<AddColumnOperation>()
            .ToList();

        Assert.AreEqual(7, addedColumns.Count);
        foreach (var column in addedColumns)
        {
            Assert.AreEqual("HardwareTranslations", column.Table);
            Assert.AreEqual("text", column.ColumnType, column.Name);
        }
    }
}
