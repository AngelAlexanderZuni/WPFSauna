using ProyectoSauna.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoSauna.Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<IEnumerable<UsuarioDTO>> GetUsuariosActivosAsync();
    }
}
