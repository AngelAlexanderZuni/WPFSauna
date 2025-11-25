using ProyectoSauna.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories.Interfaces
{
    public interface IDetalleServicioRepository : IRepository<DetalleServicio>
    {
        Task<IEnumerable<DetalleServicio>> GetByServicioAsync(int idServicio);
        Task<IEnumerable<DetalleServicio>> GetByCuentaAsync(int idCuenta);
        Task<IEnumerable<DetalleServicio>> GetRecentAsync(int count = 20);
        Task<IEnumerable<DetalleServicio>> GetAllWithIncludesAsync();
    }
}