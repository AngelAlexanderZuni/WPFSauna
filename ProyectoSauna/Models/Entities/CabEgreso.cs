using System;
using System.Collections.Generic;

namespace ProyectoSauna.Models.Entities;

public partial class CabEgreso
{
    public int idCabEgreso { get; set; }

    public DateTime fecha { get; set; }

    public decimal? montoTotal { get; set; }

    public int? idUsuario { get; set; }

    public virtual ICollection<DetEgreso> DetEgreso { get; set; } = new List<DetEgreso>();

    public virtual Usuario? idUsuarioNavigation { get; set; }
}
