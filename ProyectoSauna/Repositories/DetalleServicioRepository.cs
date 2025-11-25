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
    public class DetalleServicioRepository : Repository<DetalleServicio>, IDetalleServicioRepository
    {
        private readonly SaunaDbContext _context;

        public DetalleServicioRepository(SaunaDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DetalleServicio>> GetByServicioAsync(int idServicio)
        {
            return await _context.DetalleServicio
                .Include(d => d.idCuentaNavigation)
                .Include(d => d.idServicioNavigation)
                .Where(d => d.idServicio == idServicio)
                .OrderByDescending(d => d.idDetalleServicio)
                .ToListAsync();
        }

        public async Task<IEnumerable<DetalleServicio>> GetByCuentaAsync(int idCuenta)
        {
            return await _context.DetalleServicio
                .Include(d => d.idCuentaNavigation)
                .Include(d => d.idServicioNavigation)
                .Where(d => d.idCuenta == idCuenta)
                .OrderByDescending(d => d.idDetalleServicio)
                .ToListAsync();
        }

        public async Task<IEnumerable<DetalleServicio>> GetRecentAsync(int count = 20)
        {
            return await _context.DetalleServicio
                .Include(d => d.idCuentaNavigation)
                .Include(d => d.idServicioNavigation)
                .OrderByDescending(d => d.idDetalleServicio)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<DetalleServicio>> GetAllWithIncludesAsync()
        {
            return await _context.DetalleServicio
                .Include(d => d.idCuentaNavigation)
                .Include(d => d.idServicioNavigation)
                .OrderByDescending(d => d.idDetalleServicio)
                .ToListAsync();
        }
    }
}