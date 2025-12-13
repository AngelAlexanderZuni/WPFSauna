using ProyectoSauna.Models.Entities;
using ProyectoSauna.ViewModels;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System;
using System.Linq;

namespace ProyectoSauna
{
    public partial class MainWindow : Window
    {
        private string _rol;
        private string _usuario;
        private UserControl _currentUserControl; // Para rastrear y limpiar el UserControl actual
        
        private void LimpiarModuloAnterior()
        {
            if (_currentUserControl != null)
            {
                try
                {
                    // Si el control actual implementa IDisposable, llamamos Dispose
                    if (_currentUserControl is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                    
                    // Para módulos específicos con métodos de limpieza de formulario
                    if (_currentUserControl is UserControlClientes clientesControl)
                    {
                        var viewModel = clientesControl.DataContext as ClientesViewModel;
                        viewModel?.LimpiarFormulario();
                    }
                    else if (_currentUserControl is UserControlPago pagosControl)
                    {
                        var viewModel = pagosControl.DataContext as PagosViewModel;
                        viewModel?.LimpiarFormulario();
                    }
                    else if (_currentUserControl is UserControlEgresos egresosControl)
                    {
                        var viewModel = egresosControl.DataContext as EgresosViewModel;
                        // EgresosViewModel tiene LimpiarFormulario() privado, pero Dispose debería manejarlo
                    }
                    
                    // Para otros módulos que implementen IDisposable, el Dispose() ya se llamó arriba
                    
                }
                catch (Exception ex)
                {
                    // Log del error pero no interrumpir el flujo
                    System.Diagnostics.Debug.WriteLine($"Error limpiando módulo anterior: {ex.Message}");
                }
            }
        }

        public MainWindow(string rol, string usuario)
        {
            InitializeComponent();
            _rol = rol;
            _usuario = usuario;

            ConfigurarPermisos();
        }

        private void ConfigurarPermisos()
        {
            this.Title = $"Panel Administrativo - {_usuario} ({_rol})";

            if (_rol == "Administrador")
            {
                // Habilita todos los menús
                HabilitarTodosLosBotones(MenuOperaciones, true);
                HabilitarTodosLosBotones(MenuFinanzas, true);
                HabilitarTodosLosBotones(MenuReportes, true);
                HabilitarTodosLosBotones(MenuConfiguracion, true);
            }
            else if (_rol == "Cajero")
            {
                string[] modulosPermitidos = {
                                                "Cuentas y Consumos",
                                                "Pagos y Comprobantes",
                                                "Clientes"
        };

                // Deshabilita todo
                HabilitarTodosLosBotones(MenuOperaciones, false);
                HabilitarTodosLosBotones(MenuFinanzas, false);
                HabilitarTodosLosBotones(MenuReportes, false);
                HabilitarTodosLosBotones(MenuConfiguracion, false);

                // Habilita solo los permitidos
                HabilitarBotonesPorNombre(MenuOperaciones, modulosPermitidos);
                HabilitarBotonesPorNombre(MenuFinanzas, modulosPermitidos);
                HabilitarBotonesPorNombre(MenuReportes, modulosPermitidos);
                HabilitarBotonesPorNombre(MenuConfiguracion, modulosPermitidos);
            }
        }

        private void HabilitarTodosLosBotones(StackPanel menu, bool estado)
        {
            foreach (var child in menu.Children.OfType<Button>())
                child.IsEnabled = estado;
        }

        private void HabilitarBotonesPorNombre(StackPanel menu, string[] nombres)
        {
            foreach (var child in menu.Children.OfType<Button>())
            {
                var texto = (child.Content as StackPanel)?
                            .Children.OfType<TextBlock>()
                            .FirstOrDefault()?.Text;

                if (texto != null && nombres.Contains(texto))
                    child.IsEnabled = true;
            }
        }

        private void SidebarButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button boton)
            {
                var texto = (boton.Content as StackPanel)?
                            .Children.OfType<TextBlock>()
                            .FirstOrDefault()?.Text;

                if (texto == null) return;

                TituloModulo.Text = $"Panel de Control - {texto}";
                PantallaBienvenida.Visibility = Visibility.Collapsed;

                // 🧹 LIMPIAR MÓDULO ANTERIOR ANTES DE CAMBIAR
                LimpiarModuloAnterior();

                // Carga del módulo correspondiente
                switch (texto)
                {
                    case "Cuentas y Consumos":
                        _currentUserControl = new UserControlCuentas();
                        ContenidoPrincipal.Content = _currentUserControl;
                        break;
                    case "Consumo":
                        MessageBox.Show("Módulo de Consumos en desarrollo.\nSe implementará junto con Cuentas.", "En Desarrollo", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                    case "Pagos y Comprobantes":
                        _currentUserControl = new UserControlPago();
                        ContenidoPrincipal.Content = _currentUserControl;
                        break;
                    case "Clientes":
                        _currentUserControl = new UserControlClientes();
                        ContenidoPrincipal.Content = _currentUserControl;
                        break;
                    case "Reportes y Estadísticas":
                        _currentUserControl = new UserControlReporte();
                        ContenidoPrincipal.Content = _currentUserControl;
                        break;
                    case "Caja y Flujo de Caja":
                        _currentUserControl = new UserControlCaja();
                        ContenidoPrincipal.Content = _currentUserControl;
                        break;
                    case "Inventario":
                        _currentUserControl = new UserControlInventario();
                        ContenidoPrincipal.Content = _currentUserControl;
                        break;
                    case "Servicios":
                        _currentUserControl = new UserControlServicios();
                        ContenidoPrincipal.Content = _currentUserControl;
                        break;
                    case "Egresos":
                        _currentUserControl = new UserControlEgresos();
                        ContenidoPrincipal.Content = _currentUserControl;
                        break;
                    case "Promociones":
                        _currentUserControl = new UserControlPromociones();
                        ContenidoPrincipal.Content = _currentUserControl;
                        break;
                    case "Usuarios":
                        _currentUserControl = new UserControlUsuarios();
                        ContenidoPrincipal.Content = _currentUserControl;
                        break;
                    default:
                        _currentUserControl = null;
                        ContenidoPrincipal.Content = null;
                        PantallaBienvenida.Visibility = Visibility.Visible;
                        break;
                }
            }
        }

        // 🔧 MÉTODO PÚBLICO PARA NAVEGACIÓN PROGRAMÁTICA
        public void CambiarAModulo(string nombreModulo)
        {
            try
            {
                TituloModulo.Text = $"Panel de Control - {nombreModulo}";
                PantallaBienvenida.Visibility = Visibility.Collapsed;

                // 🧹 LIMPIAR MÓDULO ANTERIOR ANTES DE CAMBIAR
                LimpiarModuloAnterior();

                // Carga del módulo correspondiente
                switch (nombreModulo)
                {
                    case "Cuentas y Consumos":
                        _currentUserControl = new UserControlCuentas();
                        ContenidoPrincipal.Content = _currentUserControl;
                        break;
                    case "Consumo":
                        MessageBox.Show("Módulo de Consumos en desarrollo.\nSe implementará junto con Cuentas.", "En Desarrollo", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                    case "Pagos y Comprobantes":
                        _currentUserControl = new UserControlPago();
                        ContenidoPrincipal.Content = _currentUserControl;
                        break;
                    case "Comprobantes": // Added
                        _currentUserControl = new UserControlComprobantes();
                        ContenidoPrincipal.Content = _currentUserControl;
                        break;
                    case "Clientes":
                        _currentUserControl = new UserControlClientes();
                        ContenidoPrincipal.Content = _currentUserControl;
                        break;
                    case "Reportes y Estadísticas":
                        ContenidoPrincipal.Content = new UserControlReporte();
                        break;
                    case "Caja y Flujo de Caja":
                        ContenidoPrincipal.Content = new UserControlCaja();
                        break;
                    case "Inventario":
                        ContenidoPrincipal.Content = new UserControlInventario();
                        break;
                    case "Servicios":
                        ContenidoPrincipal.Content = new UserControlServicios();
                        break;
                    case "Egresos":
                        ContenidoPrincipal.Content = new UserControlEgresos();
                        break;
                    case "Promociones":
                        ContenidoPrincipal.Content = new UserControlPromociones();
                        break;
                    case "Usuarios":
                        ContenidoPrincipal.Content = new UserControlUsuarios();
                        break;
                    default:
                        ContenidoPrincipal.Content = null;
                        PantallaBienvenida.Visibility = Visibility.Visible;
                        break;
                }
                
                System.Diagnostics.Debug.WriteLine($"✅ Módulo cambiado exitosamente a: {nombreModulo}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERROR al cambiar módulo a {nombreModulo}: {ex.Message}");
                throw;
            }
        }

        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("¿Deseas cerrar sesión y volver al login?", "Confirmar cierre de sesión",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                // Abrir ventana de login
                LoginSauna loginWindow = new LoginSauna();
                loginWindow.Show();

                // Cerrar ventana actual
                this.Close();
            }
        }

        // Métodos para los botones de la barra de título
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ClickCount == 2)
                {
                    // Doble clic: Maximizar/Restaurar
                    BtnMaximizar_Click(sender, e);
                }
                else if (e.ChangedButton == MouseButton.Left)
                {
                    // Click simple: Permitir arrastrar la ventana
                    this.DragMove();
                }
            }
            catch (Exception)
            {
                // Ignorar excepciones de DragMove cuando la ventana está maximizada
            }
        }

        private void BtnMinimizar_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void BtnMaximizar_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("¿Deseas cerrar la aplicación completamente?", "Confirmar salida",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
    }
}
