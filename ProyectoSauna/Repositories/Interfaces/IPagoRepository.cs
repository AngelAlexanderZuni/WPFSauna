using ProyectoSauna.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories.Interfaces
{
    public interface IPagoRepository : IRepository<Pago>
    {
        Task<IEnumerable<Pago>> ObtenerConNavegacionAsync();
        Task<Pago> CrearAsync(Pago pago);
        Task<bool> EliminarAsync(int idPago);

        // Opcionales
        Task<IEnumerable<Pago>> ObtenerPorRangoFechasAsync(DateTime desde, DateTime hasta);
        Task<IEnumerable<Pago>> ObtenerPorMetodoAsync(int idMetodoPago);
        Task<IEnumerable<Pago>> BuscarPorReferenciaAsync(string referencia);
    }
}
