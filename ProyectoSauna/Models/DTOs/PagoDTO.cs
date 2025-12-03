using System;

namespace ProyectoSauna.Models.DTOs
{
    public class PagoDTO
    {
        public int idPago { get; set; }
        public DateTime fechaHora { get; set; }
        public decimal monto { get; set; }
        public string? numeroReferencia { get; set; }
        public int idMetodoPago { get; set; }
        public int idCuenta { get; set; }
        // Auxiliares
        public string? metodoPagoNombre { get; set; }
    }
}
