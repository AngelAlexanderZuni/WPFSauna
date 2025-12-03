using System;
using System.Collections.Generic;

namespace ProyectoSauna.Models.Entities;

public partial class DetEgreso
{
    public int idDetEgreso { get; set; }

    public int? idCabEgreso { get; set; }

    public string concepto { get; set; } = null!;

    public decimal monto { get; set; }

    public bool recurrente { get; set; }

    public string? comprobanteRuta { get; set; }

    public int idTipoEgreso { get; set; }

    public virtual CabEgreso? idCabEgresoNavigation { get; set; }

    public virtual TipoEgreso idTipoEgresoNavigation { get; set; } = null!;
}
