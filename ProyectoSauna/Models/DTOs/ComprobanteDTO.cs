using System;

namespace ProyectoSauna.Models.DTOs
{
    public class ComprobanteDTO
    {
        public int idComprobante { get; set; }
        public string serie { get; set; } = null!;
        public string numero { get; set; } = null!;
        public DateTime fechaEmision { get; set; }
        public decimal subtotal { get; set; }
        public decimal igv { get; set; }
        public decimal total { get; set; }
        public int idTipoComprobante { get; set; }
        public int idCuenta { get; set; }

        // Propiedades de navegación simplificadas o nombres para mostrar
        public string? TipoComprobanteNombre { get; set; }
    }
}
