// Services/ClienteService.cs - COMPLETAMENTE CORREGIDO
using ProyectoSauna.Models.DTOs;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Models;
using ProyectoSauna.Repositories.Interfaces;
using ProyectoSauna.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProyectoSauna.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly ClienteConcurrencyService? _concurrencyService;
        private readonly ClienteAuditService? _auditService;
        private readonly bool _useConcurrencyControl;

        // Constructor principal (mantiene compatibilidad)
        public ClienteService(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
            _useConcurrencyControl = false;
        }

        // Constructor extendido con control de concurrencia (opcional)
        public ClienteService(IClienteRepository clienteRepository, 
                             ClienteConcurrencyService concurrencyService, 
                             ClienteAuditService auditService,
                             bool useConcurrencyControl = false)
        {
            _clienteRepository = clienteRepository;
            _concurrencyService = concurrencyService;
            _auditService = auditService;
            _useConcurrencyControl = useConcurrencyControl;
        }

        public async Task<List<ClienteDTO>> GetAllClientesAsync()
        {
            var clientes = await _clienteRepository.GetAllAsync();
            return clientes.Select(MapToDTO).ToList();
        }

        // Alias para compatibilidad
        public async Task<List<ClienteDTO>> GetAllAsync()
        {
            return await GetAllClientesAsync();
        }

        public async Task<ClienteDTO?> GetClienteByIdAsync(int id)
        {
            var cliente = await _clienteRepository.GetByIdAsync(id);
            return cliente != null ? MapToDTO(cliente) : null;
        }

        public async Task<ClienteDTO?> GetClienteByDNIAsync(string dni)
        {
            var cliente = await _clienteRepository.GetByDNIAsync(dni);
            return cliente != null ? MapToDTO(cliente) : null;
        }

        public async Task<List<ClienteDTO>> BuscarClientesPorNombreAsync(string nombre)
        {
            var clientes = await _clienteRepository.BuscarPorNombreAsync(nombre);
            return clientes.Select(MapToDTO).ToList();
        }

        public async Task<List<ClienteDTO>> BuscarClientesPorDNIAsync(string dni)
        {
            var clientes = await _clienteRepository.BuscarPorDNIAsync(dni);
            return clientes.Select(MapToDTO).ToList();
        }

        public async Task<List<ClienteDTO>> GetClientesActivosAsync()
        {
            var clientes = await _clienteRepository.GetClientesActivosAsync();
            return clientes.Select(MapToDTO).ToList();
        }

        public async Task<List<ClienteDTO>> GetClientesInactivosAsync()
        {
            var clientes = await _clienteRepository.GetClientesInactivosAsync();
            return clientes.Select(MapToDTO).ToList();
        }

        public async Task<(bool exito, string mensaje, ClienteDTO? cliente)> CrearClienteAsync(ClienteDTO clienteDto)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var operation = new ClienteOperation
            {
                TipoOperacion = "Crear",
                DNI = clienteDto.numero_documento ?? "",
                Usuario = ProyectoSauna.Models.SesionActual.EstaLogueado 
                    ? $"{ProyectoSauna.Models.SesionActual.NombreCompleto} ({ProyectoSauna.Models.SesionActual.Rol})"
                    : "Sistema"
            };

            try
            {
                // Si el control de concurrencia está habilitado, usar el servicio especializado
                if (_useConcurrencyControl && _concurrencyService != null)
                {
                    var result = await _concurrencyService.CrearClienteConcurrenteAsync(clienteDto);
                    
                    operation.ClienteId = result.cliente?.idCliente;
                    operation.Resultado = result.exito ? "Éxito" : "Error";
                    operation.DetallesError = result.exito ? null : result.mensaje;
                    operation.DuracionMs = stopwatch.ElapsedMilliseconds;
                    
                    await LogOperationAsync(operation);
                    return result;
                }

                // Funcionamiento original (sin cambios para mantener compatibilidad)
                var validacion = ValidarCliente(clienteDto);
                if (!validacion.valido)
                {
                    operation.Resultado = "Error";
                    operation.DetallesError = validacion.mensaje;
                    operation.DuracionMs = stopwatch.ElapsedMilliseconds;
                    await LogOperationAsync(operation);
                    return (false, validacion.mensaje, null);
                }

                if (await _clienteRepository.ExisteDNIAsync(clienteDto.numero_documento))
                {
                    operation.Resultado = "Conflicto";
                    operation.DetallesError = "DNI duplicado";
                    operation.DuracionMs = stopwatch.ElapsedMilliseconds;
                    await LogOperationAsync(operation);
                    return (false, "Ya existe un cliente con ese número de documento.", null);
                }

                var cliente = new Cliente
                {
                    nombre = clienteDto.nombre.Trim(),
                    apellidos = clienteDto.apellidos.Trim(),
                    numero_documento = clienteDto.numero_documento.Trim(),
                    telefono = string.IsNullOrWhiteSpace(clienteDto.telefono) ? null : clienteDto.telefono.Trim(),
                    correo = string.IsNullOrWhiteSpace(clienteDto.correo) ? null : clienteDto.correo.Trim().ToLower(),
                    direccion = string.IsNullOrWhiteSpace(clienteDto.direccion) ? null : clienteDto.direccion.Trim(),
                    fechaNacimiento = clienteDto.fechaNacimiento,
                    fechaRegistro = DateTime.Now,
                    visitasTotales = 0,
                    activo = true
                };

                await _clienteRepository.AddAsync(cliente);

                operation.ClienteId = cliente.idCliente;
                operation.Resultado = "Éxito";
                operation.DuracionMs = stopwatch.ElapsedMilliseconds;
                await LogOperationAsync(operation);

                return (true, "Cliente registrado exitosamente.", MapToDTO(cliente));
            }
            catch (DbUpdateException dbEx)
            {
                operation.Resultado = "Error";
                operation.DetallesError = dbEx.InnerException?.Message ?? dbEx.Message;
                operation.DuracionMs = stopwatch.ElapsedMilliseconds;
                await LogOperationAsync(operation);
                return (false, $"Error al guardar en la base de datos: {dbEx.InnerException?.Message ?? dbEx.Message}", null);
            }
            catch (Exception ex)
            {
                operation.Resultado = "Error";
                operation.DetallesError = ex.Message;
                operation.DuracionMs = stopwatch.ElapsedMilliseconds;
                await LogOperationAsync(operation);
                return (false, $"Error inesperado: {ex.Message}", null);
            }
        }

        public async Task<(bool exito, string mensaje)> ActualizarClienteAsync(ClienteDTO clienteDto)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var operation = new ClienteOperation
            {
                TipoOperacion = "Actualizar",
                ClienteId = clienteDto.idCliente,
                DNI = clienteDto.numero_documento ?? "",
                Usuario = ProyectoSauna.Models.SesionActual.EstaLogueado 
                    ? $"{ProyectoSauna.Models.SesionActual.NombreCompleto} ({ProyectoSauna.Models.SesionActual.Rol})"
                    : "Sistema"
            };

            try
            {
                // Si el control de concurrencia está habilitado, usar el servicio especializado
                if (_useConcurrencyControl && _concurrencyService != null)
                {
                    var result = await _concurrencyService.ActualizarClienteConcurrenteAsync(clienteDto);
                    
                    operation.Resultado = result.exito ? "Éxito" : "Error";
                    operation.DetallesError = result.exito ? null : result.mensaje;
                    operation.DuracionMs = stopwatch.ElapsedMilliseconds;
                    
                    await LogOperationAsync(operation);
                    return result;
                }

                // Funcionamiento original (sin cambios para mantener compatibilidad)
                var validacion = ValidarCliente(clienteDto);
                if (!validacion.valido)
                {
                    operation.Resultado = "Error";
                    operation.DetallesError = validacion.mensaje;
                    operation.DuracionMs = stopwatch.ElapsedMilliseconds;
                    await LogOperationAsync(operation);
                    return (false, validacion.mensaje);
                }

                // Verificar si existe otro cliente con el mismo DNI
                if (await _clienteRepository.ExisteDNIAsync(clienteDto.numero_documento, clienteDto.idCliente))
                {
                    operation.Resultado = "Conflicto";
                    operation.DetallesError = "DNI duplicado en otro cliente";
                    operation.DuracionMs = stopwatch.ElapsedMilliseconds;
                    await LogOperationAsync(operation);
                    return (false, "Ya existe otro cliente con ese número de documento.");
                }

                // Use direct SQL update to avoid tracking issues
                var updateResult = await _clienteRepository.UpdateClienteDirectAsync(clienteDto);
                
                if (updateResult)
                {
                    operation.Resultado = "Éxito";
                    operation.DuracionMs = stopwatch.ElapsedMilliseconds;
                    await LogOperationAsync(operation);
                    return (true, "Cliente actualizado exitosamente.");
                }
                else
                {
                    operation.Resultado = "Error";
                    operation.DetallesError = "Cliente no encontrado o sin cambios";
                    operation.DuracionMs = stopwatch.ElapsedMilliseconds;
                    await LogOperationAsync(operation);
                    return (false, "No se pudo encontrar el cliente o no se realizaron cambios.");
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                var usuarioActual = ProyectoSauna.Models.SesionActual.EstaLogueado 
                    ? $"{ProyectoSauna.Models.SesionActual.NombreCompleto} ({ProyectoSauna.Models.SesionActual.Rol})"
                    : "otro usuario";
                    
                operation.Resultado = "Conflicto";
                operation.DetallesError = $"Concurrencia - modificado por {usuarioActual}";
                operation.DuracionMs = stopwatch.ElapsedMilliseconds;
                await LogOperationAsync(operation);
                
                // Mensaje simplificado - el bloqueo preventivo debería evitar este escenario
                return (false, $"El cliente fue modificado por otro usuario. Recargue los datos e intente nuevamente.");
            }
            catch (DbUpdateException dbEx)
            {
                operation.Resultado = "Error";
                operation.DetallesError = dbEx.InnerException?.Message ?? dbEx.Message;
                operation.DuracionMs = stopwatch.ElapsedMilliseconds;
                await LogOperationAsync(operation);
                return (false, $"Error al actualizar en la base de datos: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                operation.Resultado = "Error";
                operation.DetallesError = ex.Message;
                operation.DuracionMs = stopwatch.ElapsedMilliseconds;
                await LogOperationAsync(operation);
                return (false, $"Error inesperado: {ex.Message}");
            }
        }

        public async Task<(bool exito, string mensaje)> DesactivarClienteAsync(int id)
        {
            try
            {
                // Use direct update approach to avoid concurrency issues
                var result = await _clienteRepository.UpdateActivoStatusAsync(id, false);
                
                if (result)
                {
                    return (true, "Cliente desactivado exitosamente.");
                }
                else
                {
                    return (false, "No se pudo encontrar el cliente o ya estaba inactivo.");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error al desactivar cliente: {ex.Message}");
            }
        }

        public async Task<(bool exito, string mensaje)> ReactivarClienteAsync(int id)
        {
            try
            {
                // Use direct update approach to avoid concurrency issues
                var result = await _clienteRepository.UpdateActivoStatusAsync(id, true);
                
                if (result)
                {
                    return (true, "Cliente reactivado exitosamente.");
                }
                else
                {
                    return (false, "No se pudo encontrar el cliente o ya estaba activo.");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error al reactivar cliente: {ex.Message}");
            }
        }

        public async Task<bool> ValidarDNIAsync(string dni, int? idClienteExcluir = null)
        {
            return !await _clienteRepository.ExisteDNIAsync(dni, idClienteExcluir);
        }

        private (bool valido, string mensaje) ValidarCliente(ClienteDTO cliente)
        {
            if (string.IsNullOrWhiteSpace(cliente.nombre))
                return (false, "El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(cliente.apellidos))
                return (false, "Los apellidos son obligatorios.");

            if (string.IsNullOrWhiteSpace(cliente.numero_documento))
                return (false, "El número de documento es obligatorio.");

            if (!Regex.IsMatch(cliente.numero_documento, @"^\d{8}$"))
                return (false, "El DNI debe tener exactamente 8 dígitos.");

            if (!string.IsNullOrWhiteSpace(cliente.telefono))
            {
                if (!Regex.IsMatch(cliente.telefono, @"^\d{9}$"))
                    return (false, "El teléfono debe tener 9 dígitos.");
            }

            if (!string.IsNullOrWhiteSpace(cliente.correo))
            {
                var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                if (!Regex.IsMatch(cliente.correo, emailPattern))
                    return (false, "El correo electrónico no es válido.");
            }

            if (cliente.fechaNacimiento.HasValue)
            {
                var hoy = DateTime.Today;
                if (cliente.fechaNacimiento.Value >= hoy)
                    return (false, "La fecha de nacimiento debe ser anterior a hoy.");

                int edad = hoy.Year - cliente.fechaNacimiento.Value.Year;
                if (cliente.fechaNacimiento.Value > hoy.AddYears(-edad)) edad--;

                if (edad < 10)
                    return (false, "El cliente debe tener al menos 10 años.");
            }

            return (true, string.Empty);
        }

        private ClienteDTO MapToDTO(Cliente cliente)
        {
            return new ClienteDTO
            {
                idCliente = cliente.idCliente,
                nombre = cliente.nombre,
                apellidos = cliente.apellidos,
                numero_documento = cliente.numero_documento,
                telefono = cliente.telefono ?? string.Empty,
                correo = cliente.correo,
                direccion = cliente.direccion,
                fechaNacimiento = cliente.fechaNacimiento,
                fechaRegistro = cliente.fechaRegistro,
                visitasTotales = cliente.visitasTotales,
                activo = cliente.activo
            };
        }

        /// <summary>
        /// Registra una operación en el servicio de auditoría si está disponible
        /// </summary>
        private async Task LogOperationAsync(ClienteOperation operation)
        {
            if (_auditService != null)
            {
                await _auditService.LogOperationAsync(operation);
            }
        }

        /// <summary>
        /// Habilita o deshabilita el control de concurrencia en tiempo de ejecución
        /// Solo disponible si el servicio fue configurado con control de concurrencia
        /// </summary>
        public bool CanUseConcurrencyControl => _concurrencyService != null && _auditService != null;

        /// <summary>
        /// Obtiene estadísticas de concurrencia si el servicio está disponible
        /// </summary>
        public ConcurrencyStats? GetConcurrencyStats()
        {
            return _concurrencyService?.GetConcurrencyStats();
        }

        /// <summary>
        /// Obtiene estadísticas de operaciones si el servicio de auditoría está disponible
        /// </summary>
        public OperationStats? GetOperationStats()
        {
            return _auditService?.GetOperationStats();
        }

        /// <summary>
        /// Detecta problemas potenciales de concurrencia
        /// </summary>
        public List<string> DetectConcurrencyIssues()
        {
            return _auditService?.DetectPotentialConcurrencyIssues() ?? new List<string>();
        }
    }
}