using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSauna.Models.Entities;

public partial class Cuenta
{
    public int idCuenta { get; set; }

    public DateTime fechaHoraCreacion { get; set; }

    [NotMapped]
    public decimal precioEntrada { get; set; }

    public DateTime? fechaHoraSalida { get; set; }

    public decimal subtotalConsumos { get; set; }

    [NotMapped]
    public decimal montoPagado { get; set; }

    public decimal descuento { get; set; }

    public decimal total { get; set; }

    [NotMapped]
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

    public virtual ICollection<DetalleServicio> DetalleServicio { get; set; } = new List<DetalleServicio>();
}
