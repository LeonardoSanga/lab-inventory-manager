using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabRobotica.Models
{
    public class Usuario
    {
        public int UsuarioId { get; set; }

        [Required]
        [StringLength(1)]
        public String? Status { get; set; }

        public String Login { get; set; } = string.Empty;

        public String Senha { get; set; } = string.Empty;

        public bool Administrador { get; set; } = false;
    }
}
