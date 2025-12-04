using ProyectoSauna.Models.Entities;

namespace ProyectoSauna.Models.DTOs
{
    public class PromocionDTO
    {
        public int idPromocion { get; set; }
        public string nombreDescuento { get; set; } = string.Empty;
        public decimal montoDescuento { get; set; }
        public int idTipoDescuento { get; set; }
        public int valorCondicion { get; set; }
        public bool activo { get; set; }
        public string motivo { get; set; } = string.Empty;
        public string? nombreTipoDescuento { get; set; }

        public static PromocionDTO FromEntity(Promociones p)
        {
            return new PromocionDTO
            {
                idPromocion = p.idPromocion,
                nombreDescuento = p.nombreDescuento,
                montoDescuento = p.montoDescuento,
                idTipoDescuento = p.idTipoDescuento,
                valorCondicion = p.valorCondicion,
                activo = p.activo,
                motivo = p.motivo,
                nombreTipoDescuento = p.idTipoDescuentoNavigation?.nombre
            };
        }

        public Promociones ToEntity()
        {
            return new Promociones
            {
                idPromocion = this.idPromocion,
                nombreDescuento = this.nombreDescuento,
                montoDescuento = this.montoDescuento,
                idTipoDescuento = this.idTipoDescuento,
                valorCondicion = this.valorCondicion,
                activo = this.activo,
                motivo = this.motivo
            };
        }
    }
}