using System;
using System.Collections.Generic;

namespace ProyectoSauna.Models.Entities;

public partial class Producto
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

    public virtual ICollection<DetalleConsumo> DetalleConsumo { get; set; } = new List<DetalleConsumo>();

    public virtual ICollection<MovimientoInventario> MovimientoInventario { get; set; } = new List<MovimientoInventario>();

    public virtual CategoriaProducto idCategoriaProductoNavigation { get; set; } = null!;
}
