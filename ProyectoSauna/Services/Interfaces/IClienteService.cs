using ProyectoSauna.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoSauna.Services.Interfaces
{
    public interface IClienteService
    {
        Task<List<ClienteDTO>> GetAllClientesAsync();
        Task<ClienteDTO?> GetClienteByIdAsync(int id);
        Task<ClienteDTO?> GetClienteByDNIAsync(string dni);
        Task<List<ClienteDTO>> BuscarClientesPorNombreAsync(string nombre);
        Task<List<ClienteDTO>> GetClientesActivosAsync();
        Task<(bool exito, string mensaje, ClienteDTO? cliente)> CrearClienteAsync(ClienteDTO clienteDto);
        Task<(bool exito, string mensaje)> ActualizarClienteAsync(ClienteDTO clienteDto);
        Task<(bool exito, string mensaje)> DesactivarClienteAsync(int id);
        Task<bool> ValidarDNIAsync(string dni, int? idClienteExcluir = null);
    }
}