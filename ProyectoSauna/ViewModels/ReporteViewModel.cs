using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using ProyectoSauna.Commands;
using ProyectoSauna.Interfaces;
using ProyectoSauna.Models.DTOs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProyectoSauna.ViewModels
{
    public class ReporteViewModel : BaseViewModel
    {
        private readonly IReporteService _reporteService;

        // Filtros
        private DateTime _fechaInicio;
        public DateTime FechaInicio
        {
            get => _fechaInicio;
            set { _fechaInicio = value; OnPropertyChanged(); }
        }

        private DateTime _fechaFin;
        public DateTime FechaFin
        {
            get => _fechaFin;
            set { _fechaFin = value; OnPropertyChanged(); }
        }

        // Gráficos y Datos - Ingresos
        public ISeries[] IngresosSeries { get; set; } = Array.Empty<ISeries>();
        public Axis[] IngresosXAxes { get; set; } = Array.Empty<Axis>();
        public ObservableCollection<ReporteIngresoDTO> ListaIngresos { get; set; } = new();

        // Gráficos y Datos - Egresos
        public ISeries[] EgresosSeries { get; set; } = Array.Empty<ISeries>();
        public ObservableCollection<ReporteEgresoDTO> ListaEgresos { get; set; } = new();

        // Datos - Top Productos
        public ObservableCollection<ReporteProductoDTO> TopProductos { get; set; } = new();

        // Datos - Mejores Clientes
        public ObservableCollection<ReporteClienteDTO> MejoresClientes { get; set; } = new();

        // Datos - Flujo Caja
        private FlujoCajaDTO _flujoCaja;
        public FlujoCajaDTO FlujoCaja
        {
            get => _flujoCaja;
            set { _flujoCaja = value; OnPropertyChanged(); }
        }

        // Comandos
        public ICommand GenerarReporteCommand { get; }
        public ICommand CargarTodosCommand { get; }

        public ReporteViewModel(IReporteService reporteService)
        {
            _reporteService = reporteService;

            // Fechas por defecto: Último mes completo
            var hoy = DateTime.Today;
            var primerDiaMesActual = new DateTime(hoy.Year, hoy.Month, 1);
            FechaInicio = primerDiaMesActual.AddMonths(-1); // primer día del mes anterior
            FechaFin = primerDiaMesActual.AddDays(-1);      // último día del mes anterior

            GenerarReporteCommand = new AsyncRelayCommand(_ => GenerarReporteAsync());
            CargarTodosCommand = new AsyncRelayCommand(_ => CargarTodoAlInicioAsync());
            
            // Inicializar FlujoCaja para evitar nulos en UI
            FlujoCaja = new FlujoCajaDTO();
        }

        private async Task CargarTodoAlInicioAsync()
        {
            await GenerarReporteAsync();
        }

        private async Task GenerarReporteAsync()
        {
            try
            {
                // 1. Ingresos (Gráfico de Líneas/Barras)
                var ingresos = await _reporteService.GetIngresosPorFechaAsync(FechaInicio, FechaFin);
                ListaIngresos = new ObservableCollection<ReporteIngresoDTO>(ingresos);
                OnPropertyChanged(nameof(ListaIngresos));

                IngresosSeries = new ISeries[]
                {
                    new ColumnSeries<decimal>
                    {
                        Values = ingresos.Select(x => x.Total).ToArray(),
                        Name = "Ingresos",
                        Fill = new SolidColorPaint(SKColors.CornflowerBlue)
                    }
                };
                
                IngresosXAxes = new Axis[]
                {
                    new Axis
                    {
                        Labels = ingresos.Select(x => x.Fecha.ToString("dd/MM")).ToArray(),
                        LabelsRotation = 45
                    }
                };
                
                OnPropertyChanged(nameof(IngresosSeries));
                OnPropertyChanged(nameof(IngresosXAxes));


                // 2. Egresos (Gráfico Circular) - Del mes de la FechaFin
                var egresos = await _reporteService.GetEgresosMensualesAsync(FechaFin.Month, FechaFin.Year);
                ListaEgresos = new ObservableCollection<ReporteEgresoDTO>(egresos);
                OnPropertyChanged(nameof(ListaEgresos));

                EgresosSeries = egresos.Select(e => new PieSeries<decimal>
                {
                    Values = new decimal[] { e.Total },
                    Name = e.TipoEgreso,
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle
                }).ToArray();
                OnPropertyChanged(nameof(EgresosSeries));


                // 3. Top Productos
                var productos = await _reporteService.GetTopProductosAsync(10);
                TopProductos = new ObservableCollection<ReporteProductoDTO>(productos);
                OnPropertyChanged(nameof(TopProductos));


                // 4. Mejores Clientes
                var clientes = await _reporteService.GetMejoresClientesAsync(10);
                MejoresClientes = new ObservableCollection<ReporteClienteDTO>(clientes);
                OnPropertyChanged(nameof(MejoresClientes));


                // 5. Flujo de Caja (Mes actual de FechaFin)
                FlujoCaja = await _reporteService.GetFlujoCajaAsync(FechaFin.Month, FechaFin.Year);
            }
            catch (Exception ex)
            {
                // En un caso real usaría DialogService
                System.Diagnostics.Debug.WriteLine($"Error generando reportes: {ex.Message}");
            }
        }
    }
}
