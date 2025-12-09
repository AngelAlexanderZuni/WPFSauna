// Tests/ManualConcurrencyTest.xaml.cs - Formulario para pruebas manuales
using Microsoft.Extensions.DependencyInjection;
using ProyectoSauna.Models;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories.Base;
using ProyectoSauna.Services;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace ProyectoSauna.Tests
{
    public partial class ManualConcurrencyTest : Window
    {
        private readonly ConcurrencyRepository<Producto> _repository;
        private Producto _currentProducto;

        public ManualConcurrencyTest()
        {
            InitializeComponent();
            
            // Obtener servicios del DI container
            var context = App.AppHost.Services.GetRequiredService<SaunaDbContext>();
            var concurrencyService = new ConcurrencyService(context);
            _repository = new ConcurrencyRepository<Producto>(context, concurrencyService, useConcurrency: true);
            
            LoadTestProduct();
        }

        private async void LoadTestProduct()
        {
            try
            {
                // Buscar o crear un producto de prueba
                _currentProducto = await _repository.FirstOrDefaultAsync(p => p.codigo == "TESTCONC");
                
                if (_currentProducto == null)
                {
                    _currentProducto = new Producto
                    {
                        codigo = "TESTCONC",
                        nombre = "Producto Prueba Concurrencia",
                        stockActual = 100,
                        precioCompra = 50,
                        precioVenta = 75,
                        stockMinimo = 10,
                        activo = true,
                        idCategoriaProducto = 1
                    };
                    await _repository.AddAsync(_currentProducto);
                }

                UpdateUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar producto: {ex.Message}");
            }
        }

        private void UpdateUI()
        {
            txtCodigo.Text = _currentProducto.codigo;
            txtNombre.Text = _currentProducto.nombre;
            txtStock.Text = _currentProducto.stockActual.ToString();
            txtPrecio.Text = _currentProducto.precioVenta.ToString();
        }

        private async void btnUpdateStock_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!int.TryParse(txtNewStock.Text, out int newStock))
                {
                    MessageBox.Show("Ingrese un stock válido");
                    return;
                }

                // Simular modificación de stock
                _currentProducto.stockActual = newStock;
                
                var result = await _repository.SafeUpdateAsync(_currentProducto);
                
                if (result.Success)
                {
                    lblResult.Content = "✅ Stock actualizado exitosamente";
                    lblResult.Foreground = System.Windows.Media.Brushes.Green;
                    UpdateUI();
                }
                else
                {
                    lblResult.Content = $"❌ Error: {result.Message}";
                    lblResult.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                lblResult.Content = $"💥 Excepción: {ex.Message}";
                lblResult.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private async void btnCheckModifications_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isModified = await _repository.IsEntityModifiedExternallyAsync(
                    _currentProducto, 
                    p => p.stockActual, 
                    p => p.precioVenta,
                    p => p.nombre
                );

                if (isModified)
                {
                    lblModified.Content = "⚠️ El producto fue modificado por otro usuario";
                    lblModified.Foreground = System.Windows.Media.Brushes.Orange;
                    
                    // Ofrecer recargar datos
                    var result = MessageBox.Show("Los datos fueron modificados externamente. ¿Desea recargar?", 
                        "Datos Modificados", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        await _repository.RefreshEntityAsync(_currentProducto);
                        UpdateUI();
                        lblModified.Content = "🔄 Datos recargados";
                        lblModified.Foreground = System.Windows.Media.Brushes.Blue;
                    }
                }
                else
                {
                    lblModified.Content = "✅ Los datos están actualizados";
                    lblModified.Foreground = System.Windows.Media.Brushes.Green;
                }
            }
            catch (Exception ex)
            {
                lblModified.Content = $"Error: {ex.Message}";
                lblModified.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private async void btnSimulateExternalChange_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Simular cambio externo usando otro contexto
                using var otherContext = new SaunaDbContext();
                var otherRepo = new Repository<Producto>(otherContext);
                
                var externalProduct = await otherRepo.GetByIdAsync(_currentProducto.idProducto);
                if (externalProduct != null)
                {
                    externalProduct.stockActual = new Random().Next(1, 200);
                    externalProduct.precioVenta = new Random().Next(50, 150);
                    await otherRepo.UpdateAsync(externalProduct);
                    
                    lblSimulation.Content = $"🔄 Cambio externo simulado - Nuevo stock: {externalProduct.stockActual}";
                    lblSimulation.Foreground = System.Windows.Media.Brushes.Blue;
                }
            }
            catch (Exception ex)
            {
                lblSimulation.Content = $"Error en simulación: {ex.Message}";
                lblSimulation.Foreground = System.Windows.Media.Brushes.Red;
            }
        }
    }
}