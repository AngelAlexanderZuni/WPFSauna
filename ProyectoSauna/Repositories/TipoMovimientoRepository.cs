using Microsoft.EntityFrameworkCore;
using ProyectoSauna.Models;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories
{
    public class TipoMovimientoRepository : ITipoMovimientoRepository
    {
        private readonly SaunaDbContext _context;

        public TipoMovimientoRepository(SaunaDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TipoMovimiento>> GetAllAsync()
        {
            return await _context.TipoMovimiento.ToListAsync();
        }

        public async Task<TipoMovimiento> GetByIdAsync(int id)
        {
            return await _context.TipoMovimiento.FindAsync(id);
        }

        public async Task<IEnumerable<TipoMovimiento>> GetByTipoAsync(string tipo)
        {
            return await _context.TipoMovimiento
                .Where(t => t.descripcion.ToLower().Contains(tipo.ToLower()))
                .ToListAsync();
        }
    }
}