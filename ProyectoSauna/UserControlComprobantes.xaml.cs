using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using ProyectoSauna.ViewModels;

namespace ProyectoSauna
{
    public partial class UserControlComprobantes : UserControl
    {
        public UserControlComprobantes()
        {
            InitializeComponent();
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                return;

            if (App.AppHost != null)
            {
                DataContext = App.AppHost.Services.GetRequiredService<ComprobantesViewModel>();
            }
        }
    }
}
