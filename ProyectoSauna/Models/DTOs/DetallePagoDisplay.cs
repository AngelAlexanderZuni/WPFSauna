namespace ProyectoSauna.Models.DTOs
{
    public class DetallePagoDisplay
    {
        public string Descripcion { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty; // "Producto" o "Servicio"
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
