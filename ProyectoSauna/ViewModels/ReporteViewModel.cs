using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ProyectoSauna.Commands;
using ProyectoSauna.Helpers;
using ProyectoSauna.Interfaces;
using ProyectoSauna.Models.DTOs;
using ProyectoSauna.Services;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows.Input;

namespace ProyectoSauna.ViewModels
{
    public class ReporteViewModel : BaseViewModel
    {
        private readonly IReporteService _reporteService;

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

        public ISeries[] IngresosSeries { get; set; } = Array.Empty<ISeries>();
        public Axis[] IngresosXAxes { get; set; } = Array.Empty<Axis>();
        public ObservableCollection<ReporteIngresoDTO> ListaIngresos { get; set; } = new();

        public ISeries[] EgresosSeries { get; set; } = Array.Empty<ISeries>();
        public ObservableCollection<ReporteEgresoDTO> ListaEgresos { get; set; } = new();

        public ObservableCollection<ReporteProductoDTO> TopProductos { get; set; } = new();
        public ObservableCollection<ReporteClienteDTO> MejoresClientes { get; set; } = new();

        private FlujoCajaDTO _flujoCaja;
        public FlujoCajaDTO FlujoCaja
        {
            get => _flujoCaja;
            set { _flujoCaja = value; OnPropertyChanged(); }
        }

        public ICommand GenerarReporteCommand { get; }
        public ICommand CargarTodosCommand { get; }

        public ReporteViewModel(IReporteService reporteService)
        {
            _reporteService = reporteService;

            var hoy = DateTime.Today;
            var primerDiaMesActual = new DateTime(hoy.Year, hoy.Month, 1);
            FechaInicio = primerDiaMesActual.AddMonths(-1);
            FechaFin = primerDiaMesActual.AddDays(-1);

            GenerarReporteCommand = new AsyncRelayCommand(_ => GenerarReporteAsync(exportPdf: true));
            CargarTodosCommand = new AsyncRelayCommand(_ => CargarTodoAlInicioAsync());

            FlujoCaja = new FlujoCajaDTO();
        }

        private async Task CargarTodoAlInicioAsync()
        {
            await GenerarReporteAsync(exportPdf: false);
        }

        private async Task GenerarReporteAsync(bool exportPdf)
        {
            try
            {
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

                var productos = await _reporteService.GetTopProductosAsync(10);
                TopProductos = new ObservableCollection<ReporteProductoDTO>(productos);
                OnPropertyChanged(nameof(TopProductos));

                var clientes = await _reporteService.GetMejoresClientesAsync(10);
                MejoresClientes = new ObservableCollection<ReporteClienteDTO>(clientes);
                OnPropertyChanged(nameof(MejoresClientes));

                FlujoCaja = await _reporteService.GetFlujoCajaAsync(FechaFin.Month, FechaFin.Year);

                if (exportPdf)
                {
                    var saveDialog = new SaveFileDialog
                    {
                        Title = "Guardar reporte en PDF",
                        Filter = "PDF (*.pdf)|*.pdf",
                        FileName = $"Reporte_{FechaInicio:yyyyMMdd}_{FechaFin:yyyyMMdd}.pdf",
                        AddExtension = true,
                        DefaultExt = ".pdf"
                    };

                    var ok = saveDialog.ShowDialog();
                    if (ok == true)
                    {
                        var data = new ReportePdfData
                        {
                            FechaInicio = FechaInicio,
                            FechaFin = FechaFin,
                            IngresosPorDia = ingresos.ToList(),
                            EgresosDelMes = egresos.ToList(),
                            TopProductos = productos.ToList(),
                            MejoresClientes = clientes.ToList(),
                            FlujoCaja = FlujoCaja
                        };

                        ReportePdfExporter.Export(saveDialog.FileName, data);
                        DialogService.Instance.ShowSuccess("PDF generado correctamente.");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generando reportes: {ex.Message}");
                DialogService.Instance.ShowError($"Ocurrió un error al generar el reporte.\n\nDetalle: {ex.Message}");
            }
        }
    }
}
