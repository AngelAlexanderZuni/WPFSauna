using System;
using System.Collections.Generic;

namespace ProyectoSauna.Models.Entities;

public partial class MetodoPago
{
    public int idMetodoPago { get; set; }

    public string nombre { get; set; } = null!;

    public virtual ICollection<Pago> Pago { get; set; } = new List<Pago>();
}
