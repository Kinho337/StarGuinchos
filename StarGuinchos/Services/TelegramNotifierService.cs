using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Options;
using StarGuinchos.Configuracoes;
using StarGuinchos.Models;

namespace StarGuinchos.Services
{
    public class TelegramNotifierService : ITelegramNotifier
    {
        private readonly HttpClient _httpClient;
        private readonly TelegramBotOptions _options;

        public TelegramNotifierService(
            HttpClient httpClient,
            IOptions<TelegramBotOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task SendNewServiceAsync(Solicitacao solicitacao)
        {
            var mensagem = MontarMensagem(solicitacao);

            var url = $"https://api.telegram.org/bot{_options.BotToken}/sendMessage";

            var payload = new
            {
                chat_id = _options.ChatId,
                text = mensagem
            };

            var response = await _httpClient.PostAsJsonAsync(url, payload);

            response.EnsureSuccessStatusCode();
        }

        private static string MontarMensagem(Solicitacao solicitacao)
        {
            var nome = string.IsNullOrWhiteSpace(solicitacao.Nome)
                ? "Não informado"
                : solicitacao.Nome;

            var telefone = string.IsNullOrWhiteSpace(solicitacao.Telefone)
                ? "Não informado"
                : solicitacao.Telefone;

            var veiculo = string.IsNullOrWhiteSpace(solicitacao.Veiculo)
                ? "Não informado"
                : solicitacao.Veiculo;

            var problema = string.IsNullOrWhiteSpace(solicitacao.Problema)
                ? "Não informado"
                : solicitacao.Problema;

            var origem = string.IsNullOrWhiteSpace(solicitacao.Ponto_partida)
                ? "Não informado"
                : solicitacao.Ponto_partida;

            var destino = string.IsNullOrWhiteSpace(solicitacao.Destino)
                ? "Não informado"
                : solicitacao.Destino;

            var data = solicitacao.Data_solicitacao.ToString("dd/MM/yyyy HH:mm");

            var status = string.IsNullOrWhiteSpace(solicitacao.Status_solicitacao)
                ? "Não informado"
                : solicitacao.Status_solicitacao;

            var sb = new StringBuilder();

            sb.AppendLine("🚨 NOVA SOLICITAÇÃO - StarGuinchos");
            sb.AppendLine();
            sb.AppendLine($"Protocolo: #{solicitacao.Id}");
            sb.AppendLine($"Cliente: {nome}");
            sb.AppendLine($"Telefone: {telefone}");
            sb.AppendLine($"Veículo: {veiculo}");
            sb.AppendLine($"Problema: {problema}");
            sb.AppendLine($"Origem: {origem}");
            sb.AppendLine($"Destino: {destino}");
            sb.AppendLine($"Data: {data}");
            sb.AppendLine($"Status: {status}");

            return sb.ToString();
        }
    }
}