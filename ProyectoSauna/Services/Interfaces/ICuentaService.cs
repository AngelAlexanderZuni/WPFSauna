using ProyectoSauna.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoSauna.Services.Interfaces
{
    public interface ICuentaService
    {
        // Métodos básicos que probablemente se necesiten, ajustables según requerimientos futuros
        Task<CuentaDTO?> GetByIdAsync(int id);
        Task<List<CuentaDTO>> GetCuentasActivasAsync();
        // Agrega más métodos según sea necesario para el módulo de pagos si interactúa con cuentas
    }
}
