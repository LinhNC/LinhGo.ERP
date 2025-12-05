using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LinhGo.ERP.Infrastructure.Data;

public class ErpDbContextDesignFactory: IDesignTimeDbContextFactory<ErpDbContext>
{
    public ErpDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ErpDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=linhgo_erp;Username=postgres;Password=1qaz@WSXD",
            b => b.MigrationsAssembly(typeof(ErpDbContext).Assembly.GetName().Name)
        );

        return new ErpDbContext(optionsBuilder.Options);
    }
}