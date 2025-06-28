using Microsoft.EntityFrameworkCore;
using DevInsightCli.Services; // Required for the Workflow class

namespace DevInsightCli.Data
{
    public class DevInsightDbContext : DbContext
    {
        public DbSet<Workflow> Workflows { get; set; }

        public DevInsightDbContext(DbContextOptions<DevInsightDbContext> options)
            : base(options)
        {
        }

        // If you're not using AddDbContext in Program.cs or Startup.cs with DI,
        // you might need an OnConfiguring method to set up the SQLite provider.
        // However, it's generally better to configure this via DI.
        // For now, we'll assume options are passed in.

        // Example for local SQLite configuration if not using DI for options:
        // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        // {
        //     if (!optionsBuilder.IsConfigured)
        //     {
        //         optionsBuilder.UseSqlite("Data Source=devinsight.db");
        //     }
        // }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure your entities here if needed
            modelBuilder.Entity<Workflow>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                // Add other configurations as necessary
            });
        }
    }
}
