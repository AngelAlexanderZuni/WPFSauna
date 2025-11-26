using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProyectoSauna.ViewModels
{
    public class ConsumoItem : INotifyPropertyChanged
    {
        public int IdDetalle { get; set; }
        public string Tipo { get; set; } // "PROD" o "SERV"
        public string NombreItem { get; set; }
        public int cantidad { get; set; }
        public decimal precioUnitario { get; set; }
        public decimal subtotal { get; set; }
        public int IdReferencia { get; set; } // idProducto o idServicio
        public int IdCuenta { get; set; }

        // ✅ Propiedad para el color del badge según el tipo
        public string TipoColor
        {
            get
            {
                return Tipo == "PROD" ? "#4CC9F0" : "#8B5CF6";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}