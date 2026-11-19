using LabRobotica.Context;
using LabRobotica.Models;

namespace LabRobotica.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _context;

        public AuthRepository(AppDbContext context) {
            _context = context;
        }

        public Usuario Get(string login)
        {
            return _context.Usuarios.FirstOrDefault(u => u.Login == login && u.Status != "I");
        }
    }
}
