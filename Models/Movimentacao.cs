using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabRobotica.Models
{
    [Table("Movimentacoes")]
    public class Movimentacao
    {
        [Key]
        public int MovimentacaoId { get; set; }

        [Required]
        public int Tipo { get; set; }

        [Required]
        public int QuantidadeItens { get; set; }

        [Required]
        public DateTime Data { get; set; }

        [Required]
        [StringLength(500)]
        public String? Observacao { get; set; }

        public int EstoqueId { get; set; }

        public Estoque? Estoque { get; set; }
    }
}
