namespace StarGuinchos.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalSolicitacoes { get; set; }
        public int SolicitacoesHoje { get; set; }
        public int NovasSolicitacoes { get; set; }
        public int EmAndamento { get; set; }
        public int Concluidas { get; set; }
        public int Canceladas { get; set; }
    }
}