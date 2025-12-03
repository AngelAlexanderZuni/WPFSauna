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
    public class ComprobanteRepository : Repository<Comprobante>, IComprobanteRepository
    {
        public ComprobanteRepository(SaunaDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Comprobante>> GetAllAsync()
        {
            return await _context.Comprobante
                .Include(c => c.idTipoComprobanteNavigation)
                .Include(c => c.idCuentaNavigation)
                .OrderByDescending(c => c.fechaEmision)
                .ToListAsync();
        }

        public override async Task<Comprobante?> GetByIdAsync(int id)
        {
            return await _context.Comprobante
                .Include(c => c.idTipoComprobanteNavigation)
                .Include(c => c.idCuentaNavigation)
                .FirstOrDefaultAsync(c => c.idComprobante == id);
        }

        public async Task<List<Comprobante>> GetByCuentaIdAsync(int idCuenta)
        {
            return await _context.Comprobante
                .Include(c => c.idTipoComprobanteNavigation)
                .Where(c => c.idCuenta == idCuenta)
                .OrderByDescending(c => c.fechaEmision)
                .ToListAsync();
        }
    }
}
