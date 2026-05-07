using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarGuinchos.Data;
using StarGuinchos.Models;
using StarGuinchos.ViewModels;
using System.IO;
using System.Net;

namespace StarGuinchos.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
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
        public IActionResult Solicitacoes(string? busca, string? status, DateTime? data)
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

                query = query.Where(s =>
                    s.Status_solicitacao != null &&
                    s.Status_solicitacao.ToLower() == status
                );
            }

            if (data.HasValue)
            {
                var diaInicio = data.Value.Date;
                var diaFim = diaInicio.AddDays(1);

                query = query.Where(s =>
                    s.Data_solicitacao >= diaInicio &&
                    s.Data_solicitacao < diaFim
                );
            }

            var lista = query
                .OrderByDescending(s => s.Id)
                .ToList();

            ViewBag.Busca = busca;
            ViewBag.Status = status;
            ViewBag.Data = data?.ToString("yyyy-MM-dd");

            return View(lista);
        }

        [HttpGet("admin/solicitacoes/{id:int}")]
        public IActionResult Detalhes(int id)
        {
            var solicitacao = _context.Solicitacoes.FirstOrDefault(s => s.Id == id);

            if (solicitacao == null)
            {
                return NotFound();
            }

            return View(solicitacao);
        }

        [HttpPost("admin/solicitacoes/{id:int}/status")]
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

        [HttpPost("admin/solicitacoes/{id:int}/excluir")]
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
                query = query.Where(a =>
                    a.Status != null &&
                    a.Status.ToLower() == filtro.ToLower()
                );
            }

            var lista = query
                .OrderByDescending(a => a.DataCriacao)
                .ToList();

            ViewBag.FiltroAtual = filtro;

            return View(lista);
        }

        [HttpPost("admin/avaliacoes/{id:int}/aprovar")]
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

        [HttpPost("admin/avaliacoes/{id:int}/recusar")]
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

        [HttpPost("admin/avaliacoes/{id:int}/excluir")]
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

        [HttpGet("admin/solicitacoes/{id:int}/whatsapp")]
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

        [HttpGet("admin/atendimentos")]
        public async Task<IActionResult> Atendimentos()
        {
            var atendimentos = await _context.Atendimentos
                .OrderByDescending(a => a.DataCadastro)
                .ToListAsync();

            return View(atendimentos);
        }

        [HttpGet("admin/atendimentos/novo")]
        public IActionResult CriarAtendimento()
        {
            return View(new AtendimentoCreateViewModel());
        }

        [HttpPost("admin/atendimentos/novo")]
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

            var pastaUploads = Path.Combine(_environment.WebRootPath, "uploads", "atendimentos");

            if (!Directory.Exists(pastaUploads))
            {
                Directory.CreateDirectory(pastaUploads);
            }

            var nomeArquivo = $"atendimento-{Guid.NewGuid()}{extensao}";
            var caminhoFisico = Path.Combine(pastaUploads, nomeArquivo);

            await using (var stream = new FileStream(caminhoFisico, FileMode.Create))
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
            TempData["AdminMensagem"] = "Atendimento cadastrado com sucesso.";

            return RedirectToAction(nameof(Atendimentos));
        }

        [HttpGet("admin/atendimentos/{id:int}/editar")]
        public async Task<IActionResult> EditarAtendimento(int id)
        {
            var atendimento = await _context.Atendimentos.FindAsync(id);

            if (atendimento == null)
            {
                return NotFound();
            }

            var model = new AtendimentoEditViewModel
            {
                Id = atendimento.Id,
                Titulo = atendimento.Titulo,
                Categoria = atendimento.Categoria,
                Descricao = atendimento.Descricao,
                ImagemUrlAtual = atendimento.ImagemUrl,
                PosicaoX = atendimento.PosicaoX,
                PosicaoY = atendimento.PosicaoY,
                Zoom = (double)atendimento.Zoom
            };

            return View(model);
        }

        [HttpPost("admin/atendimentos/{id:int}/editar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarAtendimento(int id, AtendimentoEditViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var atendimento = await _context.Atendimentos.FindAsync(id);

            if (atendimento == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                model.ImagemUrlAtual = atendimento.ImagemUrl;
                return View(model);
            }

            atendimento.Titulo = model.Titulo;
            atendimento.Categoria = model.Categoria ?? string.Empty;
            atendimento.Descricao = model.Descricao;
            atendimento.PosicaoX = model.PosicaoX;
            atendimento.PosicaoY = model.PosicaoY;
            atendimento.Zoom = (decimal)model.Zoom;

            if (model.Imagem != null && model.Imagem.Length > 0)
            {
                var extensoesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extensao = Path.GetExtension(model.Imagem.FileName).ToLowerInvariant();

                if (!extensoesPermitidas.Contains(extensao))
                {
                    ModelState.AddModelError(nameof(model.Imagem), "Envie uma imagem nos formatos JPG, JPEG, PNG ou WEBP.");
                    model.ImagemUrlAtual = atendimento.ImagemUrl;
                    return View(model);
                }

                var pastaUploads = Path.Combine(_environment.WebRootPath, "uploads", "atendimentos");

                if (!Directory.Exists(pastaUploads))
                {
                    Directory.CreateDirectory(pastaUploads);
                }

                var nomeArquivo = $"atendimento-{Guid.NewGuid()}{extensao}";
                var caminhoFisico = Path.Combine(pastaUploads, nomeArquivo);

                await using (var stream = new FileStream(caminhoFisico, FileMode.Create))
                {
                    await model.Imagem.CopyToAsync(stream);
                }

                if (!string.IsNullOrWhiteSpace(atendimento.ImagemUrl))
                {
                    var caminhoAntigo = Path.Combine(
                        _environment.WebRootPath,
                        atendimento.ImagemUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)
                    );

                    if (System.IO.File.Exists(caminhoAntigo))
                    {
                        System.IO.File.Delete(caminhoAntigo);
                    }
                }

                atendimento.ImagemUrl = $"/uploads/atendimentos/{nomeArquivo}";
            }

            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] = "Atendimento atualizado com sucesso.";
            TempData["AdminMensagem"] = "Atendimento atualizado com sucesso.";

            return RedirectToAction(nameof(Atendimentos));
        }

        [HttpPost("admin/atendimentos/{id:int}/excluir")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirAtendimento(int id)
        {
            var atendimento = await _context.Atendimentos.FindAsync(id);

            if (atendimento == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(atendimento.ImagemUrl))
            {
                var caminhoArquivo = Path.Combine(
                    _environment.WebRootPath,
                    atendimento.ImagemUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)
                );

                if (System.IO.File.Exists(caminhoArquivo))
                {
                    System.IO.File.Delete(caminhoArquivo);
                }
            }

            _context.Atendimentos.Remove(atendimento);
            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] = "Atendimento excluído com sucesso.";
            TempData["AdminMensagem"] = "Atendimento excluído com sucesso.";

            return RedirectToAction(nameof(Atendimentos));
        }
    }
}