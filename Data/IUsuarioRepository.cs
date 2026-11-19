using LabRobotica.Models;

namespace LabRobotica.Data
{
    public interface IUsuarioRepository
    {
        Usuario Get(int id);
        Usuario GetPorLogin(string login);
        Task<List<Usuario>> GetAllAsync();
        void Insert(Usuario usuario);
        void Commit();
    }
}
