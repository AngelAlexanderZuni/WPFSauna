// Repositories/TipoDescuentoRepository.cs
using Microsoft.EntityFrameworkCore;
using ProyectoSauna.Models;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories.Base;
using ProyectoSauna.Repositories.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories
{
    public class TipoDescuentoRepository : Repository<TipoDescuento>, ITipoDescuentoRepository
    {
        private readonly SaunaDbContext _context;

        public TipoDescuentoRepository(SaunaDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<TipoDescuento?> ObtenerPorNombreAsync(string nombre)
        {
            return await _context.Set<TipoDescuento>()
                .FirstOrDefaultAsync(t => t.nombre.ToLower() == nombre.ToLower());
        }
    }
}