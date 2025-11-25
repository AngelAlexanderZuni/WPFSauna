using System;
using System.Collections.Generic;

namespace ProyectoSauna.Models.Entities;

public partial class MovimientoInventario
{
    public int idMovimiento { get; set; }

    public int cantidad { get; set; }

    public decimal costoUnitario { get; set; }

    public decimal costoTotal { get; set; }

    public DateTime fecha { get; set; }

    public string? observaciones { get; set; }

    public int idTipoMovimiento { get; set; }

    public int idProducto { get; set; }

    public int idUsuario { get; set; }

    public virtual Producto idProductoNavigation { get; set; } = null!;

    public virtual TipoMovimiento idTipoMovimientoNavigation { get; set; } = null!;

    public virtual Usuario idUsuarioNavigation { get; set; } = null!;
}
