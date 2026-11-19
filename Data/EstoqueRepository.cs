using LabRobotica.Context;
using LabRobotica.Models;
using Microsoft.EntityFrameworkCore;

namespace LabRobotica.Data
{
    public class EstoqueRepository : IEstoqueRepository
    {
        private readonly AppDbContext _context;

        public EstoqueRepository(AppDbContext context)
        {
            _context = context;
        }

        public Estoque Get(int id)
        {
            return _context.Estoques.FirstOrDefault(e => e.EstoqueId == id && e.Status != "I");
        }

        public Estoque GetByItemId(int itemId)
        {
            return _context.Estoques.FirstOrDefault(e => e.ItemId == itemId && e.Status != "I");
        }

        public async Task<List<Estoque>> GetAllAsync()
        {
            return await _context.Estoques.Where(e => e.Status != "I").ToListAsync();
        }

        public async Task<List<Estoque>> GetItensComEstoqueAsync()
        {
            return await _context.Estoques
                .Where(e => e.QuantidadeDisponivel > 0 && e.Status != "I")
                .ToListAsync();
        }

        public void Insert(Estoque estoque)
        {
            _context.Estoques.Add(estoque);
        }

        public void Commit()
        {
            _context.SaveChanges();
        }

    }
}
