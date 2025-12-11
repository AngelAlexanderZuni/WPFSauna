// UserControlEgresos.xaml.cs - VERSIÓN FUNCIONAL PRINCIPAL
using ProyectoSauna.ViewModels;
using System.Windows.Controls;

namespace ProyectoSauna
{
    /// <summary>
    /// Maqueta funcional del módulo de Egresos
    /// </summary>
    public partial class UserControlEgresos : UserControl
    {
        public UserControlEgresos()
        {
            InitializeComponent();
            // ✅ Usar el ViewModel principal funcional
            DataContext = new EgresosViewModel();
        }
    }
}