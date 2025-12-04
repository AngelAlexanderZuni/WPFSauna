using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoSauna.Models.DTOs
{
    public class PagoDTO
    {
        public int idPago { get; set; }
        public DateTime fechaHora { get; set; }
        public decimal monto { get; set; }
        public string? numeroReferencia { get; set; }
        public int idMetodoPago { get; set; }
        public int idCuenta { get; set; }

        // Para listados
        public string? nombreMetodo { get; set; }
        public string? nombreCuenta { get; set; }
    }
}
