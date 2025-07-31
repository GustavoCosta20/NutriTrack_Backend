using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace NutriTrack_Connection
{
    public class Context : DbContext
    {
        private IConfiguration _config;

        public Context(IConfiguration configuration, DbContextOptions options ) : base(options)
        {
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var typeDatabase = _config["TypeDatabase"];
            var connectionString = _config.GetConnectionString(typeDatabase);

            optionsBuilder.UseNpgsql(connectionString);
        }
    }
}
