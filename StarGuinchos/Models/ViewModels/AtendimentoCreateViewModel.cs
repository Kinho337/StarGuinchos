using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace StarGuinchos.ViewModels
{
    public class AtendimentoCreateViewModel
    {
        [Required(ErrorMessage = "Informe o título do atendimento.")]
        [StringLength(120)]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a categoria.")]
        [StringLength(80)]
        public string Categoria { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a descrição.")]
        public string Descricao { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecione uma imagem.")]
        public IFormFile? Imagem { get; set; }

        [Range(0, 100)]
        public int PosicaoX { get; set; } = 50;

        [Range(0, 100)]
        public int PosicaoY { get; set; } = 50;

        [Range(1.0, 2.5, ErrorMessage = "O zoom deve ficar entre 1.0 e 2.5.")]
        public decimal Zoom { get; set; } = 1.00m;
    }
}