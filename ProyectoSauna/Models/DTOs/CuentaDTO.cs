using System;

namespace ProyectoSauna.Models.DTOs
{
    public class CuentaDTO
    {
        public int idCuenta { get; set; }
        public DateTime fechaHoraIngreso { get; set; }
        public DateTime? fechaHoraSalida { get; set; }
        public decimal precioEntrada { get; set; }
        public decimal subtotalConsumos { get; set; }
        public decimal descuento { get; set; }
        public decimal total { get; set; }
        public decimal montoPagado { get; set; }
        public decimal saldo { get; set; }
        public int idCliente { get; set; }
        public int idEstadoCuenta { get; set; }
        public int idUsuario { get; set; }

        public string NombreCliente { get; set; } = "";
        public string DocumentoCliente { get; set; } = "";
        public string EstadoCuenta { get; set; } = "";
        public string NombreUsuario { get; set; } = "";

        public string HoraIngreso => fechaHoraIngreso.ToString("HH:mm");
        public string FechaIngreso => fechaHoraIngreso.ToString("dd/MM/yyyy");
        
        public string TiempoTranscurrido
        {
            get
            {
                if (fechaHoraSalida.HasValue)
                    return "Finalizada";

                TimeSpan tiempo = DateTime.Now - fechaHoraIngreso;
                return $"{tiempo.Hours}h {tiempo.Minutes}m";
            }
        }

        public bool TieneDescuento => descuento > 0;
    }
}
