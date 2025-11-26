using ProyectoSauna.Models;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories.Base;
using ProyectoSauna.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ProyectoSauna.Repositories
{
    public class PagoRepository : Repository<Pago>, IPagoRepository
    {
        public PagoRepository(SaunaDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Pago>> ObtenerConNavegacionAsync()
        {
            // Evitamos Include a Cuenta/MetodoPago para no provocar lectura de columnas inexistentes
            return await _context.Pago.AsNoTracking().ToListAsync();
        }

        public async Task<Pago> CrearAsync(Pago pago)
        {
            await _context.Pago.AddAsync(pago);
            await _context.SaveChangesAsync();
            return pago;
        }

        public async Task<bool> EliminarAsync(int idPago)
        {
            var entity = await _context.Pago.FindAsync(idPago);
            if (entity == null) return false;
            _context.Pago.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Pago>> ObtenerPorRangoFechasAsync(DateTime desde, DateTime hasta)
        {
            var d = desde.Date;
            var h = hasta.Date.AddDays(1).AddTicks(-1);
            return await _context.Pago
                .Include(p => p.idMetodoPagoNavigation)
                .Include(p => p.idCuentaNavigation)
                .Where(p => p.fechaHora >= d && p.fechaHora <= h)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pago>> ObtenerPorMetodoAsync(int idMetodoPago)
        {
            return await _context.Pago
                .Include(p => p.idMetodoPagoNavigation)
                .Include(p => p.idCuentaNavigation)
                .Where(p => p.idMetodoPago == idMetodoPago)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pago>> BuscarPorReferenciaAsync(string referencia)
        {
            referencia = referencia?.Trim().ToLower() ?? string.Empty;
            if (string.IsNullOrEmpty(referencia)) return new List<Pago>();

            return await _context.Pago
                .Include(p => p.idMetodoPagoNavigation)
                .Include(p => p.idCuentaNavigation)
                .Where(p => (p.numeroReferencia ?? string.Empty).ToLower().Contains(referencia))
                .ToListAsync();
        }
    }
}
