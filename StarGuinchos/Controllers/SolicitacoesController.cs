using Microsoft.AspNetCore.Mvc;
using StarGuinchos.Data;
using StarGuinchos.Models;
using StarGuinchos.Services;

namespace StarGuinchos.Controllers
{
    public class SolicitacoesController : Controller
    {
        private const string WhatsAppNumero = "5511946999701";

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
            return View(new Solicitacao());
        }

        [HttpPost("solicitar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Solicitacao solicitacao)
        {
            try
            {
                solicitacao.Nome = string.IsNullOrWhiteSpace(solicitacao.Nome)
                    ? "Cliente não informado"
                    : solicitacao.Nome;

                solicitacao.Telefone = string.IsNullOrWhiteSpace(solicitacao.Telefone)
                    ? "Não informado"
                    : solicitacao.Telefone;

                solicitacao.Veiculo = string.IsNullOrWhiteSpace(solicitacao.Veiculo)
                    ? "Não informado"
                    : solicitacao.Veiculo;

                solicitacao.Problema = string.IsNullOrWhiteSpace(solicitacao.Problema)
                    ? "Não informado"
                    : solicitacao.Problema;

                solicitacao.Ponto_partida = string.IsNullOrWhiteSpace(solicitacao.Ponto_partida)
                    ? "Não informado"
                    : solicitacao.Ponto_partida;

                solicitacao.Destino = string.IsNullOrWhiteSpace(solicitacao.Destino)
                    ? "Não informado"
                    : solicitacao.Destino;

                solicitacao.Data_solicitacao = DateTime.Now;
                solicitacao.Status_solicitacao = "novo";

                _context.Solicitacoes.Add(solicitacao);
                await _context.SaveChangesAsync();

                try
                {
                    await _telegramNotifier.SendNewServiceAsync(solicitacao);
                }
                catch
                {
                    // Se o Telegram falhar, a solicitação continua salva
                    // e o cliente segue normalmente para o WhatsApp.
                }

                var mensagemWhatsApp = CriarMensagemWhatsApp(solicitacao);
                var urlWhatsApp = CriarUrlWhatsApp(mensagemWhatsApp);

                return Redirect(urlWhatsApp);
            }
            catch (Exception ex)
            {
                var erroInterno = ex.InnerException?.Message;
                return Content("Erro ao salvar: " + ex.Message + " | Inner: " + erroInterno);
            }
        }

        [HttpPost("solicitar/rapido")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rapido()
        {
            try
            {
                var solicitacao = new Solicitacao
                {
                    Nome = "Solicitação rápida via WhatsApp",
                    Telefone = "Não informado",
                    Veiculo = "Não informado",
                    Problema = "Solicitação rápida",
                    Ponto_partida = "Cliente pulou o formulário",
                    Destino = "A confirmar pelo WhatsApp",
                    Data_solicitacao = DateTime.Now,
                    Status_solicitacao = "novo"
                };

                _context.Solicitacoes.Add(solicitacao);
                await _context.SaveChangesAsync();

                try
                {
                    await _telegramNotifier.SendNewServiceAsync(solicitacao);
                }
                catch
                {
                    // Se o Telegram falhar, a solicitação rápida continua salva
                    // e o cliente segue normalmente para o WhatsApp.
                }

                var mensagemWhatsApp = "Preciso de um guincho, pode me ajudar?";
                var urlWhatsApp = CriarUrlWhatsApp(mensagemWhatsApp);

                return Redirect(urlWhatsApp);
            }
            catch (Exception ex)
            {
                var erroInterno = ex.InnerException?.Message;
                return Content("Erro ao registrar solicitação rápida: " + ex.Message + " | Inner: " + erroInterno);
            }
        }

        [HttpGet("solicitacao/enviada")]
        public IActionResult Success()
        {
            return View();
        }

        private static string CriarUrlWhatsApp(string mensagem)
        {
            var mensagemCodificada = Uri.EscapeDataString(mensagem);
            return $"https://wa.me/{WhatsAppNumero}?text={mensagemCodificada}";
        }

        private static string CriarMensagemWhatsApp(Solicitacao solicitacao)
        {
            return
$@"Olá, preciso de um guincho.

Dados da solicitação:

Nome: {solicitacao.Nome}
Telefone: {solicitacao.Telefone}
Veículo: {solicitacao.Veiculo}
Problema: {solicitacao.Problema}
Ponto de partida: {solicitacao.Ponto_partida}
Destino: {solicitacao.Destino}

Pode me ajudar?";
        }
    }
}