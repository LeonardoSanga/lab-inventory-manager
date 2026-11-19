namespace LabRobotica.ViewModels
{
    public class ItemListViewModel
    {
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? Marca { get; set; }
        public int Tipo { get; set; }
        public string? Descricao { get; set; }

        public IEnumerable<ItemListViewModel> List;
}
}
