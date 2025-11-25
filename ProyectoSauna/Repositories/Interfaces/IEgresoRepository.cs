using ProyectoSauna.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories.Interfaces
{
    public interface IEgresoRepository : IRepository<DetEgreso>
    {
        Task<IEnumerable<DetEgreso>> ObtenerPorTipoAsync(int idTipoEgreso);
        Task<IEnumerable<DetEgreso>> ObtenerPorRangoFechasAsync(DateTime desde, DateTime hasta);
        Task<IEnumerable<DetEgreso>> BuscarPorConceptoAsync(string texto);
        Task<IEnumerable<DetEgreso>> ObtenerConNavegacionAsync();
    }
}
