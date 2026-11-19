using Microsoft.AspNetCore.Authorization;
using LabRobotica.Data;
using LabRobotica.Models;
using LabRobotica.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace LabRobotica.Controllers
{
    [Authorize]
    public class EstoqueController : Controller
    {
        private readonly IEstoqueRepository _estoqueRepository;
        private readonly IItemRepository _itemRepository;
        private readonly IMovimentacaoRepository _movimentacaoRepository;
        private readonly ILogAuditoriaRepository _logAuditoriaRepository;

        public EstoqueController(IEstoqueRepository estoqueRepository, IItemRepository itemRepository, IMovimentacaoRepository movimentacaoRepository, ILogAuditoriaRepository logAuditoriaRepository)
        {
            _estoqueRepository = estoqueRepository;
            _itemRepository = itemRepository;
            _movimentacaoRepository = movimentacaoRepository;
            _logAuditoriaRepository=logAuditoriaRepository;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var estoques = await _estoqueRepository.GetAllAsync();

            var item = new Item();

            var dataCompra = new DateTime();
            var dataUltCompra = new DateTime();

            var model = new EstoqueListViewModel();
            model.List = estoques.Select(i =>
            {

                item = _itemRepository.Get(i.ItemId);

                if (item == null)
                    return null;

                dataCompra = _movimentacaoRepository.GetMinData(i.EstoqueId);
                dataUltCompra = _movimentacaoRepository.GetMaxData(i.EstoqueId); 

                return new EstoqueListViewModel
                {
                    Id = i.EstoqueId,
                    QuantidadeTotal = i.QuantidadeTotal,
                    QuantidadeDisponivel = i.QuantidadeDisponivel,
                    DataCompra = dataCompra,
                    DataUltimaEntrada = dataUltCompra,
                    ItemId = i.ItemId,
                    ItemNome = item.Nome,
                    ItemMarca = item.Marca,
                    ItemTipo = item.Tipo
                };
            }).Where(x => x != null).ToList(); 

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Insert()
        {
            var items = await _itemRepository.GetItensSemEstoqueAsync();

            var model = new EstoqueListViewModel();

            model.Items = items.Select(i => new ItemListViewModel
            {
                Id = i.ItemId,
                Nome = i.Nome
            }).ToList();

            return View(model);

        }

        [HttpPost]
        public IActionResult Insert(EstoqueListViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Erro"] = "Erro ao inserir Estoque. Verifique os campos preenchidos.";
                return RedirectToAction(nameof(List));
            }

            var item = _itemRepository.Get(model.ItemId);

            if (item == null)
            {
                TempData["Erro"] = "Erro ao inserir Estoque. Item não existe.";
                return RedirectToAction(nameof(List));
            }

            var estoque = new Estoque()
            {
                ItemId = item.ItemId,
                QuantidadeTotal = model.QuantidadeTotal,
                QuantidadeDisponivel = model.QuantidadeTotal,
                Status = "A" // Ativo
            };

            _estoqueRepository.Insert(estoque);

            _estoqueRepository.Commit();

            RegistrarLog(
                "Cadastro de Estoque",
                $"Novo estoque (Id \"{estoque.EstoqueId}\" : Item : \"{item.Nome}\") cadastrado."
            );

            return RedirectToAction("Insert", "Movimentacao", new { estoqueId = estoque.EstoqueId, qtdAnterior = 0 }); 
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var estoque = _estoqueRepository.Get(id);

            if (estoque == null)
            {
                TempData["Erro"] = "Erro ao editar Estoque. Verifique os campos preenchidos.";
                return RedirectToAction(nameof(List));
            }

            var item = _itemRepository.Get(estoque.ItemId);

            var model = new EstoqueListViewModel
            {
                Id = id,
                ItemId = item.ItemId,
                ItemNome = item.Nome,
                QuantidadeTotal = estoque.QuantidadeTotal,
                QuantidadeDisponivel = estoque.QuantidadeDisponivel
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(EstoqueListViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Erro"] = "Erro ao editar Estoque. Verifique os campos preenchidos.";
                return View("Edit", model);
            }

            var estoque = _estoqueRepository.Get(model.Id);

            var quantidadeAnterior = estoque.QuantidadeTotal;

            var diferenca = quantidadeAnterior - model.QuantidadeTotal;

            estoque.QuantidadeTotal = model.QuantidadeTotal;
            estoque.QuantidadeDisponivel -= diferenca;

            if (estoque.QuantidadeDisponivel < 0)
            {
                estoque.QuantidadeDisponivel = 0;
            }

            _estoqueRepository.Commit();

            var item = _itemRepository.Get(estoque.ItemId);
            RegistrarLog(
                "Alteração de Estoque",
                $"Estoque (Id \"{estoque.EstoqueId}\" : Item : \"{item.Nome}\") alterado."
            );

            return RedirectToAction("Insert", "Movimentacao", new { estoqueId = estoque.EstoqueId, qtdAnterior = quantidadeAnterior });
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var estoque = _estoqueRepository.Get(id);

            if (estoque != null)
            {
                estoque.Status = "I"; // Inativo

                _estoqueRepository.Commit();

                var item = _itemRepository.Get(estoque.ItemId);
                RegistrarLog(
                    "Deleção de Estoque",
                    $"Estoque (Id \"{estoque.EstoqueId}\" : Item : \"{item.Nome}\") deletado."
                );

                TempData["Sucesso"] = "Estoque excluído com sucesso!";
            }
            else
            {
                TempData["Erro"] = "Erro ao excluir Estoque!";
            }

            return RedirectToAction(nameof(List));
        }

        private void RegistrarLog(string acao, string detalhes)
        {
            var usuarioResponsavelId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var usuarioResponsavelLogin = User.Identity?.Name ?? "Desconhecido";

            var log = new LogAuditoria
            {
                DataHora = DateTime.Now,
                UsuarioResponsavelId = usuarioResponsavelId,
                UsuarioResponsavelLogin = usuarioResponsavelLogin,
                Acao = acao,
                Detalhes = detalhes
            };

            _logAuditoriaRepository.Insert(log);
            _logAuditoriaRepository.Commit();
        }
    }
}
