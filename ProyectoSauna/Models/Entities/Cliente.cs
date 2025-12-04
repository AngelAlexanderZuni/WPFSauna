// Models/Entities/Cliente.cs - ÚNICO Y COMPLETO
using System;
using System.Collections.Generic;

namespace ProyectoSauna.Models.Entities
{
    public partial class Cliente
    {
        public int idCliente { get; set; }
        public string nombre { get; set; } = string.Empty;
        public string apellidos { get; set; } = string.Empty;
        public string? numero_documento { get; set; }
        public string? telefono { get; set; }
        public string? correo { get; set; }
        public string? direccion { get; set; }
        public DateTime? fechaNacimiento { get; set; }
        public DateTime fechaRegistro { get; set; }
        public int visitasTotales { get; set; }
        public bool activo { get; set; }

        public virtual ICollection<Cuenta> Cuenta { get; set; } = new List<Cuenta>();
    }
}
