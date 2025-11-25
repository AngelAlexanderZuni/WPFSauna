using System.Collections.Generic;

namespace ProyectoSauna.Models.Entities;

public partial class Servicio
{
    public int idServicio { get; set; }

    public string nombre { get; set; } = null!;

    public decimal precio { get; set; }

    public int? duracionEstimada { get; set; }

    public bool activo { get; set; }

    public int? idCategoriaServicio { get; set; }

    public virtual CategoriaServicio? idCategoriaServicioNavigation { get; set; }

    public virtual ICollection<DetalleServicio> DetalleServicio { get; set; } = new List<DetalleServicio>();
}