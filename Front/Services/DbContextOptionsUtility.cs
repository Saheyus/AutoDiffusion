// DbContextOptionsUtility.cs
using AutoDiffusion.Data;
using Microsoft.EntityFrameworkCore;

public static class DbContextOptionsUtility
{
    public static DbContextOptions<AppDbContext> GetDbContextOptions()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

        var config = builder.Build();
        var connectionString = config.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return optionsBuilder.Options;
    }
}

