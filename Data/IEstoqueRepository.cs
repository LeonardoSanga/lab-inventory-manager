using LabRobotica.Models;

namespace LabRobotica.Data
{
    public interface IEstoqueRepository
    {
        Estoque Get(int id);
        Estoque GetByItemId(int itemId);
        Task<List<Estoque>> GetAllAsync();
        Task<List<Estoque>> GetItensComEstoqueAsync();
        void Insert(Estoque estoque);
        void Commit();
    }
}
