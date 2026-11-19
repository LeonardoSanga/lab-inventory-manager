namespace LabRobotica.ViewModels
{
    public class EmprestimoListViewModel
    {
        public int Id { get; set; }
        public string Aluno { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public string Curso { get; set; }
        public int Finalidade { get; set; }
        public int EstoqueId { get; set; }
        public int QuantidadeDisponivel { get; set; }
        public string ItemNome { get; set; }
        public int ItemTipo { get; set; }
        public int QuantidadeEmprestada { get; set; }
        public int QuantidadeDevolvida {  get; set; }
        public int QuantidadeEstragada { get; set; }
        public DateTime DataRetirada {  get; set; }
        public DateTime DataDevolucao { get; set; }
        public string? Descricao { get; set; }
        public int Status {  get; set; }
        public IEnumerable<EstoqueListViewModel>? Estoques { get; set; }
        public IEnumerable<EmprestimoListViewModel> List {  get; set; }
    }
}
