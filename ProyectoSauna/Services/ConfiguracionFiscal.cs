using System;

namespace ProyectoSauna.Services
{
    /// <summary>
    /// Configuración fiscal centralizada - Más simple que BD pero con trazabilidad
    /// </summary>
    public static class ConfiguracionFiscal
    {
        // 🏛️ IGV ACTUAL - Cambiar aquí cuando haya modificaciones legales
        public const decimal IGV_PORCENTAJE = 18.0m;
        public static readonly DateTime IGV_VIGENCIA_DESDE = new DateTime(2024, 1, 1);
        
        // 📜 Historial para auditoría (solo agregar, nunca modificar)
        public static readonly (DateTime desde, decimal porcentaje)[] HISTORIAL_IGV = 
        {
            (new DateTime(2024, 1, 1), 18.0m)
            // Si cambia IGV agregar: (new DateTime(2025, 6, 1), 19.0m)
        };

        /// <summary>
        /// Obtiene el IGV vigente para una fecha específica
        /// </summary>
        public static decimal ObtenerIGVVigente(DateTime? fecha = null)
        {
            var fechaConsulta = fecha ?? DateTime.Now;
            
            // Buscar en historial el IGV vigente para esa fecha
            decimal igvVigente = 18.0m; // Default
            foreach (var (desde, porcentaje) in HISTORIAL_IGV)
            {
                if (fechaConsulta >= desde)
                    igvVigente = porcentaje;
            }
            
            return igvVigente;
        }

        /// <summary>
        /// Calcula subtotal e IGV desde un total con impuestos incluidos
        /// </summary>
        public static (decimal subtotal, decimal igv) CalcularDesdeTotal(decimal totalConIgv, DateTime? fecha = null)
        {
            var porcentajeIgv = ObtenerIGVVigente(fecha);
            var factor = 1 + (porcentajeIgv / 100);
            
            var subtotal = Math.Round(totalConIgv / factor, 2);
            var igv = Math.Round(totalConIgv - subtotal, 2);
            
            return (subtotal, igv);
        }

        /// <summary>
        /// Agrega IGV a un subtotal
        /// </summary>
        public static decimal AgregarIGV(decimal subtotal, DateTime? fecha = null)
        {
            var porcentajeIgv = ObtenerIGVVigente(fecha);
            var igv = subtotal * (porcentajeIgv / 100);
            return Math.Round(subtotal + igv, 2);
        }
    }
}