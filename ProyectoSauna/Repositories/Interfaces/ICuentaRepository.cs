using ProyectoSauna.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories.Interfaces
{
    public interface ICuentaRepository
    {
        Task<List<Cuenta>> GetCuentasPendientesAsync();
        Task<Cuenta> GetCuentaByIdAsync(int idCuenta);
        Task<int> CrearCuentaAsync(Cuenta cuenta);
        Task ActualizarCuentaAsync(Cuenta cuenta);
        Task ActualizarTotalCuentaAsync(int idCuenta);
        Task DeleteAsync(int idCuenta);
        Task<Cliente?> ObtenerClienteConProgramaAsync(int idCliente); 
    }
}