using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabRobotica.Models
{
    [Table("Itens")]
    public class Item
    {
        [Key]
        public int ItemId { get; set; }

        [Required]
        [StringLength(1)]
        public String? Status { get; set; }

        [Required]
        [StringLength(80)]
        public String? Nome { get; set; }

        [StringLength(100)]
        public string? Marca { get; set; }

        [Required]
        public int Tipo { get; set; }

        [StringLength(350)]
        public String? Descricao { get; set; }

        public Estoque? Estoque { get; set; }
    }
}
