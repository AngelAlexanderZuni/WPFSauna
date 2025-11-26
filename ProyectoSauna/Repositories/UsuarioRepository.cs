    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using ProyectoSauna.Models;
    using ProyectoSauna.Models.Entities;
    using ProyectoSauna.Repositories.Base;
    using ProyectoSauna.Repositories.Interfaces;
    using System.Windows;

    namespace ProyectoSauna.Repositories
    {
        public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
        {
            public UsuarioRepository(SaunaDbContext context) : base(context)
            {
            }

            public async Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario)
            {
                return await _dbSet
                    .Include(u => u.idRolNavigation)
                    .FirstOrDefaultAsync(u => u.nombreUsuario == nombreUsuario);
            }

            public async Task<List<Usuario>> ObtenerActivosAsync()
            {
                return await _dbSet
                    .Include(u => u.idRolNavigation)
                    .Where(u => u.activo)
                    .OrderBy(u => u.nombreUsuario)
                    .ToListAsync();
            }

            public async Task<List<Usuario>> ObtenerInactivosAsync()
            {
                return await _dbSet
                    .Include(u => u.idRolNavigation)
                    .Where(u => !u.activo)  // ? Usuarios INactivos
                    .OrderBy(u => u.nombreUsuario)
                    .ToListAsync();
            }

            public async Task<List<Usuario>> ObtenerTodosConRelacionesAsync()
            {
                return await _dbSet
                    .Include(u => u.idRolNavigation)
                    .OrderBy(u => u.nombreUsuario)
                    .ToListAsync();
            }

            public async Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, int? excluirId = null)
            {
                var query = _dbSet.Where(u => u.nombreUsuario == nombreUsuario && u.activo);
            
                if (excluirId.HasValue)
                    query = query.Where(u => u.idUsuario != excluirId.Value);

                return await query.AnyAsync();
            }

            public async Task<bool> ValidarCredencialesAsync(string nombreUsuario, string passwordHash)
            {
                return await _dbSet
                    .AnyAsync(u => u.nombreUsuario == nombreUsuario 
                                && u.contraseniaHash == passwordHash 
                                && u.activo);
            }

            public async Task<bool> DesactivarAsync(int id)
            {
                var usuario = await GetByIdAsync(id);
                if (usuario == null) return false;

                usuario.activo = false;
                await _context.SaveChangesAsync();
                return true;
            }
        }
    }