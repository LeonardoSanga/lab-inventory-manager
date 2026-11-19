using LabRobotica.Context;
using LabRobotica.Models;

namespace LabRobotica.Data
{
    public class MovimentacaoRepository : IMovimentacaoRepository
    {
        private readonly AppDbContext _context;

        public MovimentacaoRepository(AppDbContext context) 
        {
            _context=context;
        }

        public IEnumerable<Movimentacao> Get(int estoqueId)
        {
            return _context.Movimentacoes.Where(m => m.EstoqueId == estoqueId).OrderByDescending(m => m.Data).ToList();
        }

        public DateTime GetMinData(int estoqueId)
        {
            var movimentacao =  _context.Movimentacoes.Where(m => m.EstoqueId == estoqueId).OrderBy(m => m.Data).FirstOrDefault();

            if (movimentacao == null)
            {
                return DateTime.Now;
            }

            return movimentacao.Data;
        }

        public DateTime GetMaxData(int estoqueId)
        {
            var movimentacao = _context.Movimentacoes.Where(m => m.EstoqueId == estoqueId).OrderByDescending(m => m.Data).FirstOrDefault();

            if (movimentacao == null)
            {
                return DateTime.Now;
            }

            return movimentacao.Data;
        }

        public void Insert(Movimentacao movimentacao)
        {
            _context.Movimentacoes.Add(movimentacao);
        }

        public void Commit()
        {
            _context.SaveChanges();
        }
    }
}
