using ProyectoSauna.Models.Entities;
using System;

namespace ProyectoSauna.Models.DTOs
{
    public class DetEgresoDTO
    {
        public int idDetEgreso { get; set; }
        public int? idCabEgreso { get; set; }
        public string concepto { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public decimal monto { get; set; }
        public bool recurrente { get; set; }
        public string? comprobanteRuta { get; set; }
        public int idTipoEgreso { get; set; }
        public int? idUsuario { get; set; }

        // Datos de navegación útiles para listados
        public string? nombreTipoEgreso { get; set; }
        public string? nombreUsuario { get; set; }

        public static DetEgresoDTO FromEntity(DetEgreso e)
        {
            return new DetEgresoDTO
            {
                idDetEgreso = e.idDetEgreso,
                idCabEgreso = e.idCabEgreso,
                concepto = e.concepto,
                fecha = e.idCabEgresoNavigation?.fecha ?? DateTime.MinValue,
                monto = e.monto,
                recurrente = e.recurrente,
                comprobanteRuta = e.comprobanteRuta,
                idTipoEgreso = e.idTipoEgreso,
                idUsuario = e.idCabEgresoNavigation?.idUsuario,
                nombreTipoEgreso = e.idTipoEgresoNavigation?.nombre,
                nombreUsuario = e.idCabEgresoNavigation?.idUsuarioNavigation?.nombreUsuario
            };
        }

        public (CabEgreso cab, DetEgreso det) ToEntities()
        {
            var cab = new CabEgreso
            {
                idCabEgreso = this.idCabEgreso ?? 0,
                fecha = this.fecha,
                montoTotal = this.monto,
                idUsuario = this.idUsuario
            };

            var det = new DetEgreso
            {
                idDetEgreso = this.idDetEgreso,
                idCabEgreso = this.idCabEgreso,
                concepto = this.concepto,
                monto = this.monto,
                recurrente = this.recurrente,
                comprobanteRuta = this.comprobanteRuta,
                idTipoEgreso = this.idTipoEgreso
            };

            return (cab, det);
        }
    }
}
