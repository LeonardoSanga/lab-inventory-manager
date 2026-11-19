using LabRobotica.Data;
using LabRobotica.Models;
using LabRobotica.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LabRobotica.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuarioController : Controller
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ILogAuditoriaRepository _logAuditoriaRepository;

        public UsuarioController(IUsuarioRepository usuarioRepository, ILogAuditoriaRepository logAuditoriaRepository)
        {
            _usuarioRepository = usuarioRepository;
            _logAuditoriaRepository = logAuditoriaRepository;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var usuarios = await _usuarioRepository.GetAllAsync();

            var model = new UsuarioListViewModel();
            model.List = usuarios.Select(u => new UsuarioListViewModel
            {
                Id = u.UsuarioId,
                Login = u.Login,
                Administrador = u.Administrador
            }).ToList();

            return View(model);
        }

        [HttpGet]
        public IActionResult Insert()
        {
            return View(new UsuarioListViewModel());
        }

        [HttpPost]
        public IActionResult Insert(UsuarioListViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Login) || string.IsNullOrWhiteSpace(model.Senha))
            {
                TempData["Erro"] = "Preencha o login e a senha do novo usuário.";
                return RedirectToAction(nameof(List));
            }

            if (model.Senha != model.ConfirmarSenha)
            {
                TempData["Erro"] = "As senhas informadas não coincidem.";
                return RedirectToAction(nameof(List));
            }

            if (_usuarioRepository.GetPorLogin(model.Login) != null)
            {
                TempData["Erro"] = "Já existe um usuário cadastrado com esse login.";
                return RedirectToAction(nameof(List));
            }

            var usuario = new Usuario
            {
                Login = model.Login,
                Senha = BCrypt.Net.BCrypt.HashPassword(model.Senha),
                Administrador = model.Administrador,
                Status = "A" // Ativo
            };

            _usuarioRepository.Insert(usuario);
            _usuarioRepository.Commit();

            RegistrarLog(
                "Cadastro de usuário",
                $"Novo usuário (Id : \"{usuario.UsuarioId}\" . Login : \"{usuario.Login}\") cadastrado (Administrador: {(usuario.Administrador ? "Sim" : "Não")})."
            );

            TempData["Sucesso"] = "Usuário cadastrado com sucesso!";

            return RedirectToAction(nameof(List));
        }

        [HttpGet]
        public async Task<IActionResult> Logs()
        {
            var logs = await _logAuditoriaRepository.GetAllAsync();

            var model = new LogAuditoriaListViewModel();
            model.List = logs.Select(l => new LogAuditoriaListViewModel
            {
                DataHora = l.DataHora,
                UsuarioResponsavelLogin = l.UsuarioResponsavelLogin,
                Acao = l.Acao,
                Detalhes = l.Detalhes
            }).ToList();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> LogsIndividual(int id)
        {
            var logs = await _logAuditoriaRepository.GetByUsuarioId(id);

            var model = new LogAuditoriaListViewModel();
            model.List = logs.Select(l => new LogAuditoriaListViewModel
            {
                DataHora = l.DataHora,
                UsuarioResponsavelLogin = l.UsuarioResponsavelLogin,
                Acao = l.Acao,
                Detalhes = l.Detalhes
            }).ToList();

            return View("Logs", model);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var usuario = _usuarioRepository.Get(id);

            if (usuario != null)
            {
                usuario.Status = "I"; // Inativo

                _usuarioRepository.Commit();

                RegistrarLog(
                    "Deleção de usuário",
                    $"Deleção do usuário (Id : \"{usuario.UsuarioId}\" . Login : \"{usuario.Login}\") do sistema (Administrador: {(usuario.Administrador ? "Sim" : "Não")})."
                );

                TempData["Sucesso"] = "Usuário excluído com sucesso!";
            }
            else
            {
                TempData["Erro"] = "Erro ao excluir Usuário!";
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
