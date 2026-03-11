using System.ComponentModel.DataAnnotations;

namespace StarGuinchos.Models
{
    public class Solicitacao
    {
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string Telefone { get; set; } = string.Empty;
        public string? Veiculo { get; set; }
        public string? Problema { get; set; }
        public string? Ponto_partida { get; set; }
        public string? Destino { get; set; }
        public DateTime Data_solicitacao { get; set; }
        public string? Status_solicitacao { get; set; }
    }
}
