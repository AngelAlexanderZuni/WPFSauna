using Microsoft.Win32;
using Microsoft.EntityFrameworkCore;
using MaterialDesignThemes.Wpf;
using ProyectoSauna.Models;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories;
using ProyectoSauna.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ProyectoSauna
{
    public partial class UserControlPago : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // Estado y listas
        private ObservableCollection<PagoVM> _pagos = new();
        private PagoVM? _pagoSeleccionado;
        private List<MetodoPago> _metodosPago = new();
        private MetodoPago? _metodoSeleccionado;
        private List<Cuenta> _cuentas = new();
        private Cuenta? _cuentaSeleccionada;
        private DateTime? _fechaDesde = DateTime.Today.AddDays(-7);
        private DateTime? _fechaHasta = DateTime.Today;
        private string _textoBusqueda = string.Empty;
        private decimal _totalPagos;
        private decimal _montoNuevo;

        public ObservableCollection<PagoVM> Pagos { get => _pagos; set { _pagos = value; OnPropertyChanged(nameof(Pagos)); } }
        public PagoVM? PagoSeleccionado { get => _pagoSeleccionado; set { _pagoSeleccionado = value; OnPropertyChanged(nameof(PagoSeleccionado)); } }
        public List<MetodoPago> MetodosPago { get => _metodosPago; set { _metodosPago = value; OnPropertyChanged(nameof(MetodosPago)); } }
        public MetodoPago? MetodoSeleccionado { get => _metodoSeleccionado; set { _metodoSeleccionado = value; OnPropertyChanged(nameof(MetodoSeleccionado)); } }
        public List<Cuenta> Cuentas { get => _cuentas; set { _cuentas = value; OnPropertyChanged(nameof(Cuentas)); } }
        public Cuenta? CuentaSeleccionada { get => _cuentaSeleccionada; set { _cuentaSeleccionada = value; OnPropertyChanged(nameof(CuentaSeleccionada)); } }
        public DateTime? FechaDesde { get => _fechaDesde; set { _fechaDesde = value; OnPropertyChanged(nameof(FechaDesde)); } }
        public DateTime? FechaHasta { get => _fechaHasta; set { _fechaHasta = value; OnPropertyChanged(nameof(FechaHasta)); } }
        public string TextoBusqueda { get => _textoBusqueda; set { _textoBusqueda = value; OnPropertyChanged(nameof(TextoBusqueda)); } }
        public decimal TotalPagos { get => _totalPagos; set { _totalPagos = value; OnPropertyChanged(nameof(TotalPagos)); } }
        public decimal MontoNuevo { get => _montoNuevo; set { _montoNuevo = value; OnPropertyChanged(nameof(MontoNuevo)); } }

        private readonly IPagoRepository _pagoRepo;

        public UserControlPago()
        {
            InitializeComponent();
            DataContext = this;
            _pagoRepo = new PagoRepository(new SaunaDbContext());
            Loaded += UserControlPago_Loaded;
        }

        private async void UserControlPago_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarCatalogosAsync();
            await CargarPagosAsync();
        }

        private async Task CargarCatalogosAsync()
        {
            using var dbContext = new SaunaDbContext();
            MetodosPago = await dbContext.MetodoPago.AsNoTracking().OrderBy(m => m.nombre).ToListAsync();
            // Proyectamos solo idCuenta para evitar columnas que no existen en la BD actual
            Cuentas = await dbContext.Cuenta.AsNoTracking()
                .Select(c => new Cuenta { idCuenta = c.idCuenta })
                .OrderByDescending(c => c.idCuenta)
                .Take(200)
                .ToListAsync();
        }

        private async Task CargarPagosAsync()
        {
            var pagos = await _pagoRepo.ObtenerConNavegacionAsync();
            var lista = pagos
                .OrderByDescending(p => p.fechaHora)
                .Select(p => new PagoVM
                {
                    idPago = p.idPago,
                    fechaHora = p.fechaHora,
                    monto = p.monto,
                    numeroReferencia = p.numeroReferencia,
                    idMetodoPago = p.idMetodoPago,
                    idCuenta = p.idCuenta,
                    nombreMetodo = p.idMetodoPagoNavigation?.nombre,
                    nombreCuenta = p.idCuentaNavigation?.idCuenta.ToString()
                })
                .ToList();

            if (FechaDesde.HasValue)
                lista = lista.Where(x => x.fechaHora >= FechaDesde.Value.Date).ToList();
            if (FechaHasta.HasValue)
                lista = lista.Where(x => x.fechaHora <= FechaHasta.Value.Date.AddDays(1).AddTicks(-1)).ToList();
            if (MetodoSeleccionado != null)
                lista = lista.Where(x => x.idMetodoPago == MetodoSeleccionado.idMetodoPago).ToList();
            if (!string.IsNullOrWhiteSpace(TextoBusqueda))
                lista = lista.Where(x => (x.numeroReferencia ?? string.Empty).Contains(TextoBusqueda.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

            Pagos = new ObservableCollection<PagoVM>(lista);
            TotalPagos = Pagos.Sum(x => x.monto);
        }

        private async void BtnCargar_Click(object sender, RoutedEventArgs e)
        {
            await CargarPagosAsync();
        }

        private async void BtnNuevo_Click(object sender, RoutedEventArgs e)
        {
            // Abre el diálogo de alta dentro de la misma vista
            MontoNuevo = 0;
            CuentaSeleccionada = Cuentas.FirstOrDefault();
            MetodoSeleccionado = MetodosPago.FirstOrDefault();
            await Task.Delay(1);
            DialogHost.OpenDialogCommand.Execute(null, DialogHostPago);
        }

        private async void BtnAgregarPago_Click(object sender, RoutedEventArgs e)
        {
            if (CuentaSeleccionada == null)
            {
                MessageBox.Show("Seleccione una cuenta.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (MetodoSeleccionado == null)
            {
                MessageBox.Show("Seleccione un método de pago.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (MontoNuevo <= 0)
            {
                MessageBox.Show("El monto debe ser mayor a 0.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var pago = new Pago
                {
                    fechaHora = DateTime.Now,
                    monto = MontoNuevo,
                    numeroReferencia = GenerarReferenciaStripeLike(),
                    idMetodoPago = MetodoSeleccionado.idMetodoPago,
                    idCuenta = CuentaSeleccionada.idCuenta
                };
                await _pagoRepo.CrearAsync(pago);

                DialogHost.CloseDialogCommand.Execute(null, DialogHostPago);
                await CargarPagosAsync();
                MessageBox.Show("Pago registrado.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar pago: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (PagoSeleccionado == null)
            {
                MessageBox.Show("Seleccione un pago para eliminar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var confirmar = MessageBox.Show("¿Eliminar el pago seleccionado?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirmar != MessageBoxResult.Yes) return;
            try
            {
                var ok = await _pagoRepo.EliminarAsync(PagoSeleccionado.idPago);
                if (!ok)
                {
                    MessageBox.Show("No se encontró el pago.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                await CargarPagosAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExportar_Click(object sender, RoutedEventArgs e)
        {
            if (Pagos == null || Pagos.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var sfd = new SaveFileDialog
            {
                Filter = "CSV (*.csv)|*.csv",
                FileName = $"pagos_{DateTime.Now:yyyyMMdd_HHmm}.csv"
            };
            if (sfd.ShowDialog() == true)
            {
                try
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Fecha,Cuenta,Método,Referencia,Monto");
                    foreach (var p in Pagos)
                    {
                        var linea = string.Join(",",
                            EscapeCsv(p.fechaHora.ToString("yyyy-MM-dd HH:mm")),
                            EscapeCsv(p.nombreCuenta ?? p.idCuenta.ToString()),
                            EscapeCsv(p.nombreMetodo ?? string.Empty),
                            EscapeCsv(p.numeroReferencia ?? string.Empty),
                            p.monto.ToString(System.Globalization.CultureInfo.InvariantCulture)
                        );
                        sb.AppendLine(linea);
                    }
                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Exportación completada.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al exportar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private static string EscapeCsv(string value)
        {
            if (value.Contains('"')) value = value.Replace("\"", "\"\"");
            if (value.Contains(',') || value.Contains('\n')) value = $"\"{value}\"";
            return value;
        }

        private static string GenerarReferenciaStripeLike()
        {
            // Similar a "pi_" + 24 chars alfanum minúscula
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var rnd = new Random();
            var suffix = new string(Enumerable.Range(0, 24).Select(_ => chars[rnd.Next(chars.Length)]).ToArray());
            return $"pi_{suffix}";
        }

        public class PagoVM
        {
            public int idPago { get; set; }
            public DateTime fechaHora { get; set; }
            public decimal monto { get; set; }
            public string? numeroReferencia { get; set; }
            public int idMetodoPago { get; set; }
            public int idCuenta { get; set; }
            public string? nombreMetodo { get; set; }
            public string? nombreCuenta { get; set; }
        }
    }
}
