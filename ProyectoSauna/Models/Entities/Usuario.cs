using System;
using System.Collections.Generic;

namespace ProyectoSauna.Models.Entities;

public partial class Usuario
{
    public int idUsuario { get; set; }

    public string nombreUsuario { get; set; } = null!;

    public string contraseniaHash { get; set; } = null!;

    public string? correo { get; set; }

    public DateTime fechaCreacion { get; set; }

    public bool activo { get; set; }

    public int idRol { get; set; }

    // TODO: Módulo Egresos temporalmente deshabilitado
    // public virtual ICollection<CabEgreso> CabEgreso { get; set; } = new List<CabEgreso>();

    public virtual ICollection<Cuenta> Cuenta { get; set; } = new List<Cuenta>();

    public virtual ICollection<MovimientoInventario> MovimientoInventario { get; set; } = new List<MovimientoInventario>();

    public virtual Rol idRolNavigation { get; set; } = null!;
}
