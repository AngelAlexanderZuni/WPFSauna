// Repositories/CuentaRepository.cs - COMPLETAMENTE CORREGIDO
using Microsoft.EntityFrameworkCore;
using ProyectoSauna.Models;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories
{
    public class CuentaRepository : ICuentaRepository
    {
        public async Task<Cliente?> ObtenerClienteConProgramaAsync(int idCliente)
        {
            using var context = new SaunaDbContext();
            return await context.Cliente
                .FirstOrDefaultAsync(c => c.idCliente == idCliente && c.activo);
        }

        public async Task<List<Cuenta>> GetCuentasPendientesAsync()
        {
            using var context = new SaunaDbContext();
            return await context.Cuenta
                .Include(c => c.idClienteNavigation)
                .Include(c => c.idEstadoCuentaNavigation)
                .Where(c => c.idEstadoCuenta == 1)
                .OrderByDescending(c => c.fechaHoraCreacion)
                .ToListAsync();
        }

        public async Task<Cuenta> GetCuentaByIdAsync(int idCuenta)
        {
            using var context = new SaunaDbContext();
            return await context.Cuenta
                .Include(c => c.idClienteNavigation)
                .Include(c => c.idEstadoCuentaNavigation)
                .FirstOrDefaultAsync(c => c.idCuenta == idCuenta);
        }

        public async Task<int> CrearCuentaAsync(Cuenta cuenta)
        {
            using var context = new SaunaDbContext();
            await context.Cuenta.AddAsync(cuenta);
            await context.SaveChangesAsync();
            return cuenta.idCuenta;
        }

        public async Task ActualizarCuentaAsync(Cuenta cuenta)
        {
            using var context = new SaunaDbContext();
            var cuentaExistente = await context.Cuenta.FindAsync(cuenta.idCuenta);
            if (cuentaExistente != null)
            {
                cuentaExistente.idCliente = cuenta.idCliente;
                cuentaExistente.fechaHoraSalida = cuenta.fechaHoraSalida;
                cuentaExistente.subtotalConsumos = cuenta.subtotalConsumos;
                cuentaExistente.precioEntrada = cuenta.precioEntrada;
                cuentaExistente.descuento = cuenta.descuento;
                cuentaExistente.total = cuenta.total;
                cuentaExistente.saldo = cuenta.saldo;
                cuentaExistente.idEstadoCuenta = cuenta.idEstadoCuenta;
                await context.SaveChangesAsync();
            }
        }

        public async Task ActualizarTotalCuentaAsync(int idCuenta)
        {
            using var context = new SaunaDbContext();
            var totalProductos = await context.DetalleConsumo
                .Where(dc => dc.idCuenta == idCuenta)
                .SumAsync(dc => (decimal?)dc.subtotal) ?? 0;

            var totalServicios = await context.DetalleServicio
                .Where(ds => ds.idCuenta == idCuenta)
                .SumAsync(ds => (decimal?)ds.subtotal) ?? 0;

            var cuenta = await context.Cuenta.FindAsync(idCuenta);
            if (cuenta != null)
            {
                cuenta.subtotalConsumos = totalProductos + totalServicios;
                cuenta.total = cuenta.precioEntrada + cuenta.subtotalConsumos - cuenta.descuento;
                await context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int idCuenta)
        {
            using var context = new SaunaDbContext();
            var cuenta = await context.Cuenta.FindAsync(idCuenta);
            if (cuenta != null)
            {
                context.Cuenta.Remove(cuenta);
                await context.SaveChangesAsync();
            }
        }
    }
}