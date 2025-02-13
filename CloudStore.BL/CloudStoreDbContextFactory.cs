using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CloudStore.BL
{
    public class CloudStoreDbContextFactory : IDesignTimeDbContextFactory<CloudStoreDbContext>
    {
        public CloudStoreDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

#if DEBUG
            var connectionString = config.GetConnectionString("TestConnection");
#elif RELEASE
            var connectionString = config.GetConnectionString("ProductionConnection");
#endif
            return new CloudStoreDbContext(connectionString);
        }

        private string[] args = [];

        public CloudStoreDbContext CreateDbContext()
        {
            return CreateDbContext(args);
        }

    }
}
