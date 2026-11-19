using LabRobotica.Context;
using LabRobotica.Models;
using Microsoft.EntityFrameworkCore;

namespace LabRobotica.Data
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _context;

        public UsuarioRepository(AppDbContext context)
        {
            _context = context;
        }

        public Usuario Get(int id)
        {
            return _context.Usuarios.FirstOrDefault(u => u.UsuarioId == id && u.Status != "I");
        }

        public Usuario GetPorLogin(string login)
        {
            return _context.Usuarios.FirstOrDefault(u => u.Login == login && u.Status != "I");
        }

        public async Task<List<Usuario>> GetAllAsync()
        {
            return await _context.Usuarios.Where(u => u.Status != "I").OrderBy(u => u.Login).ToListAsync();
        }

        public void Insert(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
        }

        public void Commit()
        {
            _context.SaveChanges();
        }
    }
}
