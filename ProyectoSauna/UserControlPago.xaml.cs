using System;
using System.Windows;
using System.Windows.Controls;

namespace ProyectoSauna
{
    /// <summary>
    /// UserControl temporal para verificar que los datos con descuentos se pasen correctamente
    /// </summary>
    public partial class UserControlPago : UserControl
    {
        public UserControlPago()
        {
            InitializeComponent();
            CargarDatosCuenta();
        }

        private void CargarDatosCuenta()
        {
            try
            {
                // 📋 OBTENER DATOS PASADOS DESDE CuentasViewModel
                if (Application.Current?.Properties != null)
                {
                    var props = Application.Current.Properties;

                    // ✅ MOSTRAR INFORMACIÓN DE LA CUENTA
                    if (props.Contains("IdCuenta"))
                        TxtIdCuenta.Text = props["IdCuenta"].ToString();

                    if (props.Contains("NombreCliente"))
                        TxtNombreCliente.Text = props["NombreCliente"].ToString();

                    if (props.Contains("DocumentoCliente"))
                        TxtDocumentoCliente.Text = props["DocumentoCliente"].ToString();

                    if (props.Contains("TotalCuenta"))
                    {
                        if (decimal.TryParse(props["TotalCuenta"].ToString(), out decimal total))
                        {
                            TxtTotalCuenta.Text = $"S/ {total:N2}";
                            
                            // 🐛 DEBUG: Log del total recibido
                            System.Diagnostics.Debug.WriteLine($"💰 TOTAL RECIBIDO EN PAGOS: S/ {total:N2}");
                        }
                    }

                    if (props.Contains("DescuentoAplicado"))
                    {
                        if (decimal.TryParse(props["DescuentoAplicado"].ToString(), out decimal descuento))
                        {
                            if (descuento > 0)
                                TxtDescuentoAplicado.Text = $"- S/ {descuento:N2}";
                            else
                                TxtDescuentoAplicado.Text = "Sin descuentos";
                                
                            // 🐛 DEBUG: Log del descuento recibido
                            System.Diagnostics.Debug.WriteLine($"🎁 DESCUENTO RECIBIDO EN PAGOS: S/ {descuento:N2}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al cargar datos: {ex.Message}");
                MessageBox.Show($"Error al cargar datos de la cuenta: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}