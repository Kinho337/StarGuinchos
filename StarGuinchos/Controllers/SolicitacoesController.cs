using Microsoft.AspNetCore.Mvc;
using StarGuinchos.Data;
using StarGuinchos.Models;
using StarGuinchos.Services;

namespace StarGuinchos.Controllers
{
    public class SolicitacoesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ITelegramNotifier _telegramNotifier;

        public SolicitacoesController(
            ApplicationDbContext context,
            ITelegramNotifier telegramNotifier)
        {
            _context = context;
            _telegramNotifier = telegramNotifier;
        }

        [HttpGet("solicitar")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("solicitar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Solicitacao solicitacao)
        {
            try
            {
                solicitacao.Data_solicitacao = DateTime.Now;
                solicitacao.Status_solicitacao = "novo";

                _context.Solicitacoes.Add(solicitacao);
                await _context.SaveChangesAsync();

                await _telegramNotifier.SendNewServiceAsync(solicitacao);

                return RedirectToAction(nameof(Success));
            }
            catch (Exception ex)
            {
                var erroInterno = ex.InnerException?.Message;
                return Content("Erro ao salvar: " + ex.Message + " | Inner: " + erroInterno);
            }
        }

        [HttpGet("solicitacao/enviada")]
        public IActionResult Success()
        {
            return View();
        }
    }
}