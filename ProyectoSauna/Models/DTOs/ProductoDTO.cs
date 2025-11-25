using ProyectoSauna.Models.Entities;

namespace ProyectoSauna.Models.DTOs
{
    /// <summary>
    /// DTO para transferir datos de productos sin exponer la entidad directamente
    /// </summary>
    public class ProductoDTO
    {
        public int idProducto { get; set; }
        public string codigo { get; set; } = null!;
        public string nombre { get; set; } = null!;
        public string? descripcion { get; set; }
        public decimal precioCompra { get; set; }
        public decimal precioVenta { get; set; }
        public int stockActual { get; set; }
        public int stockMinimo { get; set; }
        public bool activo { get; set; }
        public int idCategoriaProducto { get; set; }
        public string? nombreCategoria { get; set; }

        /// <summary>
        /// Convierte una entidad Producto a ProductoDTO
        /// </summary>
        public static ProductoDTO FromEntity(Producto producto)
        {
            return new ProductoDTO
            {
                idProducto = producto.idProducto,
                codigo = producto.codigo,
                nombre = producto.nombre,
                descripcion = producto.descripcion,
                precioCompra = producto.precioCompra,
                precioVenta = producto.precioVenta,
                stockActual = producto.stockActual,
                stockMinimo = producto.stockMinimo,
                activo = producto.activo,
                idCategoriaProducto = producto.idCategoriaProducto,
                nombreCategoria = producto.idCategoriaProductoNavigation?.nombre
            };
        }

        /// <summary>
        /// Convierte el DTO a una entidad Producto
        /// </summary>
        public Producto ToEntity()
        {
            return new Producto
            {
                idProducto = this.idProducto,
                codigo = this.codigo,
                nombre = this.nombre,
                descripcion = this.descripcion,
                precioCompra = this.precioCompra,
                precioVenta = this.precioVenta,
                stockActual = this.stockActual,
                stockMinimo = this.stockMinimo,
                activo = this.activo,
                idCategoriaProducto = this.idCategoriaProducto
            };
        }
    }
}
