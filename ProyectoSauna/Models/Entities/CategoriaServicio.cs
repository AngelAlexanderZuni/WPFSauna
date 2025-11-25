using System.Collections.Generic;

namespace ProyectoSauna.Models.Entities;

public partial class CategoriaServicio
{
    public int idCategoriaServicio { get; set; }

    public string nombre { get; set; } = null!;

    public bool activo { get; set; }

    public virtual ICollection<Servicio> Servicio { get; set; } = new List<Servicio>();
}