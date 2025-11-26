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
    public class ServicioRepository : Repository<Servicio>, IServicioRepository
    {
        private readonly SaunaDbContext _context;

        public ServicioRepository(SaunaDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Servicio>> ObtenerPorCategoriaAsync(int idCategoriaServicio)
        {
            return await _context.Servicio
                .Where(s => s.idCategoriaServicio == idCategoriaServicio)
                .ToListAsync();
        }

        public async Task<IEnumerable<Servicio>> BuscarPorNombreAsync(string nombre)
        {
            return await _context.Servicio
                .Where(s => s.nombre.ToLower().Contains(nombre.ToLower()))
                .ToListAsync();
        }
    }
}