using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabRobotica.Models
{
    [Table("Emprestimos")]
    public class Emprestimo
    {
        [Key]
        public int EmprestimoId { get; set; }

        [Required]
        [StringLength(1)]
        public String? StatusRegistro { get; set; }

        [Required]
        [StringLength(80)]
        public String? NomeAluno { get; set; }

        [Required]
        [StringLength(120)]
        public String? OcupacaoAluno { get; set; }

        [Required]
        [StringLength(11)]
        public String? TelefoneAluno { get; set; }

        [Required]
        [StringLength(120)]
        public String? EmailAluno { get; set; }

        [Required]
        public int Finalidade { get; set; }

        [Required]
        public int QuantidadeEmprestada { get; set; }

        [Required]
        public DateTime DataEmprestimo { get; set; }

        public int QuantidadeDevolvida { get; set; }

        public DateTime DataDevolucao { get; set; }

        public int QuantidadeEstragada { get; set; }

        public int Status { get; set; }

        [StringLength(1000)]
        public String? Descricao { get; set; }

        public int EstoqueId { get; set; }

        public Estoque? Estoque { get; set; }
    }
}
