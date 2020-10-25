using SimuladorGerenciaMemoria.Models;
using Microsoft.EntityFrameworkCore;

namespace SimuladorGerenciaMemoria.Models
{
    public class SimuladorContext : DbContext
    {
        public SimuladorContext(DbContextOptions<SimuladorContext> options) : base(options)
        {
        }

        public DbSet<Process> Processes { get; set; }

        public DbSet<Memory> Memories { get; set; }

        public DbSet<Simulation> Simulations { get; set; }

        public DbSet<Frame> Frames { get; set; }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Process>().ToTable("Processes");
            modelBuilder.Entity<Memory>().ToTable("Memories");
            modelBuilder.Entity<Simulation>().ToTable("Simulations");
            modelBuilder.Entity<Frame>().ToTable("Frames");
            modelBuilder.Entity<User>().ToTable("Users");
        }
    }
}