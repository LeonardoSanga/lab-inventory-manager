using BCrypt.Net;
using LabRobotica.Data;
using LabRobotica.Models;
using LabRobotica.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using System.Security.Claims;

namespace LabRobotica.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthRepository _repository;
        private readonly ILogAuditoriaRepository _logAuditoriaRepository;

        public AuthController(IAuthRepository repository, ILogAuditoriaRepository logAuditoriaRepository)
        {
            _repository = repository;
            _logAuditoriaRepository = logAuditoriaRepository;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity is { IsAuthenticated: true })
            {
                return RedirectToAction("List", "Item");
            }    
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(AuthViewModel model)
        {
            var usuario = _repository.Get(model.Login);

            if (usuario == null || !VerificarSenha(model.Senha, usuario.Senha))
            {
                ViewBag.Erro = "Usuário ou senha inválidos.";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
                new Claim(ClaimTypes.Name, usuario.Login),
                new Claim(ClaimTypes.Role, usuario.Administrador ? "Administrador" : "Usuario")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) // Expira em 8 horas
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            RegistrarLog(
                usuario.Login,
                "Login no sistema",
                $"Login do usuário no sistema (Administrador: {(usuario.Administrador ? "Sim" : "Não")}).",
                usuario.UsuarioId
            );

            return RedirectToAction("List", "Item");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var usuarioResponsavelLogin = User.Identity?.Name ?? "Desconhecido";
            RegistrarLog(
                usuarioResponsavelLogin,
                "Logout no sistema",
                $"Logout do usuário no sistema."
            );

            return RedirectToAction("Login", "Auth");
        }

        private bool VerificarSenha(string senhaDigitada, string hashNoBanco)
        {
            if (string.IsNullOrWhiteSpace(senhaDigitada) || string.IsNullOrWhiteSpace(hashNoBanco))
            {
                return false;
            }

            try
            {
                return BCrypt.Net.BCrypt.Verify(senhaDigitada, hashNoBanco);
            }
            catch (SaltParseException)
            {
                return false;
            }
        }

        private void RegistrarLog(string usuarioLogin, string acao, string detalhes, int usuarioId = 0)
        {

            var usuarioResponsavelId = usuarioId;
            if (usuarioResponsavelId == 0)
            {
                usuarioResponsavelId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            }

                var log = new LogAuditoria
                {
                    DataHora = DateTime.Now,
                    UsuarioResponsavelId = usuarioResponsavelId,
                    UsuarioResponsavelLogin = usuarioLogin,
                    Acao = acao,
                    Detalhes = detalhes
                };

            _logAuditoriaRepository.Insert(log);
            _logAuditoriaRepository.Commit();
        }
    }
}
