using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BoaSorte.Infrastructure.Persistence;

/// <summary>Lets `dotnet ef migrations add` build the model without a running database or app configuration.</summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=boasorte_design_time")
            .UseSnakeCaseNamingConvention()
            .Options);
}
