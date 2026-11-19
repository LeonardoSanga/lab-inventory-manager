using Microsoft.AspNetCore.Authorization;
using LabRobotica.Data;
using LabRobotica.Models;
using LabRobotica.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Principal;

namespace LabRobotica.Controllers
{
    [Authorize]
    public class EmprestimoController : Controller
    {
        private readonly IEmprestimoRepository _emprestimoRepository;
        private readonly IEstoqueRepository _estoqueRepository;
        private readonly IItemRepository _itemRepository;
        private readonly IMovimentacaoRepository _movimentacaoRepository;
        private readonly ILogAuditoriaRepository _logAuditoriaRepository;

        public EmprestimoController(IEmprestimoRepository emprestimoRepository, IEstoqueRepository estoqueRepository, IItemRepository itemRepositoty, IMovimentacaoRepository movimentacaoRepository, ILogAuditoriaRepository logAuditoriaRepository)
        {
            _emprestimoRepository = emprestimoRepository;
            _estoqueRepository = estoqueRepository;
            _itemRepository = itemRepositoty;
            _movimentacaoRepository = movimentacaoRepository;
            _logAuditoriaRepository = logAuditoriaRepository;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var emprestimos = await _emprestimoRepository.GetAllAsync();

            var estoque = new Estoque();
            var item = new Item();

            var model = new EmprestimoListViewModel();
            model.List = emprestimos.Select(i =>
            {
                estoque = _estoqueRepository.Get(i.EstoqueId);

                if (estoque == null)
                    return null;

                item = _itemRepository.Get(estoque.ItemId);

                if (item == null)
                    return null;

                return new EmprestimoListViewModel
                {
                    Id = i.EmprestimoId,
                    Aluno = i.NomeAluno,
                    Email = i.EmailAluno,
                    Telefone = i.TelefoneAluno,
                    Curso = i.OcupacaoAluno,
                    Finalidade = i.Finalidade,
                    ItemNome = item.Nome,
                    ItemTipo = item.Tipo,
                    QuantidadeEmprestada = i.QuantidadeEmprestada,
                    QuantidadeDevolvida = i.QuantidadeDevolvida,
                    DataRetirada = i.DataEmprestimo,
                    DataDevolucao = i.DataDevolucao,
                    Descricao = i.Descricao,
                    Status = i.Status
                };
            }).Where(x => x != null).ToList();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Insert()
        {
            var estoques = await _estoqueRepository.GetItensComEstoqueAsync();

            var item = new Item();

            var model = new EmprestimoListViewModel();

            model.Estoques = estoques.Select(i =>
            {
                item = _itemRepository.Get(i.ItemId);

                if (item == null)
                    return null;

                return new EstoqueListViewModel
                {
                    Id = i.EstoqueId,
                    QuantidadeDisponivel = i.QuantidadeDisponivel,
                    ItemNome = item.Nome
                };
            }).Where(x => x != null).ToList();

            return View(model);
        }

        [HttpPost]
        public IActionResult Insert(EmprestimoListViewModel model)
        {
            var emprestimo = new Emprestimo()
            {
                NomeAluno = model.Aluno,
                EmailAluno = model.Email,
                TelefoneAluno = model.Telefone,
                OcupacaoAluno = model.Curso,
                Finalidade = model.Finalidade,
                EstoqueId = model.EstoqueId,
                QuantidadeEmprestada = model.QuantidadeEmprestada,
                DataEmprestimo = model.DataRetirada,
                QuantidadeDevolvida = 0,
                DataDevolucao = DateTime.MinValue,
                Status = 2,
                StatusRegistro = "A" // Ativo
            };

            _emprestimoRepository.Insert(emprestimo);

            _emprestimoRepository.Commit();

            // Atualizando a Quantidade Disponível do Estoque
            var estoque = _estoqueRepository.Get(emprestimo.EstoqueId);

            estoque.QuantidadeDisponivel -= emprestimo.QuantidadeEmprestada;

            _estoqueRepository.Commit();

            RegistrarLog(
                "Cadastro de Empréstimo",
                $"Novo empréstimo (Id: \"{emprestimo.EmprestimoId}\") para \"{emprestimo.NomeAluno}\" realizado."
            );

            return RedirectToAction(nameof(List));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var emprestimo = _emprestimoRepository.Get(id);

            var model = new EmprestimoListViewModel()
            {
                Id = emprestimo.EmprestimoId,
                Aluno = emprestimo.NomeAluno,
                QuantidadeEmprestada = emprestimo.QuantidadeEmprestada,
                QuantidadeDevolvida = emprestimo.QuantidadeDevolvida
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(EmprestimoListViewModel model)
        {
            var emprestimo = _emprestimoRepository.Get(model.Id);

            emprestimo.DataDevolucao = model.DataDevolucao;
            emprestimo.Descricao += " " + model.Descricao;

            if (emprestimo.Status == 2 || emprestimo.Status == 4)
            {
                if (model.QuantidadeEstragada > 0)
                {
                    if (model.QuantidadeDevolvida == (emprestimo.QuantidadeEmprestada - emprestimo.QuantidadeDevolvida))
                    {
                        emprestimo.Status = 3;
                    }
                    else
                    {
                        emprestimo.Status = 5;
                    }
                } else
                {
                    if (model.QuantidadeDevolvida == (emprestimo.QuantidadeEmprestada - emprestimo.QuantidadeDevolvida))
                    {
                        emprestimo.Status = 1;
                    }
                    else
                    {
                        emprestimo.Status = 4;
                    }
                }
                
            } else if (emprestimo.Status == 5)
            {
                if (model.QuantidadeDevolvida == (emprestimo.QuantidadeEmprestada - emprestimo.QuantidadeDevolvida))
                {
                    emprestimo.Status = 3;
                }
            }

            emprestimo.QuantidadeDevolvida += model.QuantidadeDevolvida;
            emprestimo.QuantidadeEstragada += model.QuantidadeEstragada;

            _emprestimoRepository.Commit();

            // Atualizando os dados do Estoque
            var estoque = _estoqueRepository.Get(emprestimo.EstoqueId);

            estoque.QuantidadeTotal -= model.QuantidadeEstragada;
            estoque.QuantidadeDisponivel += (model.QuantidadeDevolvida - model.QuantidadeEstragada);

            _estoqueRepository.Commit();

            // Registrando a movimentação caso haja baixa
            if (model.QuantidadeEstragada > 0)
            {
                var movimentacao = new Movimentacao()
                {
                    Tipo = 2,
                    QuantidadeItens = -model.QuantidadeEstragada,
                    Data = model.DataDevolucao,
                    Observacao = emprestimo.NomeAluno + " - " + model.Descricao,
                    EstoqueId = emprestimo.EstoqueId,
                };

                _movimentacaoRepository.Insert(movimentacao);

                _movimentacaoRepository.Commit();
            }

            RegistrarLog(
                "Devolução de Empréstimo",
                $"Empréstimo (Id: \"{emprestimo.EmprestimoId}\") de \"{emprestimo.NomeAluno}\" devolvido."
            );

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
