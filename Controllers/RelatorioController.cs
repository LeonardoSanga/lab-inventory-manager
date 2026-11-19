using LabRobotica.Data;
using LabRobotica.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace LabRobotica.Controllers
{
    [Authorize]
    public class RelatorioController : Controller
    {
        private readonly IItemRepository _itemRepository;
        private readonly IEstoqueRepository _estoqueRepository;
        private readonly IEmprestimoRepository _emprestimoRepository;
        private readonly IMovimentacaoRepository _movimentacaoRepository;

        public RelatorioController(IItemRepository itemRepository, IEstoqueRepository estoqueRepository, IEmprestimoRepository emprestimoRepository, IMovimentacaoRepository movimentacaoRepository)
        {
            _itemRepository = itemRepository;
            _estoqueRepository = estoqueRepository;
            _emprestimoRepository = emprestimoRepository;
            _movimentacaoRepository = movimentacaoRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Itens()
        {
            var model = new RelatorioItensViewModel
            {
                List = await MontarRelatorioItensAsync()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Pendentes()
        {
            var model = new RelatorioPendentesViewModel
            {
                List = await MontarRelatorioPendentesAsync()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ItensCsv()
        {
            var linhas = await MontarRelatorioItensAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Nome do Item;Quantidade Total;Quantidade Disponivel;Quantidade Emprestada;Quantidade de Baixas");

            foreach (var linha in linhas)
            {
                csv.AppendLine($"{linha.ItemNome};{linha.QuantidadeTotal};{linha.QuantidadeDisponivel};{linha.QuantidadeEmprestada};{linha.QuantidadeBaixas}");
            }

            return ArquivoCsv(csv.ToString(), "relatorio_itens");
        }

        [HttpGet]
        public async Task<IActionResult> PendentesCsv()
        {
            var linhas = await MontarRelatorioPendentesAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Nome do Aluno;Telefone;Item;Data do Emprestimo;Quantidade Emprestada;Quantidade Pendente");

            foreach (var linha in linhas)
            {
                csv.AppendLine($"{linha.NomeAluno};{linha.TelefoneAluno};{linha.ItemNome};{linha.DataEmprestimo:dd/MM/yyyy};{linha.QuantidadeEmprestada};{linha.QuantidadePendente}");
            }

            return ArquivoCsv(csv.ToString(), "relatorio_pendentes");
        }

        private FileContentResult ArquivoCsv(string conteudo, string nomeArquivo)
        {
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(conteudo)).ToArray();

            return File(bytes, "text/csv", $"{nomeArquivo}_{DateTime.Now:yyyyMMdd_HHmm}.csv");
        }

        private async Task<List<RelatorioItensViewModel>> MontarRelatorioItensAsync()
        {
            var itens = await _itemRepository.GetAllAsync();
            var estoques = await _estoqueRepository.GetAllAsync();
            var emprestimos = await _emprestimoRepository.GetAllAsync();

            var resultado = new List<RelatorioItensViewModel>();

            foreach (var item in itens)
            {
                var estoque = estoques.FirstOrDefault(e => e.ItemId == item.ItemId);

                if (estoque == null)
                {
                    continue; 
                }

                var emprestimosDoItem = emprestimos.Where(e => e.EstoqueId == estoque.EstoqueId);
                var movimentacoesDoItem = _movimentacaoRepository.Get(estoque.EstoqueId);
                var movimentacoesBaixaDoItem = movimentacoesDoItem.Where(m => m.Tipo == 2).ToList();

                var quantidadeEmprestada = emprestimosDoItem
                    .Where(e => e.Status != 1 && e.Status != 3)
                    .Sum(e => e.QuantidadeEmprestada - e.QuantidadeDevolvida);

                var quantidadeBaixas = Math.Abs(movimentacoesBaixaDoItem.Sum(m => m.QuantidadeItens));

                resultado.Add(new RelatorioItensViewModel
                {
                    ItemNome = item.Nome,
                    QuantidadeTotal = estoque.QuantidadeTotal,
                    QuantidadeDisponivel = estoque.QuantidadeDisponivel,
                    QuantidadeEmprestada = quantidadeEmprestada,
                    QuantidadeBaixas = quantidadeBaixas
                });
            }

            return resultado.OrderBy(r => r.ItemNome).ToList();
        }

        private async Task<List<RelatorioPendentesViewModel>> MontarRelatorioPendentesAsync()
        {
            var emprestimos = await _emprestimoRepository.GetAllAsync();
            var estoques = await _estoqueRepository.GetAllAsync();
            var itens = await _itemRepository.GetAllAsync();

            var pendentes = emprestimos.Where(e => e.Status != 1 && e.Status != 3);

            var resultado = new List<RelatorioPendentesViewModel>();

            foreach (var emprestimo in pendentes)
            {
                var estoque = estoques.FirstOrDefault(e => e.EstoqueId == emprestimo.EstoqueId);
                var item = estoque != null ? itens.FirstOrDefault(i => i.ItemId == estoque.ItemId) : null;

                if (estoque == null || item == null)
                {
                    continue;
                }

                var quantidadePendente = emprestimo.QuantidadeEmprestada - emprestimo.QuantidadeDevolvida;

                if (quantidadePendente <= 0)
                {
                    continue;
                }

                resultado.Add(new RelatorioPendentesViewModel
                {
                    NomeAluno = emprestimo.NomeAluno,
                    TelefoneAluno = emprestimo.TelefoneAluno,
                    ItemNome = item?.Nome,
                    DataEmprestimo = emprestimo.DataEmprestimo,
                    QuantidadeEmprestada = emprestimo.QuantidadeEmprestada,
                    QuantidadePendente = quantidadePendente
                });
            }

            return resultado.OrderBy(r => r.DataEmprestimo).ToList();
        }
    }
}
