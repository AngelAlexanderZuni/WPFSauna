// Repositories/ClienteRepository.cs - COMPLETO
using Microsoft.EntityFrameworkCore;
using ProyectoSauna.Models;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories.Base;
using ProyectoSauna.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories
{
    public class ClienteRepository : Repository<Cliente>, IClienteRepository
    {
        private readonly SaunaDbContext _context;

        public ClienteRepository(SaunaDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Cliente?> GetByIdAsync(int idCliente)
        {
            // 🔄 FORZAR RECARGA DESDE BD (sin caché de Entity Framework)
            var cliente = await _context.Cliente
                .AsNoTracking() // No usar caché de EF
                .FirstOrDefaultAsync(c => c.idCliente == idCliente);
                
            return cliente;
        }

        public async Task<Cliente?> GetByDNIAsync(string dni)
        {
            // 🔄 FORZAR RECARGA DESDE BD (sin caché de Entity Framework)
            return await _context.Cliente
                .AsNoTracking() // No usar caché de EF
                .FirstOrDefaultAsync(c => c.numero_documento == dni);
        }

        public async Task<Cliente?> ObtenerPorDocumentoAsync(string numeroDocumento)
        {
            return await _context.Cliente.FirstOrDefaultAsync(c => c.numero_documento == numeroDocumento);
        }

        public async Task<IEnumerable<Cliente>> BuscarPorNombreAsync(string nombre)
        {
            var nombreLower = nombre.ToLower();
            return await _context.Cliente
                .Where(c => c.nombre.ToLower().Contains(nombreLower) || c.apellidos.ToLower().Contains(nombreLower))
                .OrderBy(c => c.nombre)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Cliente>> BuscarPorDNIAsync(string dni)
        {
            // Búsqueda parcial por DNI (LIKE)
            return await _context.Cliente
                .Where(c => c.numero_documento.Contains(dni))
                .OrderBy(c => c.numero_documento)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Cliente>> ObtenerActivosAsync()
        {
            return await _context.Cliente.Where(c => c.activo).OrderBy(c => c.nombre).ToListAsync();
        }

        public async Task<IEnumerable<Cliente>> GetClientesActivosAsync()
        {
            return await ObtenerActivosAsync();
        }

        public async Task<IEnumerable<Cliente>> GetClientesInactivosAsync()
        {
            return await _context.Cliente.Where(c => !c.activo).OrderBy(c => c.nombre).ToListAsync();
        }

        public async Task<IEnumerable<Cliente>> ObtenerConVisitasMinimasAsync(int visitasMinimas)
        {
            return await _context.Cliente
                .Where(c => c.visitasTotales >= visitasMinimas)
                .OrderByDescending(c => c.visitasTotales)
                .ToListAsync();
        }

        public async Task<bool> ExisteDNIAsync(string dni, int? idClienteExcluir = null)
        {
            if (string.IsNullOrWhiteSpace(dni))
                return false;

            var query = _context.Cliente.Where(c => c.numero_documento == dni);

            if (idClienteExcluir.HasValue)
                query = query.Where(c => c.idCliente != idClienteExcluir.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> UpdateActivoStatusAsync(int idCliente, bool activo)
        {
            try
            {
                // Use direct SQL update to avoid concurrency issues
                var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE Cliente SET activo = {0} WHERE idCliente = {1}", 
                    activo, 
                    idCliente);

                return rowsAffected > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateClienteDirectAsync(Models.DTOs.ClienteDTO clienteDto)
        {
            try
            {
                // First, get the current client data to compare
                var clienteActual = await _context.Cliente
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.idCliente == clienteDto.idCliente);
                
                if (clienteActual == null)
                    return false;

                // Build dynamic UPDATE query with only changed fields
                var updateFields = new List<string>();
                var parameters = new List<object> { clienteDto.idCliente };
                var parameterIndex = 1;

                // Compare and add only changed fields
                var newNombre = clienteDto.nombre?.Trim();
                if (newNombre != clienteActual.nombre)
                {
                    updateFields.Add($"nombre = {{{parameterIndex}}}");
                    parameters.Add(newNombre);
                    parameterIndex++;
                }

                var newApellidos = clienteDto.apellidos?.Trim();
                if (newApellidos != clienteActual.apellidos)
                {
                    updateFields.Add($"apellidos = {{{parameterIndex}}}");
                    parameters.Add(newApellidos);
                    parameterIndex++;
                }

                var newDocumento = clienteDto.numero_documento?.Trim();
                if (newDocumento != clienteActual.numero_documento)
                {
                    updateFields.Add($"numero_documento = {{{parameterIndex}}}");
                    parameters.Add(newDocumento);
                    parameterIndex++;
                }

                var newTelefono = string.IsNullOrWhiteSpace(clienteDto.telefono) ? null : clienteDto.telefono.Trim();
                if (newTelefono != clienteActual.telefono)
                {
                    updateFields.Add($"telefono = {{{parameterIndex}}}");
                    parameters.Add(newTelefono);
                    parameterIndex++;
                }

                var newCorreo = string.IsNullOrWhiteSpace(clienteDto.correo) ? null : clienteDto.correo.Trim().ToLower();
                if (newCorreo != clienteActual.correo)
                {
                    updateFields.Add($"correo = {{{parameterIndex}}}");
                    parameters.Add(newCorreo);
                    parameterIndex++;
                }

                var newDireccion = string.IsNullOrWhiteSpace(clienteDto.direccion) ? null : clienteDto.direccion.Trim();
                if (newDireccion != clienteActual.direccion)
                {
                    updateFields.Add($"direccion = {{{parameterIndex}}}");
                    parameters.Add(newDireccion);
                    parameterIndex++;
                }

                if (clienteDto.fechaNacimiento != clienteActual.fechaNacimiento)
                {
                    updateFields.Add($"fechaNacimiento = {{{parameterIndex}}}");
                    parameters.Add(clienteDto.fechaNacimiento);
                    parameterIndex++;
                }

                // NOTE: We deliberately don't update 'activo' here since this method is for data updates
                // The 'activo' field should only be updated through specific activation/deactivation methods

                // If no fields changed, return success (no update needed)
                if (!updateFields.Any())
                    return true;

                // Build and execute the dynamic SQL
                var sql = $"UPDATE Cliente SET {string.Join(", ", updateFields)} WHERE idCliente = {{0}}";
                var rowsAffected = await _context.Database.ExecuteSqlRawAsync(sql, parameters.ToArray());

                return rowsAffected > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}