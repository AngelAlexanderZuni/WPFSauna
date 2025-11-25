using System.Collections.Generic;
using System.Threading.Tasks;
using ProyectoSauna.Models.Entities;

namespace ProyectoSauna.Repositories.Interfaces
{
    public interface ICuentaRepository
    {
        Task<List<Cuenta>> GetCuentasPendientesAsync();
        Task<Cuenta?> GetCuentaActivaDeClienteAsync(int idCliente);
        Task<int> CrearCuentaAsync(Cuenta cuenta);
        Task ActualizarCuentaAsync(Cuenta cuenta);
        Task<Cuenta?> GetCuentaByIdAsync(int idCuenta);
    }
}
