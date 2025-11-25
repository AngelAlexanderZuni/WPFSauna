using System;
using System.Collections.Generic;

namespace ProyectoSauna.Models.Entities;

public partial class Cuenta
{
    public int idCuenta { get; set; }
    public DateTime fechaHoraCreacion { get; set; }
    public decimal precioEntrada { get; set; }
    public DateTime? fechaHoraSalida { get; set; }
    public decimal subtotalConsumos { get; set; }
    public decimal montoPagado { get; set; }
    public decimal descuento { get; set; }

    public decimal total { get; set; }

    public decimal saldo { get; set; }

    public int idEstadoCuenta { get; set; }

    public int idUsuarioCreador { get; set; }

    public int idCliente { get; set; }

    public virtual Comprobante? Comprobante { get; set; }

    public virtual ICollection<DetalleConsumo> DetalleConsumo { get; set; } = new List<DetalleConsumo>();

    public virtual ICollection<Pago> Pago { get; set; } = new List<Pago>();

    public virtual Cliente idClienteNavigation { get; set; } = null!;

    public virtual EstadoCuenta idEstadoCuentaNavigation { get; set; } = null!;

    public virtual Usuario idUsuarioCreadorNavigation { get; set; } = null!;
}
