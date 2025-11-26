using ProyectoSauna.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories.Interfaces
{
    public interface ITipoMovimientoRepository
    {
        Task<IEnumerable<TipoMovimiento>> GetAllAsync();
        Task<TipoMovimiento> GetByIdAsync(int id);
        Task<IEnumerable<TipoMovimiento>> GetByTipoAsync(string tipo);
    }
}