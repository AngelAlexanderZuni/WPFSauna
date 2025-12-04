// Models/Entities/Promociones.cs
using System;

namespace ProyectoSauna.Models.Entities
{
    public partial class Promociones
    {
        public int idPromocion { get; set; }
        public string nombreDescuento { get; set; } = string.Empty;
        public decimal montoDescuento { get; set; }
        public int idTipoDescuento { get; set; }
        public int valorCondicion { get; set; }
        public bool activo { get; set; }
        public string motivo { get; set; } = string.Empty;

        public virtual TipoDescuento? idTipoDescuentoNavigation { get; set; }
    }
}