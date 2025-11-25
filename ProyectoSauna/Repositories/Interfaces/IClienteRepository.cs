using ProyectoSauna.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories.Interfaces
{
    public interface IClienteRepository : IRepository<Cliente>
    {
        Task<Cliente?> GetByDNIAsync(string dni);
        Task<List<Cliente>> BuscarPorNombreAsync(string nombre);
        Task<List<Cliente>> GetClientesActivosAsync();
        Task<bool> ExisteDNIAsync(string dni, int? idClienteExcluir = null);
    }
}
