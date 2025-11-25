namespace ProyectoSauna.Models.Entities;

public partial class DetalleServicio
{
    public int idDetalleServicio { get; set; }

    public int cantidad { get; set; }

    public decimal precioUnitario { get; set; }

    public decimal subtotal { get; set; }

    public int idCuenta { get; set; }

    public int idServicio { get; set; }

    public virtual Cuenta idCuentaNavigation { get; set; } = null!;

    public virtual Servicio idServicioNavigation { get; set; } = null!;
}