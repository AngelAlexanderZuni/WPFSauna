using Microsoft.EntityFrameworkCore;
using ProyectoSauna.Models;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories.Base;
using ProyectoSauna.Repositories.Interfaces;

namespace ProyectoSauna.Repositories
{
    public class RolRepository : Repository<Rol>, IRolRepository
    {
        public RolRepository(SaunaDbContext context) : base(context)
        {
        }

        public async Task<Rol?> ObtenerPorNombreAsync(string nombre)
        {
            return await _dbSet.FirstOrDefaultAsync(r => r.nombre == nombre);
        }

        public override async Task<IEnumerable<Rol>> GetAllAsync()
        {
            return await _dbSet.OrderBy(r => r.nombre).ToListAsync();
        }
    }
}