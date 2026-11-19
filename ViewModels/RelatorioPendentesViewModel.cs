namespace LabRobotica.ViewModels
{
    public class RelatorioPendentesViewModel
    {
        public string? NomeAluno { get; set; }
        public string? TelefoneAluno { get; set; }
        public string? ItemNome { get; set; }
        public DateTime DataEmprestimo { get; set; }
        public int QuantidadeEmprestada { get; set; }
        public int QuantidadePendente { get; set; }

        public IEnumerable<RelatorioPendentesViewModel> List { get; set; }
    }
}
