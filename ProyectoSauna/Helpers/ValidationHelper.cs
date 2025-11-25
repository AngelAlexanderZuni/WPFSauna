using System;
using System.Text.RegularExpressions;

namespace ProyectoSauna.Helpers
{
    /// <summary>
    /// Clase para validaciones comunes (RNF17)
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Valida que un texto no esté vacío
        /// </summary>
        public static bool ValidarTextoRequerido(string? texto, out string mensajeError)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                mensajeError = "Este campo es obligatorio.";
                return false;
            }
            mensajeError = string.Empty;
            return true;
        }

        /// <summary>
        /// Valida formato de DNI peruano (8 dígitos)
        /// </summary>
        public static bool ValidarDNI(string? dni, out string mensajeError)
        {
            if (string.IsNullOrWhiteSpace(dni))
            {
                mensajeError = "El DNI es obligatorio.";
                return false;
            }

            if (!Regex.IsMatch(dni, @"^\d{8}$"))
            {
                mensajeError = "El DNI debe tener 8 dígitos.";
                return false;
            }

            mensajeError = string.Empty;
            return true;
        }

        /// <summary>
        /// Valida formato de RUC peruano (11 dígitos)
        /// </summary>
        public static bool ValidarRUC(string? ruc, out string mensajeError)
        {
            if (string.IsNullOrWhiteSpace(ruc))
            {
                mensajeError = "El RUC es obligatorio.";
                return false;
            }

            if (!Regex.IsMatch(ruc, @"^\d{11}$"))
            {
                mensajeError = "El RUC debe tener 11 dígitos.";
                return false;
            }

            mensajeError = string.Empty;
            return true;
        }

        /// <summary>
        /// Valida formato de correo electrónico
        /// </summary>
        public static bool ValidarEmail(string? email, out string mensajeError)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                mensajeError = "El correo es obligatorio.";
                return false;
            }

            // Patrón simplificado de email
            var patron = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(email, patron))
            {
                mensajeError = "El formato del correo no es válido.";
                return false;
            }

            mensajeError = string.Empty;
            return true;
        }

        /// <summary>
        /// Valida formato de teléfono (9 dígitos, opcionalmente con prefijo)
        /// </summary>
        public static bool ValidarTelefono(string? telefono, out string mensajeError)
        {
            if (string.IsNullOrWhiteSpace(telefono))
            {
                mensajeError = "El teléfono es obligatorio.";
                return false;
            }

            // Acepta: 999999999 o +51999999999
            if (!Regex.IsMatch(telefono, @"^(\+51)?\d{9}$"))
            {
                mensajeError = "El teléfono debe tener 9 dígitos.";
                return false;
            }

            mensajeError = string.Empty;
            return true;
        }

        /// <summary>
        /// Valida que un número sea positivo
        /// </summary>
        public static bool ValidarNumeroPositivo(decimal numero, string nombreCampo, out string mensajeError)
        {
            if (numero <= 0)
            {
                mensajeError = $"{nombreCampo} debe ser mayor a cero.";
                return false;
            }

            mensajeError = string.Empty;
            return true;
        }

        /// <summary>
        /// Valida que un número esté en un rango
        /// </summary>
        public static bool ValidarRango(decimal numero, decimal min, decimal max, string nombreCampo, out string mensajeError)
        {
            if (numero < min || numero > max)
            {
                mensajeError = $"{nombreCampo} debe estar entre {min} y {max}.";
                return false;
            }

            mensajeError = string.Empty;
            return true;
        }

        /// <summary>
        /// Valida que una fecha no sea futura
        /// </summary>
        public static bool ValidarFechaNoFutura(DateTime fecha, string nombreCampo, out string mensajeError)
        {
            if (fecha > DateTime.Now)
            {
                mensajeError = $"{nombreCampo} no puede ser una fecha futura.";
                return false;
            }

            mensajeError = string.Empty;
            return true;
        }

        /// <summary>
        /// Valida longitud mínima de texto
        /// </summary>
        public static bool ValidarLongitudMinima(string? texto, int longitudMinima, string nombreCampo, out string mensajeError)
        {
            if (string.IsNullOrWhiteSpace(texto) || texto.Length < longitudMinima)
            {
                mensajeError = $"{nombreCampo} debe tener al menos {longitudMinima} caracteres.";
                return false;
            }

            mensajeError = string.Empty;
            return true;
        }
    }
}
