using ProyectoSauna.Models.Entities;

namespace ProyectoSauna.Repositories.Interfaces
{
    public interface IRolRepository : IRepository<Rol>
    {
        Task<Rol?> ObtenerPorNombreAsync(string nombre);
    }
}