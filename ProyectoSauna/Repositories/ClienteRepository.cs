// Repositories/ClienteRepository.cs - COMPLETO
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
    public class ClienteRepository : Repository<Cliente>, IClienteRepository
    {
        private readonly SaunaDbContext _context;

        public ClienteRepository(SaunaDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Cliente?> GetByIdAsync(int idCliente)
        {
            // 🔄 FORZAR RECARGA DESDE BD (sin caché de Entity Framework)
            var cliente = await _context.Cliente
                .AsNoTracking() // No usar caché de EF
                .FirstOrDefaultAsync(c => c.idCliente == idCliente);
                
            return cliente;
        }

        public async Task<Cliente?> GetByDNIAsync(string dni)
        {
            // 🔄 FORZAR RECARGA DESDE BD (sin caché de Entity Framework)
            return await _context.Cliente
                .AsNoTracking() // No usar caché de EF
                .FirstOrDefaultAsync(c => c.numero_documento == dni);
        }

        public async Task<Cliente?> ObtenerPorDocumentoAsync(string numeroDocumento)
        {
            return await _context.Cliente.FirstOrDefaultAsync(c => c.numero_documento == numeroDocumento);
        }

        public async Task<IEnumerable<Cliente>> BuscarPorNombreAsync(string nombre)
        {
            var nombreLower = nombre.ToLower();
            return await _context.Cliente
                .Where(c => c.nombre.ToLower().Contains(nombreLower) || c.apellidos.ToLower().Contains(nombreLower))
                .OrderBy(c => c.nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Cliente>> ObtenerActivosAsync()
        {
            return await _context.Cliente.Where(c => c.activo).OrderBy(c => c.nombre).ToListAsync();
        }

        public async Task<IEnumerable<Cliente>> GetClientesActivosAsync()
        {
            return await ObtenerActivosAsync();
        }

        public async Task<IEnumerable<Cliente>> ObtenerConVisitasMinimasAsync(int visitasMinimas)
        {
            return await _context.Cliente
                .Where(c => c.visitasTotales >= visitasMinimas)
                .OrderByDescending(c => c.visitasTotales)
                .ToListAsync();
        }

        public async Task<bool> ExisteDNIAsync(string dni, int? idClienteExcluir = null)
        {
            if (string.IsNullOrWhiteSpace(dni))
                return false;

            var query = _context.Cliente.Where(c => c.numero_documento == dni);

            if (idClienteExcluir.HasValue)
                query = query.Where(c => c.idCliente != idClienteExcluir.Value);

            return await query.AnyAsync();
        }
    }
}