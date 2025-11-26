// Services/ClienteService.cs - COMPLETAMENTE CORREGIDO
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
                transaction = await _context.Database.BeginTransactionAsync();

                var validacion = ValidarCliente(clienteDto);
                if (!validacion.valido)
                {
                    await transaction.RollbackAsync();
                    return (false, validacion.mensaje, null);
                }

                if (await _clienteRepository.ExisteDNIAsync(clienteDto.numero_documento))
                {
                    await transaction.RollbackAsync();
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
                await transaction.CommitAsync();

                return (true, "Cliente registrado exitosamente.", MapToDTO(cliente));
            }
            catch (DbUpdateException dbEx)
            {
                if (transaction != null)
                    await transaction.RollbackAsync();

                return (false, $"Error al guardar en la base de datos: {dbEx.InnerException?.Message ?? dbEx.Message}", null);
            }
            catch (Exception ex)
            {
                if (transaction != null)
                    await transaction.RollbackAsync();

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
                transaction = await _context.Database.BeginTransactionAsync();

                var validacion = ValidarCliente(clienteDto);
                if (!validacion.valido)
                {
                    await transaction.RollbackAsync();
                    return (false, validacion.mensaje);
                }

                var clienteExistente = await _clienteRepository.GetByIdAsync(clienteDto.idCliente);
                if (clienteExistente == null)
                {
                    await transaction.RollbackAsync();
                    return (false, "Cliente no encontrado.");
                }

                if (await _clienteRepository.ExisteDNIAsync(clienteDto.numero_documento, clienteDto.idCliente))
                {
                    await transaction.RollbackAsync();
                    return (false, "Ya existe otro cliente con ese número de documento.");
                }

                clienteExistente.nombre = clienteDto.nombre.Trim();
                clienteExistente.apellidos = clienteDto.apellidos.Trim();
                clienteExistente.numero_documento = clienteDto.numero_documento.Trim();
                clienteExistente.telefono = string.IsNullOrWhiteSpace(clienteDto.telefono) ? null : clienteDto.telefono.Trim();
                clienteExistente.correo = string.IsNullOrWhiteSpace(clienteDto.correo) ? null : clienteDto.correo.Trim().ToLower();
                clienteExistente.direccion = string.IsNullOrWhiteSpace(clienteDto.direccion) ? null : clienteDto.direccion.Trim();
                clienteExistente.fechaNacimiento = clienteDto.fechaNacimiento;

                await _clienteRepository.UpdateAsync(clienteExistente);
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
    }
}