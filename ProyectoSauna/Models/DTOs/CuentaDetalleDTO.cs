using System.Collections.Generic;

namespace ProyectoSauna.Models.DTOs
{
    /// <summary>
    /// DTO para mostrar cuenta con todos sus detalles de consumo
    /// Usado para integración con módulo de Consumos (Angel Zuñiga)
    /// </summary>
    public class CuentaDetalleDTO
    {
        public CuentaDTO Cuenta { get; set; } = new();
        public List<DetalleConsumoDTO> Consumos { get; set; } = new();
        
        // Propiedades Computadas
        public decimal TotalGeneral => Cuenta?.total ?? 0;
        public int TotalConsumos => Consumos?.Count ?? 0;
        public decimal SubtotalConsumos => Consumos?.Sum(c => c.subtotal) ?? 0;
    }
}
