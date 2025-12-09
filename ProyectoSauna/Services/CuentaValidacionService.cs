using Microsoft.EntityFrameworkCore;
using ProyectoSauna.Models;
using ProyectoSauna.Models.Entities;
using System;
using System.Threading.Tasks;

namespace ProyectoSauna.Services
{
    /// <summary>
    /// Servicio para validaciones específicas de cuentas y operaciones seguras
    /// </summary>
    public class CuentaValidacionService
    {
        /// <summary>
        /// Verifica si una cuenta está en estado válido para modificaciones
        /// </summary>
        public async Task<(bool esValida, string mensaje)> ValidarCuentaParaModificacionAsync(int idCuenta)
        {
            try
            {
                using var context = new SaunaDbContext();
                var cuenta = await context.Cuenta
                    .Include(c => c.idEstadoCuentaNavigation)
                    .FirstOrDefaultAsync(c => c.idCuenta == idCuenta);

                if (cuenta == null)
                    return (false, "La cuenta no existe en el sistema.");

                if (cuenta.idEstadoCuenta != 1) // 1 = Pendiente
                {
                    var estadoNombre = cuenta.idEstadoCuentaNavigation?.nombre ?? "Desconocido";
                    return (false, $"No se puede modificar una cuenta en estado '{estadoNombre}'. Solo las cuentas pendientes pueden ser modificadas.");
                }

                return (true, "Cuenta válida para modificación.");
            }
            catch (Exception ex)
            {
                return (false, $"Error al validar cuenta: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica si un producto tiene stock suficiente para agregar a cuenta
        /// </summary>
        public async Task<(bool hayStock, string mensaje, int stockDisponible)> ValidarStockProductoAsync(int idProducto, int cantidadSolicitada)
        {
            try
            {
                using var context = new SaunaDbContext();
                var producto = await context.Producto.FindAsync(idProducto);

                if (producto == null)
                    return (false, "El producto no existe en el sistema.", 0);

                if (!producto.activo)
                    return (false, "El producto no está activo en el sistema.", producto.stockActual);

                if (producto.stockActual < cantidadSolicitada)
                    return (false, $"Stock insuficiente. Disponible: {producto.stockActual}, Solicitado: {cantidadSolicitada}", producto.stockActual);

                return (true, "Stock disponible.", producto.stockActual);
            }
            catch (Exception ex)
            {
                return (false, $"Error al validar stock: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// Valida si un cliente está activo y puede tener cuentas
        /// </summary>
        public async Task<(bool esValido, string mensaje)> ValidarClienteActivoAsync(int idCliente)
        {
            try
            {
                using var context = new SaunaDbContext();
                var cliente = await context.Cliente.FindAsync(idCliente);

                if (cliente == null)
                    return (false, "El cliente no existe en el sistema.");

                if (!cliente.activo)
                    return (false, "El cliente está desactivado en el sistema. Contacte al administrador.");

                return (true, "Cliente válido y activo.");
            }
            catch (Exception ex)
            {
                return (false, $"Error al validar cliente: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica si una cuenta tiene consumos antes de eliminar
        /// </summary>
        public async Task<(bool tieneConsumos, string mensaje, int cantidadConsumos)> ValidarCuentaTieneConsumosAsync(int idCuenta)
        {
            try
            {
                using var context = new SaunaDbContext();
                
                var cantidadProductos = await context.DetalleConsumo
                    .CountAsync(dc => dc.idCuenta == idCuenta);
                
                var cantidadServicios = await context.DetalleServicio
                    .CountAsync(ds => ds.idCuenta == idCuenta);

                var totalConsumos = cantidadProductos + cantidadServicios;

                if (totalConsumos > 0)
                {
                    return (true, $"La cuenta tiene {totalConsumos} consumo(s). Se recomienda procesar el pago en lugar de eliminar.", totalConsumos);
                }

                return (false, "La cuenta no tiene consumos, puede ser eliminada de forma segura.", 0);
            }
            catch (Exception ex)
            {
                return (false, $"Error al verificar consumos: {ex.Message}", 0);
            }
        }
    }
}