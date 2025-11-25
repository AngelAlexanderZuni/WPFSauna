using System;
using System.Collections.Generic;

namespace ProyectoSauna.Models.Entities;

public partial class Cliente
{
    public int idCliente { get; set; }

    public string nombre { get; set; } = null!;

    public string apellidos { get; set; } = null!;

    public string? numero_documento { get; set; }

    public string? telefono { get; set; }

    public string? correo { get; set; }

    public string? direccion { get; set; }

    public DateOnly? fechaNacimiento { get; set; }

    public DateTime fechaRegistro { get; set; }

    public int visitasTotales { get; set; }

    public bool activo { get; set; }

    public int? idPrograma { get; set; }

    public virtual ICollection<Cuenta> Cuenta { get; set; } = new List<Cuenta>();

    public virtual ProgramaFidelizacion idProgramaNavigation { get; set; } = null!;
}
