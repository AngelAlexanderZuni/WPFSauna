using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProyectoSauna.Models;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories.Base;
using ProyectoSauna.Repositories.Interfaces;

namespace ProyectoSauna.Repositories
{
    public class EgresoRepository : Repository<CabEgreso>, IEgresoRepository
    {
        private readonly SaunaDbContext _context;

        public EgresoRepository(SaunaDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<CabEgreso> CrearEgresoAsync(CabEgreso cabecera, IEnumerable<DetEgreso> detalles)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.CabEgreso.AddAsync(cabecera);
                await _context.SaveChangesAsync();

                foreach (var d in detalles)
                {
                    d.idCabEgreso = cabecera.idCabEgreso;
                }

                await _context.DetEgreso.AddRangeAsync(detalles);
                await _context.SaveChangesAsync();

                // Recalcular total
                cabecera.montoTotal = await _context.DetEgreso
                    .Where(x => x.idCabEgreso == cabecera.idCabEgreso)
                    .SumAsync(x => (decimal?)x.monto) ?? 0m;

                _context.CabEgreso.Update(cabecera);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return cabecera;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<CabEgreso>> GetRecientesAsync(int count = 20)
        {
            return await _context.CabEgreso
                .Include(c => c.idUsuarioNavigation)
                .OrderByDescending(c => c.fecha)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<DetEgreso>> GetDetallesPorCabeceraAsync(int idCabEgreso)
        {
            return await _context.DetEgreso
                .Include(d => d.idTipoEgresoNavigation)
                .Include(d => d.idCabEgresoNavigation)
                    .ThenInclude(c => c.idUsuarioNavigation)
                .Where(d => d.idCabEgreso == idCabEgreso)
                .OrderBy(d => d.idDetEgreso)
                .ToListAsync();
        }

        public async Task<bool> ActualizarDetalleAsync(DetEgreso detalle)
        {
            var existente = await _context.DetEgreso.FirstOrDefaultAsync(x => x.idDetEgreso == detalle.idDetEgreso);
            if (existente == null) return false;

            existente.concepto = detalle.concepto;
            existente.monto = detalle.monto;
            existente.recurrente = detalle.recurrente;
            existente.comprobanteRuta = detalle.comprobanteRuta;
            existente.idTipoEgreso = detalle.idTipoEgreso;

            _context.DetEgreso.Update(existente);
            await _context.SaveChangesAsync();

            // Recalcular total de cabecera
            var cab = await _context.CabEgreso.FirstAsync(c => c.idCabEgreso == existente.idCabEgreso);
            cab.montoTotal = await _context.DetEgreso
                .Where(x => x.idCabEgreso == existente.idCabEgreso)
                .SumAsync(x => (decimal?)x.monto) ?? 0m;
            _context.CabEgreso.Update(cab);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> EliminarDetalleAsync(int idDetEgreso)
        {
            var existente = await _context.DetEgreso.FirstOrDefaultAsync(x => x.idDetEgreso == idDetEgreso);
            if (existente == null) return false;

            var idCab = existente.idCabEgreso;
            _context.DetEgreso.Remove(existente);
            await _context.SaveChangesAsync();

            // Recalcular total de cabecera
            var cab = await _context.CabEgreso.FirstAsync(c => c.idCabEgreso == idCab);
            cab.montoTotal = await _context.DetEgreso
                .Where(x => x.idCabEgreso == idCab)
                .SumAsync(x => (decimal?)x.monto) ?? 0m;
            _context.CabEgreso.Update(cab);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
