using StarGuinchos.Models;

namespace StarGuinchos.Services
{
    public interface ITelegramNotifier
    {
        Task SendNewServiceAsync(Solicitacao solicitacao);
    }
}