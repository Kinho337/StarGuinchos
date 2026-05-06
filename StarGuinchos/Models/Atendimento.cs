using System.ComponentModel.DataAnnotations;

namespace StarGuinchos.Models
{
    public class Atendimento
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O título do atendimento é obrigatório.")]
        [StringLength(120)]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A categoria é obrigatória.")]
        [StringLength(80)]
        public string Categoria { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descrição é obrigatória.")]
        public string Descricao { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string ImagemUrl { get; set; } = string.Empty;

        public int PosicaoX { get; set; } = 50;

        public int PosicaoY { get; set; } = 50;

        public decimal Zoom { get; set; } = 1.00m;

        public DateTime DataCadastro { get; set; } = DateTime.Now;

        public bool Ativo { get; set; } = true;
    }
}