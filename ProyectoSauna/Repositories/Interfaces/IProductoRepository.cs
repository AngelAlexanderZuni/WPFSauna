using ProyectoSauna.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoSauna.Repositories.Interfaces
{
    public interface IProductoRepository : IRepository<Producto>
    {
        // Métodos específicos para Producto
        Task<IEnumerable<Producto>> ObtenerPorCategoriaAsync(int idCategoria);
        Task<IEnumerable<Producto>> BuscarPorNombreAsync(string nombre);
    }
}