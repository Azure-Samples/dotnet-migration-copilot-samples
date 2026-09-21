using Microsoft.EntityFrameworkCore;
using System;
using System.Configuration;
using System.IO;

namespace ContosoUniversity.Data
{
    public static class SchoolContextFactory
    {
        public static SchoolContext Create()
        {
            var connectionString = GetConnectionString();
            var optionsBuilder = new DbContextOptionsBuilder<SchoolContext>();
            optionsBuilder.UseSqlServer(connectionString);
            
            return new SchoolContext(optionsBuilder.Options);
        }

        // The build copies Web.config to "legacy.config" next to the app so ConfigurationManager
        // (from the System.Configuration.ConfigurationManager package) can resolve it explicitly,
        // instead of relying on the .exe/.dll.config naming convention that varies by launch method.
        private static string GetConnectionString()
        {
            var configMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = Path.Combine(AppContext.BaseDirectory, "legacy.config")
            };
            var config = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            return config.ConnectionStrings.ConnectionStrings["DefaultConnection"].ConnectionString;
        }
    }
}
