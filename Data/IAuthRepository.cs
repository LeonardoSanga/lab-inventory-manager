using LabRobotica.Models;

namespace LabRobotica.Data
{
    public interface IAuthRepository
    {
        Usuario Get(string login);
    }
}
