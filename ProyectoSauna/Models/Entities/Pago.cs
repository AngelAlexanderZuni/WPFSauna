using System;
using System.Collections.Generic;

namespace ProyectoSauna.Models.Entities;

public partial class Pago
{
    public int idPago { get; set; }

    public DateTime fechaHora { get; set; }

    public decimal monto { get; set; }

    public string? numeroReferencia { get; set; }

    public int idMetodoPago { get; set; }

    public int idCuenta { get; set; }

    public virtual Cuenta idCuentaNavigation { get; set; } = null!;

    public virtual MetodoPago idMetodoPagoNavigation { get; set; } = null!;
}
