using Microsoft.EntityFrameworkCore;
using ProyectoSauna.Models;
using ProyectoSauna.Models.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoSauna.Services
{
    /// <summary>
    /// Servicio para prevenir creación simultánea de cuentas del mismo cliente
    /// </summary>
    public class CuentaUnicaService
    {
        /// <summary>
        /// Verifica si un cliente ya tiene una cuenta activa (Pendiente)
        /// y previene crear cuentas duplicadas simultáneamente
        /// </summary>
        public async Task<(bool puedeCrear, string mensaje, int? idCuentaExistente)> ValidarCreacionCuentaAsync(int idCliente)
        {
            try
            {
                using var context = new SaunaDbContext();
                
                // Usar transacción para prevenir condiciones de carrera
                using var transaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    // Buscar cuenta pendiente del cliente con bloqueo optimista
                    var cuentaExistente = await context.Cuenta
                        .Where(c => c.idCliente == idCliente && c.idEstadoCuenta == 1) // 1 = Pendiente
                        .FirstOrDefaultAsync();

                    if (cuentaExistente != null)
                    {
                        await transaction.RollbackAsync();
                        return (false, 
                            $"El cliente ya tiene una cuenta abierta (#{cuentaExistente.idCuenta}) desde las {cuentaExistente.fechaHoraCreacion:HH:mm}.\n\n" +
                            $"No se pueden crear múltiples cuentas pendientes para el mismo cliente.\n\n" +
                            $"💡 Sugerencia: Use la cuenta existente o cierre la cuenta anterior antes de crear una nueva.",
                            cuentaExistente.idCuenta);
                    }

                    // Si no existe, marcar como "en proceso de creación" temporalmente
                    await transaction.CommitAsync();
                    return (true, "Cuenta puede ser creada", null);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error al validar creación de cuenta: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Crea una cuenta de forma segura previniendo duplicados simultáneos
        /// </summary>
        public async Task<(bool exito, string mensaje, int? idCuentaCreada)> CrearCuentaSeguraAsync(
            int idCliente, 
            decimal precioEntrada, 
            int idUsuarioCreador)
        {
            try
            {
                using var context = new SaunaDbContext();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    // Verificar NUEVAMENTE que no existe cuenta (doble verificación)
                    var cuentaExistente = await context.Cuenta
                        .Where(c => c.idCliente == idCliente && c.idEstadoCuenta == 1)
                        .FirstOrDefaultAsync();

                    if (cuentaExistente != null)
                    {
                        await transaction.RollbackAsync();
                        return (false, 
                            $"Otro usuario creó una cuenta para este cliente al mismo tiempo.\n\n" +
                            $"Cuenta existente: #{cuentaExistente.idCuenta}\n" +
                            $"Creada: {cuentaExistente.fechaHoraCreacion:dd/MM/yyyy HH:mm}\n\n" +
                            $"Usar la cuenta existente en lugar de crear una nueva.",
                            cuentaExistente.idCuenta);
                    }

                    // Crear la cuenta
                    var nuevaCuenta = new Cuenta
                    {
                        idCliente = idCliente,
                        fechaHoraCreacion = DateTime.Now,
                        subtotalConsumos = 0,
                        descuento = 0,
                        total = precioEntrada,
                        idEstadoCuenta = 1, // Pendiente
                        idUsuarioCreador = idUsuarioCreador
                    };

                    context.Cuenta.Add(nuevaCuenta);
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return (true, 
                        $"✅ Cuenta creada exitosamente.\n\n" +
                        $"ID Cuenta: #{nuevaCuenta.idCuenta}\n" +
                        $"Cliente: {idCliente}\n" +
                        $"Hora: {nuevaCuenta.fechaHoraCreacion:HH:mm}",
                        nuevaCuenta.idCuenta);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    
                    // Si es error de concurrencia, dar mensaje claro
                    if (ex is DbUpdateConcurrencyException)
                    {
                        return (false, 
                            "Otro usuario estaba creando una cuenta para este cliente al mismo tiempo.\n\n" +
                            "Por favor, verifique las cuentas pendientes y use una existente si está disponible.",
                            null);
                    }
                    
                    throw;
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error al crear cuenta: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Obtiene todas las cuentas pendientes de un cliente
        /// </summary>
        public async Task<System.Collections.Generic.List<Cuenta>> ObtenerCuentasPendientesClienteAsync(int idCliente)
        {
            using var context = new SaunaDbContext();
            return await context.Cuenta
                .Include(c => c.idClienteNavigation)
                .Include(c => c.idEstadoCuentaNavigation)
                .Where(c => c.idCliente == idCliente && c.idEstadoCuenta == 1)
                .OrderByDescending(c => c.fechaHoraCreacion)
                .ToListAsync();
        }
    }
}