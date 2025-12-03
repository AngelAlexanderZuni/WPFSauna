using Microsoft.EntityFrameworkCore;
using ProyectoSauna.Data;
using ProyectoSauna.Models;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories.Base;
using ProyectoSauna.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories
{
    public class MovimientoInventarioRepository : Repository<MovimientoInventario>, IMovimientoInventarioRepository
    {
        private readonly SaunaDbContext _context;

        public MovimientoInventarioRepository(SaunaDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MovimientoInventario>> GetByProductoAsync(int idProducto)
        {
            return await _context.MovimientoInventario
                .Include(m => m.idTipoMovimientoNavigation)
                .Where(m => m.idProducto == idProducto)
                .OrderByDescending(m => m.fecha)
                .ToListAsync();
        }

        public async Task<IEnumerable<MovimientoInventario>> GetRecentAsync(int count = 20)
        {
            return await _context.MovimientoInventario
                .Include(m => m.idTipoMovimientoNavigation)
                .Include(m => m.idProductoNavigation)
                .OrderByDescending(m => m.fecha)
                .Take(count)
                .ToListAsync();
        }
    }

}
