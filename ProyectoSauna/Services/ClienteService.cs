using ProyectoSauna.Models.DTOs;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories.Interfaces;
using ProyectoSauna.Services.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
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
        private readonly DbContext _context;

        public ClienteService(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
            // Obtener el contexto desde el repositorio para transacciones
            _context = ((dynamic)_clienteRepository).GetContext();
        }

        public async Task<List<ClienteDTO>> GetAllClientesAsync()
        {
            var clientes = await _clienteRepository.GetAllAsync();
            return clientes.Select(MapToDTO).ToList();
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

        public async Task<List<ClienteDTO>> GetClientesActivosAsync()
        {
            var clientes = await _clienteRepository.GetClientesActivosAsync();
            return clientes.Select(MapToDTO).ToList();
        }

        public async Task<(bool exito, string mensaje, ClienteDTO? cliente)> CrearClienteAsync(ClienteDTO clienteDto)
        {
            IDbContextTransaction? transaction = null;
            try
            {
                // Iniciar transacci├│n
                transaction = await _context.Database.BeginTransactionAsync();

                // Validaciones
                var validacion = ValidarCliente(clienteDto);
                if (!validacion.valido)
                {
                    await transaction.RollbackAsync();
                    return (false, validacion.mensaje, null);
                }

                // Verificar DNI ├║nico
                if (await _clienteRepository.ExisteDNIAsync(clienteDto.numero_documento))
                {
                    await transaction.RollbackAsync();
                    return (false, "Ya existe un cliente con ese n├║mero de documento.", null);
                }

                // Crear entidad
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
                    activo = true,
                    idPrograma = 1 // Todos los clientes nuevos se inscriben autom├íticamente al programa de fidelizaci├│n
                };

                await _clienteRepository.AddAsync(cliente);
                
                // Confirmar transacci├│n
                await transaction.CommitAsync();

                return (true, "Cliente registrado exitosamente.", MapToDTO(cliente));
            }
            catch (DbUpdateException dbEx)
            {
                if (transaction != null)
                    await transaction.RollbackAsync();
                
                // Error espec├¡fico de base de datos
                return (false, $"Error al guardar en la base de datos: {dbEx.InnerException?.Message ?? dbEx.Message}", null);
            }
            catch (Exception ex)
            {
                if (transaction != null)
                    await transaction.RollbackAsync();
                
                // Error general
                return (false, $"Error inesperado: {ex.Message}", null);
            }
            finally
            {
                transaction?.Dispose();
            }
        }

        public async Task<(bool exito, string mensaje)> ActualizarClienteAsync(ClienteDTO clienteDto)
        {
            IDbContextTransaction? transaction = null;
            try
            {
                // Iniciar transacci├│n
                transaction = await _context.Database.BeginTransactionAsync();

                // Validaciones
                var validacion = ValidarCliente(clienteDto);
                if (!validacion.valido)
                {
                    await transaction.RollbackAsync();
                    return (false, validacion.mensaje);
                }

                // Verificar que el cliente existe
                var clienteExistente = await _clienteRepository.GetByIdAsync(clienteDto.idCliente);
                if (clienteExistente == null)
                {
                    await transaction.RollbackAsync();
                    return (false, "Cliente no encontrado.");
                }

                // Verificar DNI ├║nico (excluyendo el cliente actual)
                if (await _clienteRepository.ExisteDNIAsync(clienteDto.numero_documento, clienteDto.idCliente))
                {
                    await transaction.RollbackAsync();
                    return (false, "Ya existe otro cliente con ese n├║mero de documento.");
                }

                // Actualizar datos
                clienteExistente.nombre = clienteDto.nombre.Trim();
                clienteExistente.apellidos = clienteDto.apellidos.Trim();
                clienteExistente.numero_documento = clienteDto.numero_documento.Trim();
                clienteExistente.telefono = string.IsNullOrWhiteSpace(clienteDto.telefono) ? null : clienteDto.telefono.Trim();
                clienteExistente.correo = string.IsNullOrWhiteSpace(clienteDto.correo) ? null : clienteDto.correo.Trim().ToLower();
                clienteExistente.direccion = string.IsNullOrWhiteSpace(clienteDto.direccion) ? null : clienteDto.direccion.Trim();
                clienteExistente.fechaNacimiento = clienteDto.fechaNacimiento;

                await _clienteRepository.UpdateAsync(clienteExistente);
                
                // Confirmar transacci├│n
                await transaction.CommitAsync();

                return (true, "Cliente actualizado exitosamente.");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (transaction != null)
                    await transaction.RollbackAsync();
                
                return (false, "El cliente fue modificado por otro usuario. Por favor, recargue los datos.");
            }
            catch (DbUpdateException dbEx)
            {
                if (transaction != null)
                    await transaction.RollbackAsync();
                
                return (false, $"Error al actualizar en la base de datos: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                if (transaction != null)
                    await transaction.RollbackAsync();
                
                return (false, $"Error inesperado: {ex.Message}");
            }
            finally
            {
                transaction?.Dispose();
            }
        }

        public async Task<(bool exito, string mensaje)> DesactivarClienteAsync(int id)
        {
            var cliente = await _clienteRepository.GetByIdAsync(id);
            if (cliente == null)
            {
                return (false, "Cliente no encontrado.");
            }

            cliente.activo = false;
            await _clienteRepository.UpdateAsync(cliente);

            return (true, "Cliente desactivado exitosamente.");
        }

        public async Task<bool> ValidarDNIAsync(string dni, int? idClienteExcluir = null)
        {
            return !await _clienteRepository.ExisteDNIAsync(dni, idClienteExcluir);
        }

        // M├®todos privados de validaci├│n
        private (bool valido, string mensaje) ValidarCliente(ClienteDTO cliente)
        {
            // Validar campos obligatorios
            if (string.IsNullOrWhiteSpace(cliente.nombre))
                return (false, "El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(cliente.apellidos))
                return (false, "Los apellidos son obligatorios.");

            if (string.IsNullOrWhiteSpace(cliente.numero_documento))
                return (false, "El n├║mero de documento es obligatorio.");

            // Validar DNI (8 d├¡gitos)
            if (!Regex.IsMatch(cliente.numero_documento, @"^\d{8}$"))
                return (false, "El DNI debe tener exactamente 8 d├¡gitos.");

            // Validar tel├®fono (9 d├¡gitos si se proporciona)
            if (!string.IsNullOrWhiteSpace(cliente.telefono))
            {
                if (!Regex.IsMatch(cliente.telefono, @"^\d{9}$"))
                    return (false, "El tel├®fono debe tener 9 d├¡gitos.");
            }

            // Validar correo electr├│nico
            if (!string.IsNullOrWhiteSpace(cliente.correo))
            {
                var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                if (!Regex.IsMatch(cliente.correo, emailPattern))
                    return (false, "El correo electr├│nico no es v├ílido.");
            }

            // Validar fecha de nacimiento (debe ser menor a hoy, mayor de 10 a├▒os)
            if (cliente.fechaNacimiento.HasValue)
            {
                var hoy = DateOnly.FromDateTime(DateTime.Today);
                if (cliente.fechaNacimiento.Value >= hoy)
                    return (false, "La fecha de nacimiento debe ser anterior a hoy.");

                int edad = hoy.Year - cliente.fechaNacimiento.Value.Year;
                if (cliente.fechaNacimiento.Value > hoy.AddYears(-edad)) edad--;

                if (edad < 10)
                    return (false, "El cliente debe tener al menos 10 a├▒os.");
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
                activo = cliente.activo,
                idPrograma = cliente.idPrograma,
                ProgramaFidelizacion = cliente.idPrograma.HasValue && cliente.idProgramaNavigation != null
                    ? $"Inscrito - {cliente.idProgramaNavigation.porcentajeDescuento}% descuento en {cliente.idProgramaNavigation.visitasParaDescuento} visitas" 
                    : "Sin programa"
            };
        }
    }
}
