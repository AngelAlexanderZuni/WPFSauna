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

namespace ProyectoSauna
{
    /// <summary>
    /// Lógica de interacción para UserControlCaja.xaml
    /// </summary>
    public partial class UserControlCaja : UserControl
    {
        private readonly SaunaDbContext _context;

        public UserControlCaja()
        {
            InitializeComponent();

            _context = new SaunaDbContext();

            var pagoRepo = new PagoRepository(_context);
            var metodoRepo = new MetodoPagoRepository(_context);
            var cuentaRepo = new CuentaRepository();
            var comprobanteRepo = new ComprobanteRepository(_context);
            var pagoService = new PagoService(pagoRepo, metodoRepo, cuentaRepo, comprobanteRepo, _context);

            var egresoRepo = new EgresoRepository(_context);
            var tipoEgresoRepo = new TipoEgresoRepository(_context);
            var egresoService = new EgresoService(egresoRepo, tipoEgresoRepo);

            DataContext = new CajaViewModel(pagoService, egresoService);

            Unloaded += (_, __) => _context.Dispose();
        }
    }
}
