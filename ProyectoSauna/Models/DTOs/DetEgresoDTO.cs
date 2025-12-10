using System;

namespace ProyectoSauna.Models.DTOs
{
    public class DetEgresoDTO
    {
        public int idDetEgreso { get; set; }
        public int? idCabEgreso { get; set; }
        public string concepto { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public bool recurrente { get; set; }
        public string? comprobanteRuta { get; set; }
        public int idTipoEgreso { get; set; }

        // Extra properties for display
        public string? TipoEgresoNombre { get; set; }
        public DateTime? Fecha { get; set; } 
        public string? UsuarioNombre { get; set; }
    }
}
