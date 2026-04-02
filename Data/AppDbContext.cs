using Microsoft.EntityFrameworkCore;
using VisaFlow.Models;

namespace VisaFlow.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Processo> Processos { get; set; }
        public DbSet<Pagamento> Pagamentos { get; set; }
        public DbSet<Documento> Documentos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Processo -> Cliente
            modelBuilder.Entity<Processo>()
                .HasOne(p => p.Cliente)
                .WithMany(c => c.Processos)
                .HasForeignKey(p => p.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Pagamento -> Processo
            modelBuilder.Entity<Pagamento>()
                .HasOne(p => p.Processo)
                .WithMany(pr => pr.Pagamentos)
                .HasForeignKey(p => p.ProcessoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Documento -> Processo
            modelBuilder.Entity<Documento>()
                .HasOne(d => d.Processo)
                .WithMany(pr => pr.Documentos)
                .HasForeignKey(d => d.ProcessoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Precisão decimal
            modelBuilder.Entity<Processo>()
                .Property(p => p.ValorTotal)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Pagamento>()
                .Property(p => p.Valor)
                .HasColumnType("decimal(18,2)");

            // Seed: admin inicial
            modelBuilder.Entity<Usuario>().HasData(new Usuario
            {
                Id = 1,
                Nome = "Administrador",
                Email = "admin@visaflow.ao",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@1234"),
                Role = "Admin",
                Activo = true,
                DataCriacao = new DateTime(2025, 1, 1)
            });
        }
    }
}
