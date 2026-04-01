using System.Collections.Generic;
using StarGuinchos.Models;

namespace StarGuinchos.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalSolicitacoes { get; set; }
        public int SolicitacoesNovas { get; set; }
        public int AvaliacoesPendentes { get; set; }
        public int AvaliacoesAprovadas { get; set; }

        public List<Solicitacao> UltimasSolicitacoes { get; set; } = new();
        public List<Avaliacao> UltimasAvaliacoesPendentes { get; set; } = new();
    }
}