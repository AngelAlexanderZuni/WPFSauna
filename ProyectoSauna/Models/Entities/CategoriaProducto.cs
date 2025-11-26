using System;
using System.Collections.Generic;

namespace ProyectoSauna.Models.Entities;

public partial class CategoriaProducto
{
    public int idCategoriaProducto { get; set; }

    public string nombre { get; set; } = null!;

    public virtual ICollection<Producto> Producto { get; set; } = new List<Producto>();
}
