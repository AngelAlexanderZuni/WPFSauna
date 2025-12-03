using ProyectoSauna.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories.Interfaces
{
    public interface IPagoRepository : IRepository<Pago>
    {
        Task<Pago> CrearPagoAsync(Pago pago);
        Task<IEnumerable<Pago>> GetPorCuentaAsync(int idCuenta);
        Task<IEnumerable<Pago>> GetRecientesAsync(int count = 20);
    }
}
