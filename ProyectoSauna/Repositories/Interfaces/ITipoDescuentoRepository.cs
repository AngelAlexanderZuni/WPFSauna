// Repositories/Interfaces/ITipoDescuentoRepository.cs
using ProyectoSauna.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories.Interfaces
{
    public interface ITipoDescuentoRepository : IRepository<TipoDescuento>
    {
        Task<TipoDescuento?> ObtenerPorNombreAsync(string nombre);
    }
}