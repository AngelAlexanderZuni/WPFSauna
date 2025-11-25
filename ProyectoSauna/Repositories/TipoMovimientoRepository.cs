using ProyectoSauna.Models.Entities;
using ProyectoSauna.Models;
using ProyectoSauna.Repositories.Base;
using ProyectoSauna.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories
{
    public class TipoMovimientoRepository : Repository<TipoMovimiento>, ITipoMovimientoRepository
    {
        private readonly SaunaDbContext _context;

        public TipoMovimientoRepository(SaunaDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TipoMovimiento>> GetByTipoAsync(string tipo)
        {
            return await _context.TipoMovimiento
                .Where(t => t.tipo == tipo)            
                .OrderBy(t => t.descripcion)          
                .ToListAsync();
        }
    }
}
