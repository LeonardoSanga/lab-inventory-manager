using LabRobotica.Models;
using Microsoft.AspNetCore.Mvc;

namespace LabRobotica.Data
{
    public interface IItemRepository
    {
        Item Get(int id);
        Task<List<Item>> GetAllAsync();
        Task<List<Item>> GetItensSemEstoqueAsync();
        void Insert(Item item);
        void Commit();
    }
}
