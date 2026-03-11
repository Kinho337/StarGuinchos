using Microsoft.AspNetCore.Mvc;
using StarGuinchos.Data;
using StarGuinchos.Models;

namespace StarGuinchos.Controllers
{
    public class SolicitacoesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SolicitacoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("solicitar")]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost("solicitar")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Solicitacao solicitacao)
        {
            try
            {
                solicitacao.Data_solicitacao = DateTime.Now;
                solicitacao.Status_solicitacao = "novo";

                _context.Solicitacoes.Add(solicitacao);
                _context.SaveChanges();

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