namespace ProyectoSauna.Models.DTOs
{
    public class ReporteClienteDTO
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public int Visitas { get; set; }
        public decimal TotalGastado { get; set; }
    }
}
