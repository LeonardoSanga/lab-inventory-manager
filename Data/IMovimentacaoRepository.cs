using LabRobotica.Models;

namespace LabRobotica.Data
{
    public interface IMovimentacaoRepository
    {
        IEnumerable<Movimentacao> Get(int estoqueId);
        DateTime GetMinData(int estoqueId);
        DateTime GetMaxData(int estoqueId);
        void Insert(Movimentacao movimentacao);
        void Commit();
    }
}
