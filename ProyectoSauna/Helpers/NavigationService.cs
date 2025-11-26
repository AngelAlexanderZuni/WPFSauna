using System;
using System.Windows;
using System.Windows.Controls;

namespace ProyectoSauna.Helpers
{
    /// <summary>
    /// Servicio de navegación para cambiar entre vistas
    /// </summary>
    public class NavigationService
    {
        private static NavigationService? _instance;
        private ContentControl? _contentControl;

        public static NavigationService Instance => _instance ??= new NavigationService();

        private NavigationService() { }

        /// <summary>
        /// Registra el contenedor principal donde se mostrarán las vistas
        /// </summary>
        public void RegisterContentControl(ContentControl contentControl)
        {
            _contentControl = contentControl;
        }

        /// <summary>
        /// Navega a una vista específica
        /// </summary>
        public void NavigateTo<T>() where T : UserControl, new()
        {
            if (_contentControl == null)
            {
                throw new InvalidOperationException("ContentControl no registrado. Llame a RegisterContentControl primero.");
            }

            var view = new T();
            _contentControl.Content = view;
        }

        /// <summary>
        /// Navega a una vista con un ViewModel específico
        /// </summary>
        public void NavigateTo<TView, TViewModel>(TViewModel viewModel) 
            where TView : UserControl, new()
        {
            if (_contentControl == null)
            {
                throw new InvalidOperationException("ContentControl no registrado. Llame a RegisterContentControl primero.");
            }

            var view = new TView();
            view.DataContext = viewModel;
            _contentControl.Content = view;
        }
    }
}
