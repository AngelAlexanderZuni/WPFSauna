using System;
using System.Collections.Generic;

namespace ProyectoSauna.Models.Entities;

public partial class TipoComprobante
{
    public int idTipoComprobante { get; set; }

    public string nombre { get; set; } = null!;

    public virtual ICollection<Comprobante> Comprobante { get; set; } = new List<Comprobante>();
}
