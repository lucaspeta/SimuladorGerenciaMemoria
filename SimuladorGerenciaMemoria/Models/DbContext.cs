using SimuladorGerenciaMemoria.Models;
using Microsoft.EntityFrameworkCore;

namespace SimuladorGerenciaMemoria
{
    public class SimuladorContext : DbContext
    {
        public SimuladorContext(DbContextOptions<SimuladorContext> options) : base(options)
        {
        }

        public DbSet<Process> Processes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Process>().ToTable("Process");
        }
    }
}