using Microsoft.EntityFrameworkCore;
using ProyectoSauna.Models;
using ProyectoSauna.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories.Base
{
    public class CategoriaServicioRepository
    {
        private readonly SaunaDbContext _context;

        public CategoriaServicioRepository(SaunaDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CategoriaServicio>> GetAllAsync()
        {
            return await _context.CategoriaServicio.ToListAsync();
        }
    }
}