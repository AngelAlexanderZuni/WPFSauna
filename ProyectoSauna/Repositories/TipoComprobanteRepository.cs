using Microsoft.EntityFrameworkCore;
using ProyectoSauna.Models;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories.Base;
using ProyectoSauna.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories
{
    public class TipoComprobanteRepository : Repository<TipoComprobante>, ITipoComprobanteRepository
    {
        public TipoComprobanteRepository(SaunaDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<TipoComprobante>> GetAllAsync()
        {
            return await _context.TipoComprobante.ToListAsync();
        }
    }
}
