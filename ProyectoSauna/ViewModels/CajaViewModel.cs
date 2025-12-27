using ProyectoSauna.Models.DTOs;
using ProyectoSauna.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using UiAsyncRelayCommand = ProyectoSauna.Commands.AsyncRelayCommand;
using UiRelayCommand = ProyectoSauna.Commands.RelayCommand;

namespace ProyectoSauna.ViewModels
{
    public class CajaViewModel : BaseViewModel
    {
        private readonly IPagoService _pagoService;
        private readonly IEgresoService _egresoService;

        private DateTime _fechaSeleccionada;
        private decimal _totalIngresos;
        private decimal _totalEgresos;
        private decimal _balance;
        private bool _isLoading;

        public ObservableCollection<MovimientoCajaDTO> Movimientos { get; } = new();

        public DateTime FechaSeleccionada
        {
            get => _fechaSeleccionada;
            set
            {
                if (SetProperty(ref _fechaSeleccionada, value))
                {
                    _ = CargarMovimientosAsync();
                }
            }
        }

        public decimal TotalIngresos
        {
            get => _totalIngresos;
            set => SetProperty(ref _totalIngresos, value);
        }

        public decimal TotalEgresos
        {
            get => _totalEgresos;
            set => SetProperty(ref _totalEgresos, value);
        }

        public decimal Balance
        {
            get => _balance;
            set => SetProperty(ref _balance, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand CargarMovimientosCommand { get; }
        public ICommand HoyCommand { get; }

        public CajaViewModel(IPagoService pagoService, IEgresoService egresoService)
        {
            _pagoService = pagoService;
            _egresoService = egresoService;

            _fechaSeleccionada = DateTime.Today;

            CargarMovimientosCommand = new UiAsyncRelayCommand(async _ => await CargarMovimientosAsync());
            HoyCommand = new UiRelayCommand(() => { FechaSeleccionada = DateTime.Today; });

            _ = CargarMovimientosAsync();
        }

        private async Task CargarMovimientosAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                Movimientos.Clear();

                var pagos = await _pagoService.GetPagosPorFechaAsync(FechaSeleccionada);
                var movimientosPagos = pagos.Select(p => new MovimientoCajaDTO
                {
                    FechaHora = p.fechaHora,
                    Concepto = $"Pago Cuenta #{p.idCuenta} - {p.metodoPagoNombre}",
                    Monto = p.monto,
                    Tipo = "Ingreso",
                    Color = "#4CAF50",
                    Usuario = "Sistema"
                });

                var egresos = await _egresoService.GetEgresosPorFechaAsync(FechaSeleccionada);
                var movimientosEgresos = egresos.Select(e => new MovimientoCajaDTO
                {
                    FechaHora = e.fecha,
                    Concepto = $"Egreso #{e.idCabEgreso}",
                    Monto = e.montoTotal,
                    Tipo = "Egreso",
                    Color = "#EF4444",
                    Usuario = e.idUsuario?.ToString() ?? "N/A"
                });

                var todos = movimientosPagos.Concat(movimientosEgresos)
                                            .OrderByDescending(m => m.FechaHora)
                                            .ToList();

                foreach (var mov in todos)
                {
                    Movimientos.Add(mov);
                }

                TotalIngresos = movimientosPagos.Sum(m => m.Monto);
                TotalEgresos = movimientosEgresos.Sum(m => m.Monto);
                Balance = TotalIngresos - TotalEgresos;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar movimientos de caja: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
