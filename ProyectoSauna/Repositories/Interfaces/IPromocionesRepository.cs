// Repositories/Interfaces/IPromocionesRepository.cs
using ProyectoSauna.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories.Interfaces
{
    public interface IPromocionesRepository : IRepository<Promociones>
    {
        Task<IEnumerable<Promociones>> ObtenerActivasAsync();
        Task<IEnumerable<Promociones>> ObtenerPorTipoAsync(int idTipoDescuento);
        Task<IEnumerable<Promociones>> BuscarPorNombreAsync(string nombre);
        Task<Promociones?> ObtenerConTipoAsync(int idPromocion);
        Task<IEnumerable<Promociones>> ObtenerTodasConTipoAsync();
    }
}