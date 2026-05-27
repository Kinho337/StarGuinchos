using System.ComponentModel.DataAnnotations;

namespace StarGuinchos.ViewModels
{
    public class AdminLoginViewModel
    {
        [Required(ErrorMessage = "Informe o usuário.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a senha.")]
        public string Password { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}