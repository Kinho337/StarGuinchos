using System.Collections.Generic;
using StarGuinchos.Models;

namespace StarGuinchos.Models.ViewModels
{
    public class HomeIndexViewModel
    {
        public List<Avaliacao> Avaliacoes { get; set; } = new();
    }
}