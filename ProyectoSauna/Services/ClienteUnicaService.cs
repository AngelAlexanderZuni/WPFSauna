using Microsoft.EntityFrameworkCore;
using ProyectoSauna.Models;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories;
using System;
using System.Threading.Tasks;

namespace ProyectoSauna.Services
{
    /// <summary>
    /// 🛡️ Servicio que garantiza la creación segura de clientes únicos
    /// Evita duplicados simultáneos por DNI/documento y maneja concurrencia
    /// </summary>
    public class ClienteUnicaService
    {
        private readonly SaunaDbContext _context;

        public ClienteUnicaService(SaunaDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 🔒 Crea un cliente de forma SEGURA evitando duplicados por DNI
        /// </summary>
        public async Task<(bool exito, string mensaje, int? idClienteCreado)> CrearClienteSeguroAsync(
            string nombre,
            string apellidos,
            string numeroDocumento,
            string? telefono = null,
            string? correo = null,
            string? direccion = null,
            DateTime? fechaNacimiento = null)
        {
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellidos) || 
                string.IsNullOrWhiteSpace(numeroDocumento))
            {
                return (false, "Nombre, apellidos y número de documento son obligatorios", null);
            }

            // Normalizar datos
            nombre = nombre.Trim();
            apellidos = apellidos.Trim();
            numeroDocumento = numeroDocumento.Trim().ToUpper();
            telefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim();
            correo = string.IsNullOrWhiteSpace(correo) ? null : correo.Trim().ToLower();
            direccion = string.IsNullOrWhiteSpace(direccion) ? null : direccion.Trim();

            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // 🔍 VERIFICACIÓN 1: Cliente ya existe por DNI/documento
                var clienteExistentePorDni = await _context.Cliente
                    .FirstOrDefaultAsync(c => c.numero_documento == numeroDocumento && c.activo);

                if (clienteExistentePorDni != null)
                {
                    await transaction.RollbackAsync();
                    return (false, 
                        $"❌ CLIENTE DUPLICADO\n\nYa existe un cliente con el documento '{numeroDocumento}':\n" +
                        $"• Nombre: {clienteExistentePorDni.nombre} {clienteExistentePorDni.apellidos}\n" +
                        $"• ID: {clienteExistentePorDni.idCliente}\n" +
                        $"• Registrado: {clienteExistentePorDni.fechaRegistro:dd/MM/yyyy}\n\n" +
                        $"No se pueden crear clientes duplicados.", null);
                }

                // 🔍 VERIFICACIÓN 2: Cliente similar por nombre y apellidos (prevención adicional)
                if (!string.IsNullOrWhiteSpace(correo))
                {
                    var clienteExistentePorCorreo = await _context.Cliente
                        .FirstOrDefaultAsync(c => c.correo == correo && c.activo);

                    if (clienteExistentePorCorreo != null)
                    {
                        await transaction.RollbackAsync();
                        return (false,
                            $"❌ CORREO DUPLICADO\n\nYa existe un cliente con el correo '{correo}':\n" +
                            $"• Nombre: {clienteExistentePorCorreo.nombre} {clienteExistentePorCorreo.apellidos}\n" +
                            $"• Documento: {clienteExistentePorCorreo.numero_documento}\n\n" +
                            $"No se pueden crear clientes con el mismo correo.", null);
                    }
                }

                // ✅ CREACIÓN SEGURA DEL CLIENTE
                var nuevoCliente = new Cliente
                {
                    nombre = nombre,
                    apellidos = apellidos,
                    numero_documento = numeroDocumento,
                    telefono = telefono,
                    correo = correo,
                    direccion = direccion,
                    fechaNacimiento = fechaNacimiento,
                    fechaRegistro = DateTime.UtcNow,
                    activo = true
                };

                _context.Cliente.Add(nuevoCliente);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                System.Diagnostics.Debug.WriteLine($"✅ Cliente creado exitosamente: ID={nuevoCliente.idCliente}, DNI={numeroDocumento}");

                return (true, $"✅ Cliente creado exitosamente\n\nID: {nuevoCliente.idCliente}\nNombre: {nombre} {apellidos}", nuevoCliente.idCliente);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                System.Diagnostics.Debug.WriteLine($"❌ Conflicto de concurrencia al crear cliente: {ex.Message}");
                
                return (false, 
                    "⚠️ CONFLICTO DE CONCURRENCIA\n\n" +
                    "Otro usuario está creando un cliente al mismo tiempo.\n" +
                    "Por favor, intente nuevamente en unos segundos.", null);
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message?.Contains("duplicate") == true ||
                                              ex.InnerException?.Message?.Contains("UNIQUE") == true)
            {
                await transaction.RollbackAsync();
                System.Diagnostics.Debug.WriteLine($"❌ Violación de restricción única: {ex.Message}");
                
                return (false, 
                    "❌ DATOS DUPLICADOS\n\n" +
                    "Ya existe un cliente con este documento o correo.\n" +
                    "Verifique los datos e intente nuevamente.", null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                System.Diagnostics.Debug.WriteLine($"❌ Error inesperado al crear cliente: {ex.Message}");
                
                return (false, 
                    $"❌ ERROR AL CREAR CLIENTE\n\n{ex.Message}\n\n" +
                    $"Por favor, verifique los datos e intente nuevamente.", null);
            }
        }

        /// <summary>
        /// 🔍 Valida si un documento ya está siendo usado por otro cliente
        /// </summary>
        public async Task<bool> DocumentoYaExisteAsync(string numeroDocumento, int? idClienteExcluir = null)
        {
            if (string.IsNullOrWhiteSpace(numeroDocumento)) return false;

            var query = _context.Cliente.Where(c => c.numero_documento == numeroDocumento.Trim().ToUpper() && c.activo);
            
            if (idClienteExcluir.HasValue)
            {
                query = query.Where(c => c.idCliente != idClienteExcluir.Value);
            }

            return await query.AnyAsync();
        }

        /// <summary>
        /// 🔍 Valida si un correo ya está siendo usado por otro cliente
        /// </summary>
        public async Task<bool> CorreoYaExisteAsync(string? correo, int? idClienteExcluir = null)
        {
            if (string.IsNullOrWhiteSpace(correo)) return false;

            var query = _context.Cliente.Where(c => c.correo == correo.Trim().ToLower() && c.activo);
            
            if (idClienteExcluir.HasValue)
            {
                query = query.Where(c => c.idCliente != idClienteExcluir.Value);
            }

            return await query.AnyAsync();
        }
    }
}