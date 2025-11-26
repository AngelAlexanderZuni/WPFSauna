// Models/Entities/TipoDescuento.cs
using System.Collections.Generic;

namespace ProyectoSauna.Models.Entities
{
    public partial class TipoDescuento
    {
        public int idTipoDescuento { get; set; }
        public string nombre { get; set; } = string.Empty;

        public virtual ICollection<Promociones> Promociones { get; set; } = new List<Promociones>();
    }
}