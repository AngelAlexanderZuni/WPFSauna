using ProyectoSauna.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories.Interfaces
{
    public interface IMovimientoInventarioRepository
    {
        Task<IEnumerable<MovimientoInventario>> GetAllAsync();
        Task<MovimientoInventario> GetByIdAsync(int id);
        Task<IEnumerable<MovimientoInventario>> GetByProductoAsync(int idProducto);
        Task<IEnumerable<MovimientoInventario>> GetRecentAsync(int count = 50); 
        Task<int> AddAsync(MovimientoInventario movimiento);
        Task UpdateAsync(MovimientoInventario movimiento);
        Task DeleteAsync(int id);
    }
}