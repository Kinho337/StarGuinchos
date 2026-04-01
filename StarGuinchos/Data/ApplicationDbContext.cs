using Microsoft.EntityFrameworkCore;
using StarGuinchos.Models;

namespace StarGuinchos.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Solicitacao> Solicitacoes { get; set; }
        public DbSet<Avaliacao> Avaliacoes { get; set; }
    }
}

        

        
    