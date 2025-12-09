// Repositories/Interfaces/IClienteRepository.cs - COMPLETO
using ProyectoSauna.Models.DTOs;
using ProyectoSauna.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories.Interfaces
{
    public interface IClienteRepository : IRepository<Cliente>
    {
        Task<Cliente?> GetByIdAsync(int idCliente);
        Task<Cliente?> GetByDNIAsync(string dni);
        Task<Cliente?> ObtenerPorDocumentoAsync(string numeroDocumento);
        Task<IEnumerable<Cliente>> BuscarPorNombreAsync(string nombre);
        Task<IEnumerable<Cliente>> BuscarPorDNIAsync(string dni);
        Task<IEnumerable<Cliente>> ObtenerActivosAsync();
        Task<IEnumerable<Cliente>> GetClientesActivosAsync();
        Task<IEnumerable<Cliente>> GetClientesInactivosAsync();
        Task<IEnumerable<Cliente>> ObtenerConVisitasMinimasAsync(int visitasMinimas);
        Task<bool> ExisteDNIAsync(string dni, int? idClienteExcluir = null);
        Task<bool> UpdateActivoStatusAsync(int idCliente, bool activo);
        Task<bool> UpdateClienteDirectAsync(ClienteDTO clienteDto);
    }
}