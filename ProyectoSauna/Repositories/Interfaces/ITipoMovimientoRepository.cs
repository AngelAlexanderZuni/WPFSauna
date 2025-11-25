using ProyectoSauna.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories.Interfaces
{
    public interface ITipoMovimientoRepository : IRepository<TipoMovimiento>
    {
        Task<IEnumerable<TipoMovimiento>> GetByTipoAsync(string tipo); // Entrada o Salida
    }
}
