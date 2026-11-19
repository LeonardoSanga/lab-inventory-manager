using LabRobotica.Models;

namespace LabRobotica.Data
{
    public interface ILogAuditoriaRepository
    {
        Task<List<LogAuditoria>> GetByUsuarioId(int usuarioId);
        Task<List<LogAuditoria>> GetAllAsync();
        void Insert(LogAuditoria log);
        void Commit();
    }
}
