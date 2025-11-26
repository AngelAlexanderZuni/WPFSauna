using System;
using System.Collections.Generic;

namespace ProyectoSauna.Models.Entities;

public partial class DetalleConsumo
{
    public int idDetalle { get; set; }

    public int cantidad { get; set; }

    public decimal precioUnitario { get; set; }

    public decimal subtotal { get; set; }

    public int idCuenta { get; set; }

    public int idProducto { get; set; }

    public virtual Cuenta idCuentaNavigation { get; set; } = null!;

    public virtual Producto idProductoNavigation { get; set; } = null!;
}
