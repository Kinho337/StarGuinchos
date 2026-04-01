using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarGuinchos.Data;
using StarGuinchos.Models.ViewModels;

namespace StarGuinchos.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeIndexViewModel
            {
                Avaliacoes = await _context.Avaliacoes
                    .Where(a => a.Status == "aprovada" && a.AutorizadoPublicacao)
                    .OrderByDescending(a => a.DataCriacao)
                    .Take(10)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        public IActionResult Servicos()
        {
            return View();
        }

        public IActionResult AreaDeAtendimento()
        {
            return View();
        }

        public IActionResult Atendimentos()
        {
            return View();
        }

        public IActionResult Sobre()
        {
            return View();
        }

        public IActionResult Contato()
        {
            return View();
        }
    }
}