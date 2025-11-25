using System;
using System.Collections.Generic;

namespace ProyectoSauna.Models.Entities;

public partial class TipoMovimiento
{
    public int idTipoMovimiento { get; set; }

    public string descripcion { get; set; } = null!;

    public string tipo { get; set; } = null!;

    public virtual ICollection<MovimientoInventario> MovimientoInventario { get; set; } = new List<MovimientoInventario>();
}
