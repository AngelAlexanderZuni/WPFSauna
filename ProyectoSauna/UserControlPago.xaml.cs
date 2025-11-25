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
    /// L├│gica de interacci├│n para UserControlPago.xaml
    /// </summary>
    public partial class UserControlPago : UserControl
    {
        public UserControlPago()
        {
            InitializeComponent();

            // DI sencilla (igual patr├│n usado en otros controles)
            var db = new SaunaDbContext();
            IPagoRepository pagoRepo = new PagoRepository(db);
            IMetodoPagoRepository metodoRepo = new MetodoPagoRepository(db);
            ICuentaRepository cuentaRepo = new CuentaRepository();
            IPagoService pagoService = new PagoService(pagoRepo, metodoRepo, cuentaRepo);
            IMetodoPagoService metodoService = new MetodoPagoService(metodoRepo);

            var vm = new PagosViewModel(pagoService, metodoService);

            // Precarga IdCuenta si existe en propiedades globales
            try
            {
                if (Application.Current?.Properties != null)
                {
                    var props = Application.Current.Properties;
                    if (props.Contains("IdCuenta") && props["IdCuenta"] is int idCuenta)
                        vm.IdCuenta = idCuenta;
                }
            }
            catch { }

            DataContext = vm;

            // Notificaci├│n de pago exitoso
            vm.PagoCreado += () =>
            {
                if (PagoNotifDialog != null)
                {
                    PagoNotifDialog.IsOpen = true;
                    var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.8) };
                    timer.Tick += (s, e) =>
                    {
                        timer.Stop();
                        PagoNotifDialog.IsOpen = false;
                    };
                    timer.Start();
                }
            };
        }
    }
}
