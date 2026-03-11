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
                $"Data: {solicitacao.Data_solicitacao:dd/MM/yyyy HH:mm}";

            string mensagemCodificada = WebUtility.UrlEncode(mensagem);

            string url = $"https://wa.me/?text={mensagemCodificada}";

            return Redirect(url);
        }
    }
}