using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProyectoSauna.Models;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories.Interfaces;

namespace ProyectoSauna.Repositories
{
    public class CuentaRepository : ICuentaRepository
    {
        public async Task<List<Cuenta>> GetCuentasPendientesAsync()
        {
            using var context = new SaunaDbContext();
            return await context.Cuenta
                .AsNoTracking()
                .Where(c => c.idEstadoCuenta == 1)
                .OrderByDescending(c => c.fechaHoraCreacion)
                .ToListAsync();
        }

        public async Task<Cuenta?> GetCuentaActivaDeClienteAsync(int idCliente)
        {
            using var context = new SaunaDbContext();
            return await context.Cuenta
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.idCliente == idCliente && c.idEstadoCuenta == 1);
        }

        public async Task<int> CrearCuentaAsync(Cuenta cuenta)
        {
            using var context = new SaunaDbContext();
            context.Cuenta.Add(cuenta);
            await context.SaveChangesAsync();
            return cuenta.idCuenta;
        }

        public async Task ActualizarCuentaAsync(Cuenta cuenta)
        {
            using var context = new SaunaDbContext();
            context.Cuenta.Update(cuenta);
            await context.SaveChangesAsync();
        }

        public async Task<Cuenta?> GetCuentaByIdAsync(int idCuenta)
        {
            using var context = new SaunaDbContext();
            return await context.Cuenta
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.idCuenta == idCuenta);
        }
    }
}
