using ProyectoSauna.Commands;
using ProyectoSauna.Models.DTOs;
using ProyectoSauna.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProyectoSauna.ViewModels
{
    public class ComprobantesViewModel : BaseViewModel
    {
        private readonly IComprobanteService _comprobanteService;
        private readonly ICuentaService _cuentaService;
        private readonly IPagoService _pagoService;

        // Listas
        public ObservableCollection<PagoDTO> Pagos { get; } = new();
        public ObservableCollection<CuentaDetalleDTO> DetallesConsumo { get; } = new();
        public ObservableCollection<ComprobanteDTO> Comprobantes { get; } = new();
        public ObservableCollection<CuentaDTO> CuentasPendientes { get; } = new();
        public ObservableCollection<TipoComprobanteDTO> TiposComprobante { get; } = new();

        // Cuenta seleccionada
        private int _idCuenta;
        public int IdCuenta
        {
            get => _idCuenta;
            set
            {
                _idCuenta = value;
                OnPropertyChanged();
            }
        }

        // Informacion de cuenta
        private string _nombreCliente = "";
        public string NombreCliente { get => _nombreCliente; set { _nombreCliente = value; OnPropertyChanged(); } }

        private string _documentoCliente = "";
        public string DocumentoCliente { get => _documentoCliente; set { _documentoCliente = value; OnPropertyChanged(); } }

        private decimal _totalCuenta;
        public decimal TotalCuenta { get => _totalCuenta; set { _totalCuenta = value; OnPropertyChanged(); } }

        private decimal _totalPagado;
        public decimal TotalPagado { get => _totalPagado; set { _totalPagado = value; OnPropertyChanged(); } }

        private decimal _saldoPendiente;
        public decimal SaldoPendiente { get => _saldoPendiente; set { _saldoPendiente = value; OnPropertyChanged(); } }

        // Formulario de comprobante
        private bool _isDialogComprobanteOpen;
        public bool IsDialogComprobanteOpen { get => _isDialogComprobanteOpen; set { _isDialogComprobanteOpen = value; OnPropertyChanged(); } }

        private TipoComprobanteDTO? _selectedTipoComprobante;
        public TipoComprobanteDTO? SelectedTipoComprobante { get => _selectedTipoComprobante; set { _selectedTipoComprobante = value; OnPropertyChanged(); } }

        private string _serie = "F001";
        public string Serie { get => _serie; set { _serie = value; OnPropertyChanged(); } }

        private string _numero = "";
        public string Numero { get => _numero; set { _numero = value; OnPropertyChanged(); } }

        private DateTime _fechaEmision = DateTime.Now;
        public DateTime FechaEmision { get => _fechaEmision; set { _fechaEmision = value; OnPropertyChanged(); } }

        private decimal _subtotal;
        public decimal Subtotal { get => _subtotal; set { _subtotal = value; OnPropertyChanged(); } }

        private decimal _igv;
        public decimal Igv { get => _igv; set { _igv = value; OnPropertyChanged(); } }

        private decimal _total;
        public decimal Total { get => _total; set { _total = value; OnPropertyChanged(); } }

        // Comandos
        public ICommand CargarDatosCuentaCommand { get; }
        public ICommand NuevoComprobanteCommand { get; }
        public ICommand GuardarComprobanteCommand { get; }
        public ICommand CancelarComprobanteCommand { get; }

        public ComprobantesViewModel(IComprobanteService comprobanteService, ICuentaService cuentaService, IPagoService pagoService)
        {
            _comprobanteService = comprobanteService;
            _cuentaService = cuentaService;
            _pagoService = pagoService;

            CargarDatosCuentaCommand = new AsyncRelayCommand(async _ => await CargarDatosCuentaAsync());
            NuevoComprobanteCommand = new AsyncRelayCommand(async _ => await PrepararNuevoComprobanteAsync());
            GuardarComprobanteCommand = new AsyncRelayCommand(async _ => await GuardarComprobanteAsync());
            CancelarComprobanteCommand = new RelayCommand(() => IsDialogComprobanteOpen = false);

            _ = InicializarAsync();
        }

        private async Task InicializarAsync()
        {
            await CargarTiposComprobanteAsync();
        }

        private async Task CargarTiposComprobanteAsync()
        {
            var tipos = await _comprobanteService.GetTiposComprobanteAsync();
            TiposComprobante.Clear();
            foreach (var t in tipos) TiposComprobante.Add(t);
        }

        public async Task CargarDatosCuentaAsync()
        {
            if (IdCuenta <= 0)
            {
                LimpiarDatos();
                return;
            }

            try
            {
                // Cargar información de la cuenta
                var cuenta = await _cuentaService.GetByIdAsync(IdCuenta);
                if (cuenta == null)
                {
                    MessageBox.Show("No se encontró la cuenta especificada.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    LimpiarDatos();
                    return;
                }

                NombreCliente = cuenta.NombreCliente;
                DocumentoCliente = cuenta.DocumentoCliente;
                TotalCuenta = cuenta.total;
                TotalPagado = cuenta.montoPagado;
                SaldoPendiente = cuenta.saldo;

                // Cargar pagos
                var pagos = await _pagoService.GetPagosPorCuentaAsync(IdCuenta);
                Pagos.Clear();
                foreach (var p in pagos) Pagos.Add(p);

                // Cargar detalles de consumo (asumiendo que existe un servicio para esto)
                // Si no existe, puedes dejarlo vacío por ahora
                DetallesConsumo.Clear();
                // var detalles = await _detalleConsumoService.GetByCuentaIdAsync(IdCuenta);
                // foreach (var d in detalles) DetallesConsumo.Add(d);

                // Cargar comprobantes ya emitidos para esta cuenta
                var comprobantes = await _comprobanteService.GetByCuentaIdAsync(IdCuenta);
                Comprobantes.Clear();
                foreach (var c in comprobantes) Comprobantes.Add(c);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task PrepararNuevoComprobanteAsync()
        {
            if (IdCuenta <= 0)
            {
                MessageBox.Show("Debe ingresar un ID de cuenta válido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            LimpiarFormularioComprobante();

            // Calcular datos del comprobante desde la cuenta
            Total = TotalCuenta;
            Subtotal = Math.Round(Total / 1.18m, 2);
            Igv = Total - Subtotal;

            // Generar número correlativo simple
            Numero = DateTime.Now.ToString("yyyyMMddHHmmss");

            IsDialogComprobanteOpen = true;
        }

        private async Task GuardarComprobanteAsync()
        {
            if (IdCuenta <= 0 || SelectedTipoComprobante == null)
            {
                MessageBox.Show("Debe seleccionar un tipo de comprobante.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var nuevo = new ComprobanteDTO
                {
                    serie = Serie,
                    numero = Numero,
                    fechaEmision = FechaEmision,
                    subtotal = Subtotal,
                    igv = Igv,
                    total = Total,
                    idTipoComprobante = SelectedTipoComprobante.idTipoComprobante,
                    idCuenta = IdCuenta
                };

                await _comprobanteService.CreateAsync(nuevo);

                IsDialogComprobanteOpen = false;
                await CargarDatosCuentaAsync(); // Recargar para actualizar lista de comprobantes
                MessageBox.Show("Comprobante generado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar comprobante: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LimpiarFormularioComprobante()
        {
            SelectedTipoComprobante = null;
            Serie = "F001";
            Numero = "";
            FechaEmision = DateTime.Now;
            Subtotal = 0;
            Igv = 0;
            Total = 0;
        }

        private void LimpiarDatos()
        {
            NombreCliente = "";
            DocumentoCliente = "";
            TotalCuenta = 0;
            TotalPagado = 0;
            SaldoPendiente = 0;
            Pagos.Clear();
            DetallesConsumo.Clear();
            Comprobantes.Clear();
        }
    }
}
