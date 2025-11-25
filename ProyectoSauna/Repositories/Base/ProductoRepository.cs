using Microsoft.EntityFrameworkCore;
using ProyectoSauna.Data;
using ProyectoSauna.Models;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories.Base;
using ProyectoSauna.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories
{
    public class ProductoRepository : Repository<Producto>, IProductoRepository
    {
        private readonly SaunaDbContext _context;

        public ProductoRepository(SaunaDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Producto>> ObtenerPorCategoriaAsync(int idCategoria)
        {
            return await _context.Producto
                .Where(p => p.idCategoriaProducto == idCategoria)
                .ToListAsync();
        }

        public async Task<IEnumerable<Producto>> BuscarPorNombreAsync(string nombre)
        {
            return await _context.Producto
                .Where(p => p.nombre.ToLower().Contains(nombre.ToLower()))
                .ToListAsync();
        }

        public async Task<IEnumerable<Producto>> GetAllFreshAsync()
        {
            return await _context.Producto.AsNoTracking().ToListAsync();
        }
    }
}