using Microsoft.EntityFrameworkCore;
using ProyectoSauna.Models;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories
{
    public class MovimientoInventarioRepository : IMovimientoInventarioRepository
    {
        private readonly SaunaDbContext _context;

        public MovimientoInventarioRepository(SaunaDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MovimientoInventario>> GetAllAsync()
        {
            return await _context.MovimientoInventario
                .Include(m => m.idProductoNavigation)
                .Include(m => m.idTipoMovimientoNavigation)
                .Include(m => m.idUsuarioNavigation)
                .OrderByDescending(m => m.fecha)
                .ToListAsync();
        }

        public async Task<MovimientoInventario> GetByIdAsync(int id)
        {
            return await _context.MovimientoInventario
                .Include(m => m.idProductoNavigation)
                .Include(m => m.idTipoMovimientoNavigation)
                .Include(m => m.idUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.idMovimiento == id);
        }

        public async Task<IEnumerable<MovimientoInventario>> GetByProductoAsync(int idProducto)
        {
            return await _context.MovimientoInventario
                .Include(m => m.idProductoNavigation)
                .Include(m => m.idTipoMovimientoNavigation)
                .Include(m => m.idUsuarioNavigation)
                .Where(m => m.idProducto == idProducto)
                .OrderByDescending(m => m.fecha)
                .ToListAsync();
        }

        // ✅ MÉTODO NUEVO: Obtener los movimientos más recientes
        public async Task<IEnumerable<MovimientoInventario>> GetRecentAsync(int count = 50)
        {
            return await _context.MovimientoInventario
                .Include(m => m.idProductoNavigation)
                .Include(m => m.idTipoMovimientoNavigation)
                .Include(m => m.idUsuarioNavigation)
                .OrderByDescending(m => m.fecha)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> AddAsync(MovimientoInventario movimiento)
        {
            _context.MovimientoInventario.Add(movimiento);
            await _context.SaveChangesAsync();
            return movimiento.idMovimiento;
        }

        public async Task UpdateAsync(MovimientoInventario movimiento)
        {
            _context.Entry(movimiento).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var movimiento = await _context.MovimientoInventario.FindAsync(id);
            if (movimiento != null)
            {
                _context.MovimientoInventario.Remove(movimiento);
                await _context.SaveChangesAsync();
            }
        }
    }
}