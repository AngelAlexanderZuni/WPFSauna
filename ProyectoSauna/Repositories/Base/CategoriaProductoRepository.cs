using Microsoft.EntityFrameworkCore;
using ProyectoSauna.Data;
using ProyectoSauna.Models;
using ProyectoSauna.Models.DTOs;
using ProyectoSauna.Models.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories.Base
{
    public class CategoriaProductoRepository
    {
        private readonly SaunaDbContext _context;

        public CategoriaProductoRepository(SaunaDbContext context)
        {
            _context = context;
        }

        // 🟢 Mantienes el método actual (por compatibilidad)
        public async Task<IEnumerable<CategoriaProducto>> GetAllAsync()
        {
            return await _context.CategoriaProducto.ToListAsync();
        }

        // 🟦 Nuevo método que devuelve DTOs
        public async Task<IEnumerable<CategoriaProductoDTO>> GetAllDTOAsync()
        {
            return await _context.CategoriaProducto
                .Select(c => new CategoriaProductoDTO
                {
                    idCategoriaProducto = c.idCategoriaProducto,
                    nombre = c.nombre
                })
                .ToListAsync();
        }
    }
}
