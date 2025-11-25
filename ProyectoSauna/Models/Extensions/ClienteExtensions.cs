// Models/Extensions/ClienteExtensions.cs - CORREGIDO
using ProyectoSauna.Models.Entities;
using System;

namespace ProyectoSauna.Models.Extensions
{
    public static class ClienteExtensions
    {
        /// <summary>
        /// Verifica si hoy es el cumpleaños del cliente
        /// </summary>
        public static bool EsCumpleanosHoy(this Cliente cliente)
        {
            if (cliente.fechaNacimiento == null)
                return false;

            var hoy = DateTime.Today;
            var fechaNac = cliente.fechaNacimiento.Value;

            return fechaNac.Month == hoy.Month && fechaNac.Day == hoy.Day;
        }

        /// <summary>
        /// Verifica si el cumpleaños está dentro de un rango de días
        /// </summary>
        public static bool EsCumpleanosEnRango(this Cliente cliente, int diasRango)
        {
            if (cliente.fechaNacimiento == null)
                return false;

            var hoy = DateTime.Today;
            var fechaNac = cliente.fechaNacimiento.Value;
            var cumpleEsteAnio = new DateTime(hoy.Year, fechaNac.Month, fechaNac.Day);

            var diasDiferencia = Math.Abs((cumpleEsteAnio - hoy).Days);
            return diasDiferencia <= diasRango;
        }

        /// <summary>
        /// Calcula la edad actual del cliente
        /// </summary>
        public static int? ObtenerEdad(this Cliente cliente)
        {
            if (cliente.fechaNacimiento == null)
                return null;

            var hoy = DateTime.Today;
            var fechaNac = cliente.fechaNacimiento.Value;
            var edad = hoy.Year - fechaNac.Year;

            if (new DateTime(hoy.Year, fechaNac.Month, fechaNac.Day) > hoy)
                edad--;

            return edad;
        }

        /// <summary>
        /// Obtiene el nombre completo del cliente
        /// </summary>
        public static string ObtenerNombreCompleto(this Cliente cliente)
        {
            return $"{cliente.nombre} {cliente.apellidos}".Trim();
        }

        /// <summary>
        /// Verifica si el cliente cumple con un número mínimo de visitas
        /// </summary>
        public static bool CumpleVisitasMinimas(this Cliente cliente, int visitasRequeridas)
        {
            return cliente.visitasTotales >= visitasRequeridas;
        }

        /// <summary>
        /// Obtiene información básica del cliente para mostrar
        /// </summary>
        public static string ObtenerInfoBasica(this Cliente cliente)
        {
            var info = $"{cliente.ObtenerNombreCompleto()}\n";
            info += $"Visitas totales: {cliente.visitasTotales}\n";

            if (cliente.fechaNacimiento != null)
            {
                var edad = cliente.ObtenerEdad();
                info += $"Edad: {edad} años\n";

                if (cliente.EsCumpleanosHoy())
                {
                    info += "🎂 ¡Hoy es su cumpleaños!";
                }
            }

            return info;
        }

        /// <summary>
        /// Formatea el teléfono del cliente
        /// </summary>
        public static string ObtenerTelefonoFormateado(this Cliente cliente)
        {
            if (string.IsNullOrWhiteSpace(cliente.telefono))
                return "Sin teléfono";

            return cliente.telefono.Trim();
        }

        /// <summary>
        /// Formatea el número de documento del cliente
        /// </summary>
        public static string ObtenerDocumentoFormateado(this Cliente cliente)
        {
            if (string.IsNullOrWhiteSpace(cliente.numero_documento))
                return "Sin documento";

            return cliente.numero_documento.Trim();
        }

        /// <summary>
        /// Verifica si el cliente tiene información de contacto completa
        /// </summary>
        public static bool TieneInfoContactoCompleta(this Cliente cliente)
        {
            return !string.IsNullOrWhiteSpace(cliente.telefono) &&
                   !string.IsNullOrWhiteSpace(cliente.numero_documento);
        }

        /// <summary>
        /// Verifica si el cliente está activo
        /// </summary>
        public static bool EstaActivo(this Cliente cliente)
        {
            return cliente.activo;
        }
    }
}