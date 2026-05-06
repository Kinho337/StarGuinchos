using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarGuinchos.Data;
using StarGuinchos.Models;
using StarGuinchos.ViewModels;
using System.Net;
using System.IO;

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
            var solicitacoes = _context.Solicitacoes
                .OrderByDescending(s => s.Data_solicitacao)
                .ToList();

            var hoje = DateTime.Today;
            var amanha = hoje.AddDays(1);

            ViewBag.TotalSolicitacoes = solicitacoes.Count;
            ViewBag.NovasSolicitacoes = solicitacoes.Count(s => s.Status_solicitacao != null && s.Status_solicitacao.ToLower() == "novo");
            ViewBag.EmAndamento = solicitacoes.Count(s => s.Status_solicitacao != null && s.Status_solicitacao.ToLower() == "em andamento");
            ViewBag.Concluidas = solicitacoes.Count(s => s.Status_solicitacao != null && s.Status_solicitacao.ToLower() == "concluido");
            ViewBag.Canceladas = solicitacoes.Count(s => s.Status_solicitacao != null && s.Status_solicitacao.ToLower() == "cancelado");

            ViewBag.UltimasSolicitacoes = solicitacoes
                .Take(6)
                .ToList();

            ViewBag.SolicitacoesHoje = solicitacoes
                .Where(s => s.Data_solicitacao >= hoje && s.Data_solicitacao < amanha)
                .ToList();

            var avaliacoes = _context.Avaliacoes
                .OrderByDescending(a => a.DataCriacao)
                .ToList();

            ViewBag.TotalAvaliacoes = avaliacoes.Count;
            ViewBag.AvaliacoesPendentes = avaliacoes.Count(a => a.Status != null && a.Status.ToLower() == "pendente");
            ViewBag.AvaliacoesAprovadas = avaliacoes.Count(a => a.Status != null && a.Status.ToLower() == "aprovada");
            ViewBag.AvaliacoesRecusadas = avaliacoes.Count(a => a.Status != null && a.Status.ToLower() == "recusada");

            ViewBag.UltimasAvaliacoes = avaliacoes
                .Take(5)
                .ToList();

            return View();


        }

        [HttpGet("admin/solicitacoes")]
        public IActionResult Solicitacoes(string busca, string status, DateTime? data)
        {
            var query = _context.Solicitacoes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(busca))
            {
                busca = busca.Trim();

                string buscaTelefone = new string(busca.Where(char.IsDigit).ToArray());

                query = query.Where(s =>
                    (s.Nome != null && s.Nome.Contains(busca)) ||
                    (s.Veiculo != null && s.Veiculo.Contains(busca)) ||
                    (
                        s.Telefone != null &&
                        s.Telefone
                            .Replace("(", "")
                            .Replace(")", "")
                            .Replace("-", "")
                            .Replace(" ", "")
                            .Contains(buscaTelefone)
                    )
                );
        }

            if (!string.IsNullOrWhiteSpace(status))
            {
                status = status.Trim().ToLower();
                query = query.Where(s => s.Status_solicitacao != null && s.Status_solicitacao.ToLower() == status);
            }

            if (data.HasValue)
            {
                var diaInicio = data.Value.Date;
                var diaFim = diaInicio.AddDays(1);

                query = query.Where(s => s.Data_solicitacao >= diaInicio && s.Data_solicitacao < diaFim);
            }

            var lista = query
                .OrderByDescending(s => s.Id)
                .ToList();

            ViewBag.Busca = busca;
            ViewBag.Status = status;
            ViewBag.Data = data?.ToString("yyyy-MM-dd");

            return View(lista);
        }

        [HttpGet("admin/solicitacoes/{id}")]
        public IActionResult Detalhes(int id)
        {
            var solicitacao = _context.Solicitacoes.FirstOrDefault(s => s.Id == id);

            if (solicitacao == null)
            {
                return NotFound();
            }

            return View(solicitacao);
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

        [HttpPost("admin/solicitacoes/{id}/excluir")]
        [ValidateAntiForgeryToken]
        public IActionResult Excluir(int id)
        {
            var solicitacao = _context.Solicitacoes.FirstOrDefault(s => s.Id == id);

            if (solicitacao == null)
            {
                return NotFound();
            }

            _context.Solicitacoes.Remove(solicitacao);
            _context.SaveChanges();

            TempData["AdminMensagem"] = $"Solicitação #{id} excluída com sucesso.";
            return RedirectToAction(nameof(Solicitacoes));
        }

        [HttpGet("admin/avaliacoes")]
        public IActionResult Avaliacoes(string filtro = "pendente")
        {
            var query = _context.Avaliacoes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro) && filtro.ToLower() != "todas")
            {
                query = query.Where(a => a.Status != null && a.Status.ToLower() == filtro.ToLower());
            }

            var lista = query
                .OrderByDescending(a => a.DataCriacao)
                .ToList();

            ViewBag.FiltroAtual = filtro;
            return View(lista);
        }

        [HttpPost("admin/avaliacoes/{id}/aprovar")]
        [ValidateAntiForgeryToken]
        public IActionResult AprovarAvaliacao(int id)
        {
            var avaliacao = _context.Avaliacoes.FirstOrDefault(a => a.Id == id);

            if (avaliacao == null)
            {
                return NotFound();
            }

            avaliacao.Status = "aprovada";
            _context.SaveChanges();

            TempData["AdminMensagem"] = $"Avaliação #{id} aprovada com sucesso.";
            return RedirectToAction(nameof(Avaliacoes));
        }

        [HttpPost("admin/avaliacoes/{id}/recusar")]
        [ValidateAntiForgeryToken]
        public IActionResult RecusarAvaliacao(int id)
        {
            var avaliacao = _context.Avaliacoes.FirstOrDefault(a => a.Id == id);

            if (avaliacao == null)
            {
                return NotFound();
            }

            avaliacao.Status = "recusada";
            _context.SaveChanges();

            TempData["AdminMensagem"] = $"Avaliação #{id} recusada com sucesso.";
            return RedirectToAction(nameof(Avaliacoes));
        }

        [HttpPost("admin/avaliacoes/{id}/excluir")]
        [ValidateAntiForgeryToken]
        public IActionResult ExcluirAvaliacao(int id)
        {
            var avaliacao = _context.Avaliacoes.FirstOrDefault(a => a.Id == id);

            if (avaliacao == null)
            {
                return NotFound();
            }

            _context.Avaliacoes.Remove(avaliacao);
            _context.SaveChanges();

            TempData["AdminMensagem"] = $"Avaliação #{id} excluída com sucesso.";
            return RedirectToAction(nameof(Avaliacoes));
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

        // =========================
        // ATENDIMENTOS - FEED ADMIN
        // =========================

        [HttpGet]
        public async Task<IActionResult> Atendimentos()
        {
            var atendimentos = await _context.Atendimentos
                .OrderByDescending(a => a.DataCadastro)
                .ToListAsync();

            return View(atendimentos);
        }

        [HttpGet]
        public IActionResult CriarAtendimento()
        {
            return View(new AtendimentoCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CriarAtendimento(AtendimentoCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Imagem == null || model.Imagem.Length == 0)
            {
                ModelState.AddModelError("Imagem", "Selecione uma imagem válida.");
                return View(model);
            }

            var extensoesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extensao = Path.GetExtension(model.Imagem.FileName).ToLowerInvariant();

            if (!extensoesPermitidas.Contains(extensao))
            {
                ModelState.AddModelError("Imagem", "Envie uma imagem nos formatos JPG, JPEG, PNG ou WEBP.");
                return View(model);
            }

            var pastaUploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "atendimentos");

            if (!Directory.Exists(pastaUploads))
            {
                Directory.CreateDirectory(pastaUploads);
            }

            var nomeArquivo = $"atendimento-{Guid.NewGuid()}{extensao}";
            var caminhoFisico = Path.Combine(pastaUploads, nomeArquivo);

            using (var stream = new FileStream(caminhoFisico, FileMode.Create))
            {
                await model.Imagem.CopyToAsync(stream);
            }

            var atendimento = new Atendimento
            {
                Titulo = model.Titulo,
                Categoria = model.Categoria,
                Descricao = model.Descricao,
                ImagemUrl = $"/uploads/atendimentos/{nomeArquivo}",
                PosicaoX = model.PosicaoX,
                PosicaoY = model.PosicaoY,
                Zoom = model.Zoom,
                DataCadastro = DateTime.Now,
                Ativo = true
            };

            _context.Atendimentos.Add(atendimento);
            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] = "Atendimento cadastrado com sucesso.";

            return RedirectToAction(nameof(Atendimentos));
        }
    }
}