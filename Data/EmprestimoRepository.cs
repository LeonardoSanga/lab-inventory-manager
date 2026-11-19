using LabRobotica.Context;
using LabRobotica.Models;
using Microsoft.EntityFrameworkCore;

namespace LabRobotica.Data
{
    public class EmprestimoRepository : IEmprestimoRepository
    {
        private readonly AppDbContext _context;

        public EmprestimoRepository(AppDbContext context)
        {
            _context = context;
        }


        public Emprestimo Get(int id)
        {
            return _context.Emprestimos.FirstOrDefault(e => e.EmprestimoId == id && e.StatusRegistro != "I");
        }

        public Emprestimo GetByEstoqueId(int estoqueId)
        {
            return _context.Emprestimos.FirstOrDefault(e => e.EstoqueId == estoqueId && e.StatusRegistro != "I");
        }

        public async Task<List<Emprestimo>> GetAllAsync()
        {
            return await _context.Emprestimos.Where(e => e.StatusRegistro != "I").OrderByDescending(e => e.EmprestimoId).ToListAsync();
        }

        public void Insert(Emprestimo emprestimo)
        {
            _context.Emprestimos.Add(emprestimo);
        }

        public void Commit()
        {
            _context.SaveChanges();
        }

    }
}
