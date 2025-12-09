// Models/DTOs/ClienteDTO.cs - CORREGIDO
using System;
using System.ComponentModel;

namespace ProyectoSauna.Models.DTOs
{
    public class ClienteDTO : INotifyPropertyChanged
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

        // ✅ PROPIEDAD PARA CONTROLAR HABILITACIÓN DEL RADIOBUTTON
        private bool _isRadioButtonEnabled = true;
        public bool IsRadioButtonEnabled
        {
            get => _isRadioButtonEnabled;
            set
            {
                _isRadioButtonEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRadioButtonEnabled)));
            }
        }

        public string NombreCompleto => $"{nombre} {apellidos}";
        public string Estado => activo ? "Activo" : "Inactivo";
        public string EstadoTexto => activo ? "✅ Activo" : "🚫 Inactivo";

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}