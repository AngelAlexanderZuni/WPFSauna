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
    public class DetalleConsumoRepository : Repository<DetalleConsumo>, IDetalleConsumoRepository
    {
        private readonly SaunaDbContext _context;

        public DetalleConsumoRepository(SaunaDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DetalleConsumo>> GetByCuentaAsync(int idCuenta)
        {
            return await _context.DetalleConsumo
                .Include(d => d.idProductoNavigation)
                .Where(d => d.idCuenta == idCuenta)
                .ToListAsync();
        }

        public async Task<DetalleConsumo> GetByIdWithIncludesAsync(int idDetalle)
        {
            return await _context.DetalleConsumo
                .Include(d => d.idProductoNavigation)
                .Include(d => d.idCuentaNavigation)
                .FirstOrDefaultAsync(d => d.idDetalle == idDetalle);
        }
    }
}