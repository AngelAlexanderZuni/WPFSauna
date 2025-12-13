using System;

namespace ProyectoSauna.Models.DTOs
{
    public class MovimientoCajaDTO
    {
        public DateTime FechaHora { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string Tipo { get; set; } = string.Empty; // "Ingreso" o "Egreso"
        public string Color { get; set; } = string.Empty; // Para visualización (Verde/Rojo)
        public string Usuario { get; set; } = string.Empty;
    }
}
