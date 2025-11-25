using ProyectoSauna.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories.Interfaces
{
    public interface IServicioRepository : IRepository<Servicio>
    {
        Task<IEnumerable<Servicio>> ObtenerPorCategoriaAsync(int idCategoriaServicio);
        Task<IEnumerable<Servicio>> BuscarPorNombreAsync(string nombre);
    }
}