namespace ProyectoSauna.Models.DTOs
{
    public class ReporteProductoDTO
    {
        public string NombreProducto { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal IngresosGenerados { get; set; } // Opcional, si queremos mostrar cuánto dinero generó
    }
}
