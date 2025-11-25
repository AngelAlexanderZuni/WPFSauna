using System;

namespace ProyectoSauna.Models.DTOs
{
    public class ClienteDTO
    {
        public int idCliente { get; set; }

        public string nombre { get; set; } = null!;

        public string apellidos { get; set; } = null!;


        public string numero_documento { get; set; } = null!;

        public string? telefono { get; set; }

        public string? correo { get; set; }

        public string? direccion { get; set; }

        public DateOnly? fechaNacimiento { get; set; }

        public DateTime fechaRegistro { get; set; }

        public int visitasTotales { get; set; }

        public bool activo { get; set; }

        public int? idPrograma { get; set; }
        public string? ProgramaFidelizacion { get; set; }
        
        // Propiedades computadas
        public string NombreCompleto => $"{nombre} {apellidos}";
        public string Estado => activo ? "Activo" : "Inactivo";
        
        /// <summary>
        /// Indica si la pr├│xima visita ser├í gratis (visita n├║mero 5, 10, 15, etc.)
        /// </summary>
        public bool EsProximaVisitaGratis => (visitasTotales + 1) % 5 == 0 && visitasTotales > 0;
        
        public string celular { get; set; } = "";
    }
}
