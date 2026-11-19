namespace LabRobotica.ViewModels
{
    public class LogAuditoriaListViewModel
    {
        public DateTime DataHora { get; set; }
        public string? UsuarioResponsavelLogin { get; set; }
        public string? Acao { get; set; }
        public string? Detalhes { get; set; }

        public IEnumerable<LogAuditoriaListViewModel> List { get; set; }
    }
}
