using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabRobotica.Models
{
    [Table("LogsAuditoria")]
    public class LogAuditoria
    {
        [Key]
        public int LogAuditoriaId { get; set; }

        [Required]
        public DateTime DataHora { get; set; }

        [Required]
        public int UsuarioResponsavelId { get; set; }

        [Required]
        [StringLength(80)]
        public String UsuarioResponsavelLogin { get; set; } = string.Empty;

        [Required]
        [StringLength(120)]
        public String Acao { get; set; } = string.Empty;

        [StringLength(500)]
        public String? Detalhes { get; set; }
    }
}
