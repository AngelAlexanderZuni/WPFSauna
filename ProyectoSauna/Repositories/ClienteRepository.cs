using ProyectoSauna.Models;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories.Base;
using ProyectoSauna.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories
{
    public class ClienteRepository : Repository<Cliente>, IClienteRepository
    {
        public ClienteRepository(SaunaDbContext context) : base(context)
        {
        }

        public async Task<Cliente?> GetByDNIAsync(string dni)
        {
            return await _context.Cliente
                .Include(c => c.idProgramaNavigation)
                .FirstOrDefaultAsync(c => c.numero_documento == dni);
        }

        public async Task<List<Cliente>> BuscarPorNombreAsync(string nombre)
        {
            var nombreBusqueda = $"%{nombre.Trim().ToLower()}%";
            return await _context.Cliente
                .Include(c => c.idProgramaNavigation)
                .Where(c =>
                    EF.Functions.Like((c.nombre + " " + c.apellidos).ToLower(), nombreBusqueda) ||
                    EF.Functions.Like(c.nombre.ToLower(), nombreBusqueda) ||
                    EF.Functions.Like(c.apellidos.ToLower(), nombreBusqueda)
                )
                .OrderBy(c => c.nombre)
                .ToListAsync();
        }

        public async Task<List<Cliente>> GetClientesActivosAsync()
        {
            return await _context.Cliente
                .Include(c => c.idProgramaNavigation)
                .Where(c => c.activo)
                .OrderBy(c => c.nombre)
                .ToListAsync();
        }

        public async Task<bool> ExisteDNIAsync(string dni, int? idClienteExcluir = null)
        {
            var query = _context.Cliente.Where(c => c.numero_documento == dni);
            
            if (idClienteExcluir.HasValue)
            {
                query = query.Where(c => c.idCliente != idClienteExcluir.Value);
            }
            
            return await query.AnyAsync();
        }
    }
}
