using LabRobotica.Context;
using LabRobotica.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabRobotica.Data
{
    public class ItemRepository : IItemRepository
    {
        private readonly AppDbContext _context;

        public ItemRepository(AppDbContext context)
        {
            _context = context;
        }

        public Item Get(int id)
        {
            return _context.Itens.FirstOrDefault(u => u.ItemId == id && u.Status != "I");
        }

        public async Task<List<Item>> GetAllAsync()
        {
            return await _context.Itens.Where(i => i.Status != "I").ToListAsync();
        }

        public async Task<List<Item>> GetItensSemEstoqueAsync()
        {
            return await _context.Itens
                .Where(item => item.Status != "I" && !_context.Estoques.Any(estoque => estoque.ItemId == item.ItemId))
                .ToListAsync();
        }

        public void Insert(Item item)
        {
            _context.Itens.Add(item);
        }

        public void Commit()
        {
            _context.SaveChanges();
        }

    }
}
