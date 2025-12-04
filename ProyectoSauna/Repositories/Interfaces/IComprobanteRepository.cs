using ProyectoSauna.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories.Interfaces
{
    public interface IComprobanteRepository : IRepository<Comprobante>
    {
        Task<List<Comprobante>> GetByCuentaIdAsync(int idCuenta);
    }
}
