namespace LabRobotica.ViewModels
{
    public class EstoqueListViewModel
    {
        public int Id { get; set; }
        public int QuantidadeTotal { get; set; }
        public int QuantidadeDisponivel { get; set; }
        public DateTime DataCompra {  get; set; }
        public DateTime DataUltimaEntrada { get; set; }
        public int ItemId { get; set; }
        public string? ItemNome { get; set; }
        public string? ItemMarca { get; set; }
        public int ItemTipo { get; set; }
        public IEnumerable<ItemListViewModel>? Items { get; set; }

        public IEnumerable<EstoqueListViewModel> List;
    }
}
