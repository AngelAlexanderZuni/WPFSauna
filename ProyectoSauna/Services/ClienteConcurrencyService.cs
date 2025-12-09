// Services/ClienteConcurrencyService.cs - Control avanzado de concurrencia para clientes
using Microsoft.EntityFrameworkCore;
using ProyectoSauna.Models;
using ProyectoSauna.Models.DTOs;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories.Interfaces;
using System.Collections.Concurrent;

namespace ProyectoSauna.Services
{
    /// <summary>
    /// Servicio especializado en control de concurrencia para operaciones de clientes
    /// Maneja: Creación simultánea, Actualizaciones concurrentes, Validación de DNI único
    /// </summary>
    public class ClienteConcurrencyService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly SaunaDbContext _context;
        
        // Cache thread-safe para evitar duplicados de DNI durante creación simultánea
        private static readonly ConcurrentDictionary<string, DateTime> _dniCreationLock = new();
        private static readonly ConcurrentDictionary<int, DateTime> _clienteUpdateLock = new();
        
        // Timeout para locks (5 segundos)
        private readonly TimeSpan _lockTimeout = TimeSpan.FromSeconds(5);

        public ClienteConcurrencyService(IClienteRepository clienteRepository, SaunaDbContext context)
        {
            _clienteRepository = clienteRepository;
            _context = context;
        }

        /// <summary>
        /// Crea un cliente con control de concurrencia para evitar DNI duplicados
        /// </summary>
        public async Task<(bool exito, string mensaje, ClienteDTO? cliente)> CrearClienteConcurrenteAsync(ClienteDTO clienteDto)
        {
            var dni = clienteDto.numero_documento?.Trim();
            if (string.IsNullOrEmpty(dni))
            {
                return (false, "El número de documento es requerido.", null);
            }

            // 🔒 Lock por DNI para evitar creación simultánea
            if (!TryLockDNI(dni))
            {
                return (false, $"Otro usuario está registrando un cliente con DNI {dni}. Intente en unos segundos.", null);
            }

            try
            {
                // Usar transacción para garantizar consistencia
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    // Verificación final de DNI (por si cambió durante el lock)
                    if (await _clienteRepository.ExisteDNIAsync(dni))
                    {
                        await transaction.RollbackAsync();
                        return (false, "Ya existe un cliente con ese número de documento.", null);
                    }

                    var cliente = new Cliente
                    {
                        nombre = clienteDto.nombre.Trim(),
                        apellidos = clienteDto.apellidos.Trim(),
                        numero_documento = dni,
                        telefono = string.IsNullOrWhiteSpace(clienteDto.telefono) ? null : clienteDto.telefono.Trim(),
                        correo = string.IsNullOrWhiteSpace(clienteDto.correo) ? null : clienteDto.correo.Trim().ToLower(),
                        direccion = string.IsNullOrWhiteSpace(clienteDto.direccion) ? null : clienteDto.direccion.Trim(),
                        fechaNacimiento = clienteDto.fechaNacimiento,
                        fechaRegistro = DateTime.Now,
                        visitasTotales = 0,
                        activo = true
                    };

                    await _clienteRepository.AddAsync(cliente);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return (true, "Cliente registrado exitosamente.", MapToDTO(cliente));
                }
                catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
                {
                    await transaction.RollbackAsync();
                    return (false, "Ya existe un cliente con ese número de documento.", null);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return (false, $"Error al crear cliente: {ex.Message}", null);
                }
            }
            finally
            {
                ReleaseDNILock(dni);
            }
        }

        /// <summary>
        /// Actualiza un cliente con control de concurrencia optimista
        /// </summary>
        public async Task<(bool exito, string mensaje)> ActualizarClienteConcurrenteAsync(ClienteDTO clienteDto)
        {
            var clienteId = clienteDto.idCliente;
            
            // 🔒 Lock por cliente para evitar actualizaciones simultáneas
            if (!TryLockCliente(clienteId))
            {
                return (false, "Otro usuario está modificando este cliente. Intente en unos segundos.");
            }

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    // Obtener cliente actual con tracking para detectar cambios
                    var clienteActual = await _context.Cliente
                        .FirstOrDefaultAsync(c => c.idCliente == clienteId);

                    if (clienteActual == null)
                    {
                        await transaction.RollbackAsync();
                        return (false, "Cliente no encontrado.");
                    }

                    // Verificar DNI único (excluyendo el cliente actual)
                    var dni = clienteDto.numero_documento?.Trim();
                    if (!string.IsNullOrEmpty(dni) && 
                        await ExisteDNIEnOtroClienteAsync(dni, clienteId))
                    {
                        await transaction.RollbackAsync();
                        return (false, "Ya existe otro cliente con ese número de documento.");
                    }

                    // Actualizar solo campos modificados para mejor performance
                    bool hasChanges = false;

                    if (clienteActual.nombre != clienteDto.nombre?.Trim())
                    {
                        clienteActual.nombre = clienteDto.nombre.Trim();
                        hasChanges = true;
                    }

                    if (clienteActual.apellidos != clienteDto.apellidos?.Trim())
                    {
                        clienteActual.apellidos = clienteDto.apellidos.Trim();
                        hasChanges = true;
                    }

                    if (clienteActual.numero_documento != dni)
                    {
                        clienteActual.numero_documento = dni;
                        hasChanges = true;
                    }

                    var telefono = string.IsNullOrWhiteSpace(clienteDto.telefono) ? null : clienteDto.telefono.Trim();
                    if (clienteActual.telefono != telefono)
                    {
                        clienteActual.telefono = telefono;
                        hasChanges = true;
                    }

                    var correo = string.IsNullOrWhiteSpace(clienteDto.correo) ? null : clienteDto.correo.Trim().ToLower();
                    if (clienteActual.correo != correo)
                    {
                        clienteActual.correo = correo;
                        hasChanges = true;
                    }

                    var direccion = string.IsNullOrWhiteSpace(clienteDto.direccion) ? null : clienteDto.direccion.Trim();
                    if (clienteActual.direccion != direccion)
                    {
                        clienteActual.direccion = direccion;
                        hasChanges = true;
                    }

                    if (clienteActual.fechaNacimiento != clienteDto.fechaNacimiento)
                    {
                        clienteActual.fechaNacimiento = clienteDto.fechaNacimiento;
                        hasChanges = true;
                    }

                    if (!hasChanges)
                    {
                        await transaction.RollbackAsync();
                        return (true, "No hay cambios para actualizar.");
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return (true, "Cliente actualizado exitosamente.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    await transaction.RollbackAsync();
                    return (false, "El cliente fue modificado por otro usuario. Por favor, recargue los datos e intente nuevamente.");
                }
                catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
                {
                    await transaction.RollbackAsync();
                    return (false, "Ya existe otro cliente con ese número de documento.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return (false, $"Error al actualizar cliente: {ex.Message}");
                }
            }
            finally
            {
                ReleaseClienteLock(clienteId);
            }
        }

        /// <summary>
        /// Verifica si un DNI existe en otro cliente (excluyendo el cliente actual)
        /// </summary>
        private async Task<bool> ExisteDNIEnOtroClienteAsync(string dni, int clienteIdExcluir)
        {
            return await _context.Cliente
                .AnyAsync(c => c.numero_documento == dni && c.idCliente != clienteIdExcluir);
        }

        /// <summary>
        /// Intenta bloquear un DNI para creación
        /// </summary>
        private bool TryLockDNI(string dni)
        {
            var now = DateTime.Now;
            var lockExpiry = now.Add(_lockTimeout);

            // Limpiar locks expirados
            CleanExpiredDNILocks();

            // Intentar adquirir lock
            return _dniCreationLock.TryAdd(dni, lockExpiry);
        }

        /// <summary>
        /// Libera el lock de un DNI
        /// </summary>
        private void ReleaseDNILock(string dni)
        {
            _dniCreationLock.TryRemove(dni, out _);
        }

        /// <summary>
        /// Intenta bloquear un cliente para actualización
        /// </summary>
        private bool TryLockCliente(int clienteId)
        {
            var now = DateTime.Now;
            var lockExpiry = now.Add(_lockTimeout);

            // Limpiar locks expirados
            CleanExpiredClienteLocks();

            // Intentar adquirir lock
            return _clienteUpdateLock.TryAdd(clienteId, lockExpiry);
        }

        /// <summary>
        /// Libera el lock de un cliente
        /// </summary>
        private void ReleaseClienteLock(int clienteId)
        {
            _clienteUpdateLock.TryRemove(clienteId, out _);
        }

        /// <summary>
        /// Limpia locks expirados de DNI
        /// </summary>
        private void CleanExpiredDNILocks()
        {
            var now = DateTime.Now;
            var expiredKeys = _dniCreationLock
                .Where(kvp => kvp.Value < now)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _dniCreationLock.TryRemove(key, out _);
            }
        }

        /// <summary>
        /// Limpia locks expirados de clientes
        /// </summary>
        private void CleanExpiredClienteLocks()
        {
            var now = DateTime.Now;
            var expiredKeys = _clienteUpdateLock
                .Where(kvp => kvp.Value < now)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _clienteUpdateLock.TryRemove(key, out _);
            }
        }

        /// <summary>
        /// Verifica si una excepción es por violación de restricción única
        /// </summary>
        private bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            return ex.InnerException?.Message.Contains("UNIQUE constraint failed") == true ||
                   ex.InnerException?.Message.Contains("duplicate key") == true ||
                   ex.InnerException?.Message.Contains("Violation of UNIQUE KEY constraint") == true;
        }

        /// <summary>
        /// Mapea entidad a DTO
        /// </summary>
        private ClienteDTO MapToDTO(Cliente cliente)
        {
            return new ClienteDTO
            {
                idCliente = cliente.idCliente,
                nombre = cliente.nombre,
                apellidos = cliente.apellidos,
                numero_documento = cliente.numero_documento ?? string.Empty,
                telefono = cliente.telefono,
                correo = cliente.correo,
                direccion = cliente.direccion,
                fechaNacimiento = cliente.fechaNacimiento,
                fechaRegistro = cliente.fechaRegistro,
                visitasTotales = cliente.visitasTotales,
                activo = cliente.activo
            };
        }

        /// <summary>
        /// Obtiene estadísticas de concurrencia para monitoreo
        /// </summary>
        public ConcurrencyStats GetConcurrencyStats()
        {
            CleanExpiredDNILocks();
            CleanExpiredClienteLocks();

            return new ConcurrencyStats
            {
                ActiveDNILocks = _dniCreationLock.Count,
                ActiveClienteLocks = _clienteUpdateLock.Count,
                Timestamp = DateTime.Now
            };
        }
    }

    /// <summary>
    /// Estadísticas de concurrencia para monitoreo
    /// </summary>
    public class ConcurrencyStats
    {
        public int ActiveDNILocks { get; set; }
        public int ActiveClienteLocks { get; set; }
        public DateTime Timestamp { get; set; }
    }
}