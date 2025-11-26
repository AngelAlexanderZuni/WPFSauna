using System;
using System.Windows;

namespace ProyectoSauna.Helpers
{
    /// <summary>
    /// Servicio para mostrar diálogos y mensajes al usuario
    /// </summary>
    public class DialogService
    {
        private static DialogService? _instance;
        public static DialogService Instance => _instance ??= new DialogService();

        private DialogService() { }

        /// <summary>
        /// Muestra un mensaje informativo
        /// </summary>
        public void ShowInformation(string message, string title = "Información")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Muestra un mensaje de advertencia
        /// </summary>
        public void ShowWarning(string message, string title = "Advertencia")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>
        /// Muestra un mensaje de error
        /// </summary>
        public void ShowError(string message, string title = "Error")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Muestra un mensaje de éxito
        /// </summary>
        public void ShowSuccess(string message, string title = "Éxito")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Muestra un diálogo de confirmación
        /// </summary>
        public bool ShowConfirmation(string message, string title = "Confirmación")
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

        /// <summary>
        /// Muestra un diálogo con opciones Sí/No/Cancelar
        /// </summary>
        public MessageBoxResult ShowYesNoCancel(string message, string title = "Confirmación")
        {
            return MessageBox.Show(message, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
        }
    }
}
