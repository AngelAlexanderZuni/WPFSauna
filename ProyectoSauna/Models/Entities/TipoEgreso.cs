using System;
using System.Collections.Generic;

namespace ProyectoSauna.Models.Entities;

public partial class TipoEgreso
{
    public int idTipoEgreso { get; set; }

    public string nombre { get; set; } = null!;

    public virtual ICollection<DetEgreso> DetEgreso { get; set; } = new List<DetEgreso>();
}
