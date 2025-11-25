using System.Text.RegularExpressions;

using System.Windows.Controls;
using System.Windows.Input;

namespace ProyectoSauna
{
    public partial class UserControlCuentas : UserControl
    {
        public UserControlCuentas()
        {
            InitializeComponent();
        }

        private void TextBox_NumerosSoloPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^\d+$");
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
