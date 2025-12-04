using Microsoft.Extensions.DependencyInjection;
using ProyectoSauna.ViewModels;
using System.Windows.Controls;

namespace ProyectoSauna
{
    public partial class UserControlComprobantes : UserControl
    {
        public UserControlComprobantes()
        {
            InitializeComponent();
            
            if (App.AppHost != null)
            {
                var viewModel = App.AppHost.Services.GetService<ComprobantesViewModel>();
                if (viewModel != null)
                {
                    DataContext = viewModel;
                }
            }
        }
    }
}
