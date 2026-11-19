using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabRobotica.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("List", "Item");
        }

        // Página exibida em erros inesperados (500) e páginas não encontradas (404).
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? codigo)
        {
            ViewBag.Codigo = codigo ?? 500;
            ViewBag.Mensagem = codigo switch
            {
                404 => "A página que você procura não foi encontrada.",
                403 => "Você não tem permissão para acessar esta página.",
                null or 500 => "Ocorreu um erro inesperado ao processar sua solicitação.",
                _ => "Não foi possível processar sua solicitação."
            };
            return View("Error");
        }

        [HttpGet]
        public IActionResult AcessoNegado()
        {
            Response.StatusCode = 403;
            return Error(403);
        }
    }
}
