using System;
using System.Collections.Generic;

namespace ProyectoSauna.Models.Entities;

public partial class Rol
{
    public int idRol { get; set; }

    public string nombre { get; set; } = null!;

    public virtual ICollection<Usuario> Usuario { get; set; } = new List<Usuario>();
}
