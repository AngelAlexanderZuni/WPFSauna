using ProyectoSauna.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories.Interfaces
{
    public interface IDetalleConsumoRepository : IRepository<DetalleConsumo>
    {
        Task<IEnumerable<DetalleConsumo>> GetByCuentaAsync(int idCuenta);
        Task<DetalleConsumo> GetByIdWithIncludesAsync(int idDetalle);
    }
}