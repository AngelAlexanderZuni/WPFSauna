using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ProyectoSauna.Models;
using ProyectoSauna.Repositories;
using ProyectoSauna.Services;
using ProyectoSauna.ViewModels;
using ProyectoSauna.Services.Interfaces;
using ProyectoSauna.Repositories.Interfaces;
using MaterialDesignThemes.Wpf;
using System.Windows.Threading;

namespace ProyectoSauna
{
    /// <summary>
    /// Lógica de interacción para UserControlEgresos.xaml
    /// </summary>
    public partial class UserControlEgresos : UserControl
    {
        public UserControlEgresos()
        {
            InitializeComponent();

            // TODO: Módulo Egresos temporalmente deshabilitado
            // Descomentar cuando se implemente completamente
            /*
            var context = new SaunaDbContext();
            var egresoRepo = new EgresoRepository(context);
            var tipoRepo = new TipoEgresoRepository(context);
            var service = new EgresoService(egresoRepo, tipoRepo);
            var usuarioRepo = new UsuarioRepository(context);
            IUsuarioService usuarioService = new UsuarioService(usuarioRepo);
            var vm = new EgresosViewModel(service, usuarioService);

            // Intentar obtener datos de usuario desde propiedades globales de la aplicación
            try
            {
                var props = Application.Current?.Properties;
                if (props != null)
                {
                    if (props.Contains("UserId") && props["UserId"] is int id)
                        vm.IdUsuario = id;
                    if (props.Contains("UsuarioId") && props["UsuarioId"] is int id2) // alias posible
                        vm.IdUsuario = vm.IdUsuario ?? id2;
                    if (props.Contains("UserName") && props["UserName"] is string nombre)
                        vm.UsuarioNombre = nombre;
                    if (props.Contains("UsuarioNombre") && props["UsuarioNombre"] is string nombre2)
                        vm.UsuarioNombre = string.IsNullOrWhiteSpace(vm.UsuarioNombre) ? nombre2 : vm.UsuarioNombre;
                }
            }
            catch { }

            DataContext = vm;

            // Suscribirse a notificación de actualización exitosa
            vm.ActualizacionExitosa += () =>
            {
                // Mostrar diálogo centrado y autocerrar
                if (NotifDialog != null)
                {
                    NotifDialog.IsOpen = true;
                    var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.8) };
                    timer.Tick += (s, e) =>
                    {
                        timer.Stop();
                        NotifDialog.IsOpen = false;
                    };
                    timer.Start();
                }
            };
            */
        }
    }
}
