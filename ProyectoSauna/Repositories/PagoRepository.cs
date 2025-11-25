using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProyectoSauna.Models;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories.Base;
using ProyectoSauna.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories
{
    public class PagoRepository : Repository<Pago>, IPagoRepository
    {
        private readonly SaunaDbContext _context;
        public PagoRepository(SaunaDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Pago> CrearPagoAsync(Pago pago)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Pago.AddAsync(pago);
                await _context.SaveChangesAsync();

                // Actualizar montos de la cuenta
                var cuenta = await _context.Cuenta.FirstAsync(c => c.idCuenta == pago.idCuenta);
                cuenta.montoPagado = await _context.Pago.Where(p => p.idCuenta == pago.idCuenta)
                    .SumAsync(p => (decimal?)p.monto) ?? 0m;
                cuenta.saldo = cuenta.total - cuenta.montoPagado;
                _context.Cuenta.Update(cuenta);
                await _context.SaveChangesAsync();

                await tx.CommitAsync();
                return pago;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<Pago>> GetPorCuentaAsync(int idCuenta)
        {
            return await _context.Pago
                .Include(p => p.idMetodoPagoNavigation)
                .Where(p => p.idCuenta == idCuenta)
                .OrderByDescending(p => p.fechaHora)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pago>> GetRecientesAsync(int count = 20)
        {
            return await _context.Pago
                .Include(p => p.idMetodoPagoNavigation)
                .Include(p => p.idCuentaNavigation)
                .OrderByDescending(p => p.fechaHora)
                .Take(count)
                .ToListAsync();
        }
    }
}
