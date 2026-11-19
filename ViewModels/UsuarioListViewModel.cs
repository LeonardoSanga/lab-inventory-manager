namespace LabRobotica.ViewModels
{
    public class UsuarioListViewModel
    {
        public int Id { get; set; }
        public string? Login { get; set; }
        public string? Senha { get; set; }
        public string? ConfirmarSenha { get; set; }
        public bool Administrador { get; set; }

        public IEnumerable<UsuarioListViewModel> List { get; set; }
    }
}
