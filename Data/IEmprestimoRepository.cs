using LabRobotica.Models;

namespace LabRobotica.Data
{
    public interface IEmprestimoRepository
    {
        Emprestimo Get(int id);
        Emprestimo GetByEstoqueId(int estoqueId);
        Task<List<Emprestimo>> GetAllAsync();
        void Insert(Emprestimo emprestimo);
        void Commit();
    }
}
