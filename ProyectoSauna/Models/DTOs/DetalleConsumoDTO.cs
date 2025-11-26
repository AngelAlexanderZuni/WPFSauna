using System;

namespace ProyectoSauna.Models.DTOs
{
    /// <summary>
    /// DTO para detalles de consumo
    /// Será implementado completamente por Angel Zuñiga (Módulo Consumos)
    /// </summary>
    public class DetalleConsumoDTO
    {
        public int idDetalle { get; set; }
        public int idCuenta { get; set; }
        public int idProducto { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int cantidad { get; set; }
        public decimal precioUnitario { get; set; }
        public decimal subtotal { get; set; }
        public DateTime fechaHora { get; set; }
        
        // Propiedades computadas
        public string Hora => fechaHora.ToString("HH:mm");
        public string Fecha => fechaHora.ToString("dd/MM/yyyy");
    }
}
