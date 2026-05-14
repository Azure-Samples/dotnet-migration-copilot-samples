using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ContosoUniversity.Data
{
    public static class SchoolContextFactory
    {
        public static SchoolContext Create(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var optionsBuilder = new DbContextOptionsBuilder<SchoolContext>();
            optionsBuilder.UseSqlServer(connectionString);
            
            return new SchoolContext(optionsBuilder.Options);
        }
    }
}

