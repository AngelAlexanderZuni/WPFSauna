using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoSauna.Models
{
    public static class SesionActual
    {
        public static int IdUsuario { get; set; }
        public static string NombreCompleto { get; set; } = string.Empty;
        public static string Rol { get; set; } = string.Empty;
        public static int CuentaSeleccionadaId { get; set; }

        public static bool EstaLogueado => IdUsuario > 0;

        public static void CerrarSesion()
        {
            IdUsuario = 0;
            NombreCompleto = string.Empty;
            Rol = string.Empty;
            CuentaSeleccionadaId = 0;
        }
    }
}
