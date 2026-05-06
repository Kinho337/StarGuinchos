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

            var ultimosAtendimentos = await _context.Atendimentos
        .Where(a => a.Ativo)
        .OrderByDescending(a => a.DataCadastro)
        .Take(6)
        .ToListAsync();

            ViewBag.UltimosAtendimentos = ultimosAtendimentos;

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

        public async Task<IActionResult> Atendimentos()
        {
            var atendimentos = await _context.Atendimentos
                .Where(a => a.Ativo)
                .OrderByDescending(a => a.DataCadastro)
                .ToListAsync();

            return View(atendimentos);
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