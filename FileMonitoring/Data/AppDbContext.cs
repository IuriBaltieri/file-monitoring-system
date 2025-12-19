using Microsoft.EntityFrameworkCore;
using FileMonitoring.Models;

namespace FileMonitoring.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        
        public DbSet<Arquivo> Arquivos { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Arquivo>(entity =>
            {
                entity.HasIndex(e => e.Empresa);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.DataProcessamento);
            });
        }
    }
}