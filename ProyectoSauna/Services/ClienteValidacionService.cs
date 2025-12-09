using Microsoft.EntityFrameworkCore;
using ProyectoSauna.Models;
using ProyectoSauna.Models.Entities;
using System;
using System.Threading.Tasks;

namespace ProyectoSauna.Services
{
    /// <summary>
    /// 🛡️ Servicio de validación para operaciones seguras con clientes
    /// Maneja validaciones de estado, concurrencia y reglas de negocio
    /// </summary>
    public class ClienteValidacionService
    {
        private readonly SaunaDbContext _context;

        public ClienteValidacionService(SaunaDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// ✅ Valida si un cliente puede ser modificado o eliminado
        /// </summary>
        public async Task<(bool esValida, string mensaje)> ValidarClienteParaModificacionAsync(int idCliente)
        {
            try
            {
                var cliente = await _context.Cliente
                    .Include(c => c.Cuenta.Where(cuenta => cuenta.idEstadoCuenta == 1)) // Solo cuentas pendientes
                    .FirstOrDefaultAsync(c => c.idCliente == idCliente);

                if (cliente == null)
                {
                    return (false, "❌ El cliente no existe o fue eliminado");
                }

                if (!cliente.activo)
                {
                    return (false, "❌ El cliente está inactivo y no puede ser modificado");
                }

                // Verificar si tiene cuentas pendientes (estado activo)
                var cuentasPendientes = cliente.Cuenta.Count(c => c.idEstadoCuenta == 1);
                if (cuentasPendientes > 0)
                {
                    return (false, 
                        $"❌ CLIENTE CON CUENTAS ACTIVAS\n\n" +
                        $"El cliente tiene {cuentasPendientes} cuenta(s) pendiente(s).\n" +
                        $"No se puede modificar o eliminar hasta que se cierren todas las cuentas.");
                }

                return (true, "✅ Cliente disponible para modificación");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error validando cliente: {ex.Message}");
                return (false, $"❌ Error de validación: {ex.Message}");
            }
        }

        /// <summary>
        /// 🔍 Valida unicidad del documento de identidad
        /// </summary>
        public async Task<(bool esValido, string mensaje)> ValidarDocumentoUnicoAsync(string numeroDocumento, int? idClienteExcluir = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(numeroDocumento))
                {
                    return (false, "❌ El número de documento es obligatorio");
                }

                var documentoNormalizado = numeroDocumento.Trim().ToUpper();

                var query = _context.Cliente.Where(c => c.numero_documento == documentoNormalizado && c.activo);
                
                if (idClienteExcluir.HasValue)
                {
                    query = query.Where(c => c.idCliente != idClienteExcluir.Value);
                }

                var clienteExistente = await query.FirstOrDefaultAsync();

                if (clienteExistente != null)
                {
                    return (false, 
                        $"❌ DOCUMENTO DUPLICADO\n\n" +
                        $"Ya existe un cliente con el documento '{numeroDocumento}':\n" +
                        $"• Nombre: {clienteExistente.nombre} {clienteExistente.apellidos}\n" +
                        $"• ID: {clienteExistente.idCliente}");
                }

                return (true, "✅ Documento disponible");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error validando documento: {ex.Message}");
                return (false, $"❌ Error de validación: {ex.Message}");
            }
        }

        /// <summary>
        /// 📧 Valida unicidad del correo electrónico
        /// </summary>
        public async Task<(bool esValido, string mensaje)> ValidarCorreoUnicoAsync(string? correo, int? idClienteExcluir = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(correo))
                {
                    return (true, "✅ Correo válido (opcional)");
                }

                var correoNormalizado = correo.Trim().ToLower();

                // Validación básica de formato de correo
                if (!correoNormalizado.Contains("@") || !correoNormalizado.Contains("."))
                {
                    return (false, "❌ Formato de correo electrónico inválido");
                }

                var query = _context.Cliente.Where(c => c.correo == correoNormalizado && c.activo);
                
                if (idClienteExcluir.HasValue)
                {
                    query = query.Where(c => c.idCliente != idClienteExcluir.Value);
                }

                var clienteExistente = await query.FirstOrDefaultAsync();

                if (clienteExistente != null)
                {
                    return (false, 
                        $"❌ CORREO DUPLICADO\n\n" +
                        $"Ya existe un cliente con el correo '{correo}':\n" +
                        $"• Nombre: {clienteExistente.nombre} {clienteExistente.apellidos}\n" +
                        $"• Documento: {clienteExistente.numero_documento}");
                }

                return (true, "✅ Correo disponible");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error validando correo: {ex.Message}");
                return (false, $"❌ Error de validación: {ex.Message}");
            }
        }

        /// <summary>
        /// 🔍 Obtiene información del cliente para verificación
        /// </summary>
        public async Task<Cliente?> ObtenerClienteParaValidacionAsync(int idCliente)
        {
            try
            {
                return await _context.Cliente
                    .Include(c => c.Cuenta)
                    .FirstOrDefaultAsync(c => c.idCliente == idCliente);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error obteniendo cliente: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 📊 Validación completa de datos de cliente para creación/edición
        /// </summary>
        public async Task<(bool esValido, string mensaje)> ValidarDatosCompletoAsync(
            string nombre, 
            string apellidos, 
            string numeroDocumento, 
            string? correo = null, 
            int? idClienteExcluir = null)
        {
            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(nombre))
                return (false, "❌ El nombre es obligatorio");

            if (string.IsNullOrWhiteSpace(apellidos))
                return (false, "❌ Los apellidos son obligatorios");

            if (string.IsNullOrWhiteSpace(numeroDocumento))
                return (false, "❌ El número de documento es obligatorio");

            if (numeroDocumento.Trim().Length < 8)
                return (false, "❌ El número de documento debe tener al menos 8 caracteres");

            // Validación de documento único
            var validacionDocumento = await ValidarDocumentoUnicoAsync(numeroDocumento, idClienteExcluir);
            if (!validacionDocumento.esValido)
                return validacionDocumento;

            // Validación de correo si se proporciona
            if (!string.IsNullOrWhiteSpace(correo))
            {
                var validacionCorreo = await ValidarCorreoUnicoAsync(correo, idClienteExcluir);
                if (!validacionCorreo.esValido)
                    return validacionCorreo;
            }

            return (true, "✅ Todos los datos son válidos");
        }
    }
}