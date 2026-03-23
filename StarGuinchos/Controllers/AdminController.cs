using Microsoft.AspNetCore.Mvc;
using StarGuinchos.Data;
using System.Net;

namespace StarGuinchos.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("admin")]
        public IActionResult Index()
        {
            var solicitacoes = _context.Solicitacoes.ToList();

            ViewBag.TotalSolicitacoes = solicitacoes.Count;
            ViewBag.NovasSolicitacoes = solicitacoes.Count(s => s.Status_solicitacao != null && s.Status_solicitacao.ToLower() == "novo");
            ViewBag.EmAndamento = solicitacoes.Count(s => s.Status_solicitacao != null && s.Status_solicitacao.ToLower() == "em andamento");
            ViewBag.Concluidas = solicitacoes.Count(s => s.Status_solicitacao != null && s.Status_solicitacao.ToLower() == "concluido");
            ViewBag.Canceladas = solicitacoes.Count(s => s.Status_solicitacao != null && s.Status_solicitacao.ToLower() == "cancelado");

            return View();
        }

        [HttpGet("admin/solicitacoes")]
        public IActionResult Solicitacoes()
        {
            var lista = _context.Solicitacoes
                .OrderByDescending(s => s.Id)
                .ToList();

            return View(lista);
        }

        [HttpPost("admin/solicitacoes/{id}/status")]
        [ValidateAntiForgeryToken]
        public IActionResult AtualizarStatus(int id, string novoStatus)
        {
            var solicitacao = _context.Solicitacoes.FirstOrDefault(s => s.Id == id);

            if (solicitacao == null)
            {
                return NotFound();
            }

            var statusPermitidos = new[] { "novo", "em andamento", "concluido", "cancelado" };

            if (string.IsNullOrWhiteSpace(novoStatus) || !statusPermitidos.Contains(novoStatus.ToLower()))
            {
                TempData["AdminMensagem"] = "Status inválido.";
                return RedirectToAction(nameof(Solicitacoes));
            }

            solicitacao.Status_solicitacao = novoStatus.ToLower();
            _context.SaveChanges();

            TempData["AdminMensagem"] = $"Status da solicitação #{solicitacao.Id} atualizado para '{solicitacao.Status_solicitacao}'.";
            return RedirectToAction(nameof(Solicitacoes));
        }

        [HttpGet("admin/solicitacoes/{id}/whatsapp")]
        public IActionResult EnviarWhatsapp(int id)
        {
            var solicitacao = _context.Solicitacoes.FirstOrDefault(s => s.Id == id);

            if (solicitacao == null)
            {
                return NotFound();
            }

            string mensagem =
                $"🚨 NOVA SOLICITAÇÃO DE GUINCHO\n\n" +
                $"Cliente: {solicitacao.Nome}\n" +
                $"Telefone: {solicitacao.Telefone}\n" +
                $"Veículo: {solicitacao.Veiculo}\n" +
                $"Problema: {solicitacao.Problema}\n" +
                $"Origem: {solicitacao.Ponto_partida}\n" +
                $"Destino: {solicitacao.Destino}\n" +
                $"Data: {solicitacao.Data_solicitacao:dd/MM/yyyy HH:mm}\n" +
                $"Status: {solicitacao.Status_solicitacao}";

            string mensagemCodificada = WebUtility.UrlEncode(mensagem);
            string url = $"https://wa.me/?text={mensagemCodificada}";

            return Redirect(url);
        }
    }
}