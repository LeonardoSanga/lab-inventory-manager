using LabRobotica.Context;
using LabRobotica.Models;
using Microsoft.EntityFrameworkCore;

namespace LabRobotica.Data
{
    public class LogAuditoriaRepository : ILogAuditoriaRepository
    {
        private readonly AppDbContext _context;

        public LogAuditoriaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<LogAuditoria>> GetByUsuarioId(int usuarioId)
        {
            return await _context.LogsAuditoria.Where(l => l.UsuarioResponsavelId == usuarioId).OrderByDescending(l => l.DataHora).ToListAsync(); ;
        }

        public async Task<List<LogAuditoria>> GetAllAsync()
        {
            return await _context.LogsAuditoria.OrderByDescending(l => l.DataHora).ToListAsync();
        }

        public void Insert(LogAuditoria log)
        {
            _context.LogsAuditoria.Add(log);
        }

        public void Commit()
        {
            _context.SaveChanges();
        }
    }
}
