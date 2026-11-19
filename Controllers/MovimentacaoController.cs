using Microsoft.AspNetCore.Authorization;
using LabRobotica.Data;
using LabRobotica.Models;
using LabRobotica.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LabRobotica.Controllers
{
    [Authorize]
    public class MovimentacaoController : Controller
    {
        private readonly IMovimentacaoRepository _movimentacaoRepository;
        private readonly IItemRepository _itemRepository;
        private readonly IEstoqueRepository _estoqueRepository;
        
        public MovimentacaoController(IMovimentacaoRepository movimentacaoRepository, IItemRepository itemRepository, IEstoqueRepository estoqueRepository)
        {
            _movimentacaoRepository = movimentacaoRepository;
            _itemRepository = itemRepository;
            _estoqueRepository = estoqueRepository;
        }

        [HttpGet]
        public IActionResult List(int id)
        {
            var estoque = _estoqueRepository.Get(id);
            
            var movimentacao = _movimentacaoRepository.Get(estoque.EstoqueId);

            var item = _itemRepository.Get(estoque.ItemId);

            var model = new MovimentacaoListViewModel()
            {
                NomeItemEstoque = item.Nome,
                MarcaItemEstoque = item.Marca,
                QuantidadeTotal = estoque.QuantidadeTotal,
                QuantidadeDisponivel = estoque.QuantidadeDisponivel
            };

            model.List = movimentacao.Select(i => new MovimentacaoListViewModel
            {
                Tipo = i.Tipo,
                QuantidadeMovimentada = i.QuantidadeItens,
                Data = i.Data,
                Observacao = i.Observacao
            }).ToList();

            return View(model);
        }

        [HttpGet]
        public IActionResult Insert(int estoqueId, int qtdAnterior)
        {
            var estoque = _estoqueRepository.Get(estoqueId);

            var diferenca = estoque.QuantidadeTotal - qtdAnterior;

            var model = new MovimentacaoListViewModel()
            {
                EstoqueId = estoque.EstoqueId,
                QuantidadeAnterior = qtdAnterior,
                QuantidadeMovimentada = diferenca
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Insert(MovimentacaoListViewModel model)
        {
            var movimentacao = new Movimentacao()
            {
                Tipo = model.Tipo,
                QuantidadeItens = model.QuantidadeMovimentada,
                Data = model.Data,
                Observacao = model.Observacao,
                EstoqueId = model.EstoqueId,
            };

            _movimentacaoRepository.Insert(movimentacao);

            _movimentacaoRepository.Commit();

            return RedirectToAction("List", "Estoque");
        }
    }
}
