using ProyectoSauna.Models.Entities;

namespace ProyectoSauna.Repositories.Interfaces
{
    public interface IUsuarioRepository : IRepository<Usuario>
    {
        Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario);
        Task<List<Usuario>> ObtenerActivosAsync();
        Task<List<Usuario>> ObtenerInactivosAsync();
        Task<List<Usuario>> ObtenerTodosConRelacionesAsync(); 
        Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, int? excluirId = null);
        Task<bool> ValidarCredencialesAsync(string nombreUsuario, string passwordHash);
        Task<bool> DesactivarAsync(int id);
    }
}