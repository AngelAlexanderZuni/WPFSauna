using ProyectoSauna.Models.DTOs;
using ProyectoSauna.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoSauna.Services
{
    public class UsuarioService : Interfaces.IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        public UsuarioService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<IEnumerable<UsuarioDTO>> GetUsuariosActivosAsync()
        {
            var lista = await _usuarioRepository.ObtenerActivosAsync();
            return lista.Select(u => new UsuarioDTO
            {
                idUsuario = u.idUsuario,
                nombreUsuario = u.nombreUsuario
            });
        }
    }
}
