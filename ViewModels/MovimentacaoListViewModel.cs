namespace LabRobotica.ViewModels
{
    public class MovimentacaoListViewModel
    {
        public int Tipo { get; set; }
        public int QuantidadeMovimentada { get; set; }
        public int QuantidadeAnterior { get; set; }
        public int EstoqueId { get; set; }
        public DateTime Data { get; set; }
        public string? Observacao { get; set; }
        public IEnumerable<MovimentacaoListViewModel> List { get; set; }

        public string? NomeItemEstoque { get; set; }
        public string? MarcaItemEstoque { get; set; }
        public int QuantidadeTotal { get; set; }
        public int QuantidadeDisponivel { get; set; }
    }
}
