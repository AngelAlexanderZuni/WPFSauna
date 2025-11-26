using ProyectoSauna.Models.Entities;

namespace ProyectoSauna.Models.DTOs
{
    public class CategoriaProductoDTO
    {
        public int idCategoriaProducto { get; set; }
        public string nombre { get; set; } = null!;
        public int? cantidadProductos { get; set; }

        public static CategoriaProductoDTO FromEntity(CategoriaProducto categoria)
        {
            return new CategoriaProductoDTO
            {
                idCategoriaProducto = categoria.idCategoriaProducto,
                nombre = categoria.nombre,
                cantidadProductos = categoria.Producto?.Count
            };
        }

        public CategoriaProducto ToEntity()
        {
            return new CategoriaProducto
            {
                idCategoriaProducto = this.idCategoriaProducto,
                nombre = this.nombre
            };
        }
    }
}
