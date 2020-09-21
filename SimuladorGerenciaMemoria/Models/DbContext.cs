using SimuladorGerenciaMemoria.Models;
using Microsoft.EntityFrameworkCore;

namespace SimuladorGerenciaMemoria.Data
{
    public class SimuladorContext : DbContext
    {
        public SimuladorContext(DbContextOptions<SimuladorContext> options) : base(options)
        {
        }

        public DbSet<Course> Courses { get; set; }
    }
}