using LabRobotica.Data;
using LabRobotica.Models;
using LabRobotica.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LabRobotica.Controllers
{
    [Authorize]
    public class ItemController : Controller
    {
        private readonly IItemRepository _itemRepository;
        private readonly IEstoqueRepository _estoqueRepository;
        private readonly IEmprestimoRepository _emprestimoRepository;
        private readonly ILogAuditoriaRepository _logAuditoriaRepository;

        public ItemController(IItemRepository repository, IEstoqueRepository estoqueRepository, IEmprestimoRepository emprestimoRepository, ILogAuditoriaRepository logAuditoriaRepository)
        {
            _itemRepository = repository;
            _estoqueRepository = estoqueRepository;
            _emprestimoRepository=emprestimoRepository;
            _logAuditoriaRepository=logAuditoriaRepository;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var items = await _itemRepository.GetAllAsync();

            var model = new ItemListViewModel();
            model.List = items.Select(i => new ItemListViewModel
            {
                Id = i.ItemId,
                Nome = i.Nome,
                Marca = i.Marca,
                Tipo = i.Tipo,
                Descricao = i.Descricao
            }).ToList();

            return View("List", model);
        }

        [HttpGet]
        public IActionResult Insert()
        {
            var model = new ItemListViewModel();

            return View("Insert", model);
        }

        [HttpPost]
        public IActionResult Insert(ItemListViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Erro"] = "Erro ao inserir Item. Verifique os campos preenchidos.";
                return RedirectToAction(nameof(List));
            }

            var item = new Item
            {
                Nome = model.Nome,
                Marca = model.Marca,
                Tipo = model.Tipo,
                Descricao = model.Descricao,
                Status = "A" // Ativo
            };

            _itemRepository.Insert(item);

            _itemRepository.Commit();

            RegistrarLog(
                "Cadastro de Item",
                $"Novo item (Id \"{item.ItemId}\" : Nome \"{item.Nome}\") cadastrado."
            );

            TempData["Sucesso"] = "Item inserido com sucesso!";

            return RedirectToAction(nameof(List));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {

            var item = _itemRepository.Get(id);

            var model = new ItemListViewModel()
            {
                Id = item.ItemId,
                Nome = item.Nome,
                Marca = item.Marca,
                Tipo = item.Tipo,
                Descricao = item.Descricao
            };

            return View("Edit", model);
        }

        [HttpPost]
        public IActionResult Edit(ItemListViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Erro"] = "Erro ao inserir Item. Verifique os campos preenchidos.";
                return View("Edit", model);
            }

            var item = _itemRepository.Get(model.Id);

            item.Nome = model.Nome;
            item.Marca = model.Marca;
            item.Tipo = model.Tipo;
            item.Descricao = model.Descricao;

            _itemRepository.Commit();

            RegistrarLog(
                "Alteração de Item",
                $"Item (Id \"{item.ItemId}\" : Nome \"{item.Nome}\") alterado."
            );

            TempData["Sucesso"] = "Item editado com sucesso!";

            return RedirectToAction(nameof(List));
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var item = _itemRepository.Get(id);
            var estoque = _estoqueRepository.GetByItemId(id);
            var emprestimo = new Emprestimo();
            if (estoque != null)
            {
                emprestimo = _emprestimoRepository.GetByEstoqueId(estoque.EstoqueId);
            }

            if (item != null)
            {
                item.Status = "I"; // Inativo

                if (estoque != null)
                {
                    estoque.Status = "I"; // Inativo
                    _estoqueRepository.Commit();
                }

                if (emprestimo != null)
                {
                    emprestimo.StatusRegistro = "I"; // Inativo
                    _emprestimoRepository.Commit();
                }

                _itemRepository.Commit();

                RegistrarLog(
                    "Deleção de Item",
                    $"Item (Id \"{item.ItemId}\" : Nome \"{item.Nome}\") deletado."
                );

                TempData["Sucesso"] = "Item excluído com sucesso!";
            }
            else
            {
                TempData["Erro"] = "Erro ao excluir Item!";
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
