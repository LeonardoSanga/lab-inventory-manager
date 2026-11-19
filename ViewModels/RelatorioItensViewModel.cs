namespace LabRobotica.ViewModels
{
    public class RelatorioItensViewModel
    {
        public string? ItemNome { get; set; }
        public int QuantidadeTotal { get; set; }
        public int QuantidadeDisponivel { get; set; }
        public int QuantidadeEmprestada { get; set; }
        public int QuantidadeBaixas { get; set; }

        public IEnumerable<RelatorioItensViewModel> List { get; set; }
    }
}
