using ProyectoSauna.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories.Interfaces
{
    public interface IMovimientoInventarioRepository : IRepository<MovimientoInventario>
    {
        Task<IEnumerable<MovimientoInventario>> GetByProductoAsync(int idProducto);
        Task<IEnumerable<MovimientoInventario>> GetRecentAsync(int count = 20); // últimos movimientos
    }
}
