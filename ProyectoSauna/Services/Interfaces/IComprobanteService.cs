using ProyectoSauna.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoSauna.Services.Interfaces
{
    public interface IComprobanteService
    {
        Task<List<ComprobanteDTO>> GetAllAsync();
        Task<ComprobanteDTO?> GetByIdAsync(int id);
        Task<ComprobanteDTO> CreateAsync(ComprobanteDTO dto);
        Task UpdateAsync(ComprobanteDTO dto);
        Task DeleteAsync(int id);
        Task<List<TipoComprobanteDTO>> GetTiposComprobanteAsync();
        Task<List<ComprobanteDTO>> GetByCuentaIdAsync(int idCuenta);
    }
}
