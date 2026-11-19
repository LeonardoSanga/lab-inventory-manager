using Microsoft.EntityFrameworkCore;

using LabRobotica.Models;

namespace LabRobotica.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Item>? Itens { get; set; }
        public DbSet<Estoque>? Estoques { get; set; }
        public DbSet<Movimentacao>? Movimentacoes { get; set; }
        public DbSet<Emprestimo>? Emprestimos { get; set; }
        public DbSet<Usuario>? Usuarios { get; set; }
        public DbSet<LogAuditoria>? LogsAuditoria { get; set; }
    }
}
