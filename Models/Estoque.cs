using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabRobotica.Models
{
    [Table("Estoques")]
    public class Estoque
    {
        [Key]
        public int EstoqueId { get; set; }

        [Required]
        [StringLength(1)]
        public String? Status { get; set; }

        [Required]
        public int QuantidadeTotal { get; set; }

        [Required]
        public int QuantidadeDisponivel { get; set; }

        public int ItemId { get; set; }

        public Item? Item { get; set; }

        public ICollection<Movimentacao>? Movimentacao { get; set; }

        public ICollection<Emprestimo>? Emprestimo { get; set; }
    }
}
