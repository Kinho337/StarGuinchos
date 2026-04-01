using System;
using System.ComponentModel.DataAnnotations;

namespace StarGuinchos.Models
{
    public class Avaliacao
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Informe seu nome.")]
        [StringLength(120)]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe seu telefone.")]
        [StringLength(20)]
        public string Telefone { get; set; } = string.Empty;

        [StringLength(120)]
        public string? Cidade { get; set; }

        [Required(ErrorMessage = "Informe uma nota.")]
        [Range(1, 5, ErrorMessage = "A nota deve ser entre 1 e 5.")]
        public int Nota { get; set; }

        [Required(ErrorMessage = "Escreva sua avaliação.")]
        [StringLength(1000)]
        public string Comentario { get; set; } = string.Empty;

        public bool AutorizadoPublicacao { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = "pendente";

        public DateTime DataCriacao { get; set; } = DateTime.Now;
    }
}