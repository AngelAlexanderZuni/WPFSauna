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
using ProyectoSauna.Services;
using ProyectoSauna.ViewModels;


namespace ProyectoSauna
{
    /// <summary>
    /// Lógica de interacción para UserControlReporte.xaml
    /// </summary>
    public partial class UserControlReporte : UserControl
    {
        private readonly SaunaDbContext _context;

        public UserControlReporte()
        {
            InitializeComponent();

            _context = new SaunaDbContext();
            var reporteService = new ReporteService(_context);
            DataContext = new ReporteViewModel(reporteService);

            Loaded += (_, __) =>
            {
                if (DataContext is ReporteViewModel vm)
                {
                    if (vm.CargarTodosCommand.CanExecute(null))
                        vm.CargarTodosCommand.Execute(null);
                }
            };

            Unloaded += (_, __) => _context.Dispose();
        }
    }
}
