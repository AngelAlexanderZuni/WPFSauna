// Repositories/PromocionesRepository.cs
using Microsoft.EntityFrameworkCore;
using ProyectoSauna.Models;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories.Base;
using ProyectoSauna.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories
{
    public class PromocionesRepository : Repository<Promociones>, IPromocionesRepository
    {
        private readonly SaunaDbContext _context;

        public PromocionesRepository(SaunaDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Promociones>> ObtenerActivasAsync()
        {
            return await _context.Set<Promociones>()
                .Include(p => p.idTipoDescuentoNavigation)
                .Where(p => p.activo)
                .OrderBy(p => p.nombreDescuento)
                .ToListAsync();
        }

        public async Task<IEnumerable<Promociones>> ObtenerPorTipoAsync(int idTipoDescuento)
        {
            return await _context.Set<Promociones>()
                .Include(p => p.idTipoDescuentoNavigation)
                .Where(p => p.idTipoDescuento == idTipoDescuento)
                .OrderBy(p => p.nombreDescuento)
                .ToListAsync();
        }

        public async Task<IEnumerable<Promociones>> BuscarPorNombreAsync(string nombre)
        {
            var nombreLower = nombre.ToLower();
            return await _context.Set<Promociones>()
                .Include(p => p.idTipoDescuentoNavigation)
                .Where(p => p.nombreDescuento.ToLower().Contains(nombreLower) ||
                           p.motivo.ToLower().Contains(nombreLower))
                .OrderBy(p => p.nombreDescuento)
                .ToListAsync();
        }

        public async Task<Promociones?> ObtenerConTipoAsync(int idPromocion)
        {
            return await _context.Set<Promociones>()
                .Include(p => p.idTipoDescuentoNavigation)
                .FirstOrDefaultAsync(p => p.idPromocion == idPromocion);
        }

        public async Task<IEnumerable<Promociones>> ObtenerTodasConTipoAsync()
        {
            return await _context.Set<Promociones>()
                .Include(p => p.idTipoDescuentoNavigation)
                .OrderByDescending(p => p.activo)
                .ThenBy(p => p.nombreDescuento)
                .ToListAsync();
        }
    }
}