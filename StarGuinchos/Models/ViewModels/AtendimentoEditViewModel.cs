using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace StarGuinchos.ViewModels
{
    public class AtendimentoEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Informe o título do atendimento.")]
        [StringLength(120)]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(80)]
        public string? Categoria { get; set; }

        [Required(ErrorMessage = "Informe a descrição do serviço.")]
        [StringLength(1000)]
        public string Descricao { get; set; } = string.Empty;

        public IFormFile? Imagem { get; set; }

        public string? ImagemUrlAtual { get; set; }

        [Range(0, 100)]
        public int PosicaoX { get; set; } = 50;

        [Range(0, 100)]
        public int PosicaoY { get; set; } = 50;

        [Range(1, 2.5)]
        public double Zoom { get; set; } = 1;
    }
}