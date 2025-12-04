using Microsoft.EntityFrameworkCore;
using ProyectoSauna.Models;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories.Base;
using ProyectoSauna.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories
{
    public class EgresoRepository : Repository<DetEgreso>, IEgresoRepository
    {
        private readonly SaunaDbContext _context;

        public EgresoRepository(SaunaDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DetEgreso>> ObtenerPorTipoAsync(int idTipoEgreso)
        {
            return await _context.DetEgreso
                .Include(e => e.idTipoEgresoNavigation)
                .Include(e => e.idCabEgresoNavigation)
                    .ThenInclude(c => c.idUsuarioNavigation)
                .Where(e => e.idTipoEgreso == idTipoEgreso)
                .OrderByDescending(e => e.idCabEgresoNavigation!.fecha)
                .ToListAsync();
        }

        public async Task<IEnumerable<DetEgreso>> ObtenerPorRangoFechasAsync(DateTime desde, DateTime hasta)
        {
            var desdeDate = desde.Date;
            var hastaInclusive = hasta.Date.AddDays(1).AddTicks(-1);

            return await _context.DetEgreso
                .Include(e => e.idTipoEgresoNavigation)
                .Include(e => e.idCabEgresoNavigation)
                    .ThenInclude(c => c.idUsuarioNavigation)
                .Where(e => e.idCabEgresoNavigation != null && e.idCabEgresoNavigation.fecha >= desdeDate && e.idCabEgresoNavigation.fecha <= hastaInclusive)
                .OrderByDescending(e => e.idCabEgresoNavigation!.fecha)
                .ToListAsync();
        }

        public async Task<IEnumerable<DetEgreso>> BuscarPorConceptoAsync(string texto)
        {
            texto = texto?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(texto)) return new List<DetEgreso>();

            return await _context.DetEgreso
                .Include(e => e.idTipoEgresoNavigation)
                .Include(e => e.idCabEgresoNavigation)
                    .ThenInclude(c => c.idUsuarioNavigation)
                .Where(e => e.concepto.ToLower().Contains(texto.ToLower()))
                .OrderByDescending(e => e.idCabEgresoNavigation!.fecha)
                .ToListAsync();
        }

        public async Task<IEnumerable<DetEgreso>> ObtenerConNavegacionAsync()
        {
            return await _context.DetEgreso
                .Include(e => e.idTipoEgresoNavigation)
                .Include(e => e.idCabEgresoNavigation)
                    .ThenInclude(c => c.idUsuarioNavigation)
                .OrderByDescending(e => e.idCabEgresoNavigation!.fecha)
                .ToListAsync();
        }
    }
}
