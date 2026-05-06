using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarGuinchos.Data;
using StarGuinchos.Models;

namespace StarGuinchos.Controllers
{
    public class AvaliacoesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AvaliacoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var avaliacoes = _context.Avaliacoes
                .Where(a => a.Status == "aprovada" && a.AutorizadoPublicacao)
                .OrderByDescending(a => a.DataCriacao)
                .ToList();

            return View(avaliacoes);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Avaliacao avaliacao)
        {
            if (!ModelState.IsValid)
            {
                return View(avaliacao);
            }

            avaliacao.Status = "pendente";
            avaliacao.DataCriacao = DateTime.Now;

            _context.Avaliacoes.Add(avaliacao);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Success));
        }

        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }
    }
}