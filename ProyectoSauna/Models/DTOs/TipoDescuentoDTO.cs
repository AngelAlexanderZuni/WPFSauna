using ProyectoSauna.Models.Entities;

namespace ProyectoSauna.Models.DTOs
{
    public class TipoDescuentoDTO
    {
        public int idTipoDescuento { get; set; }
        public string nombre { get; set; } = string.Empty;

        public static TipoDescuentoDTO FromEntity(TipoDescuento t)
        {
            return new TipoDescuentoDTO
            {
                idTipoDescuento = t.idTipoDescuento,
                nombre = t.nombre
            };
        }

        public TipoDescuento ToEntity()
        {
            return new TipoDescuento
            {
                idTipoDescuento = this.idTipoDescuento,
                nombre = this.nombre
            };
        }
    }
}