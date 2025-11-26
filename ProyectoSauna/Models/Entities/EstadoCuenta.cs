using System;
using System.Collections.Generic;

namespace ProyectoSauna.Models.Entities;

public partial class EstadoCuenta
{
    public int idEstadoCuenta { get; set; }

    public string nombre { get; set; } = null!;

    public virtual ICollection<Cuenta> Cuenta { get; set; } = new List<Cuenta>();
}
