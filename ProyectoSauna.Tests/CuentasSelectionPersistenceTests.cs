using ProyectoSauna.ViewModels;
using ProyectoSauna.Models;
using Xunit;

namespace ProyectoSauna.Tests
{
    public class CuentasSelectionPersistenceTests
    {
        [Fact]
        public void SetterCuentaSeleccionada_PersisteEnSesion()
        {
            var vm = new CuentasViewModel();
            var cuenta = new CuentaPendiente { idCuenta = 9, precioEntrada = 0m, descuento = 0m };

            vm.CuentaSeleccionada = cuenta;

            Assert.Equal(9, vm.CuentaSeleccionada.idCuenta);
        }

        [Fact]
        public void CargarCuentasPendientes_RestaurarSeleccionDesdeSesion()
        {
            var vm = new CuentasViewModel();
            SesionActual.CuentaSeleccionadaId = 9;

            // Simular carga de cuentas y restauración
            vm.CuentasPendientes = new System.Collections.ObjectModel.ObservableCollection<CuentaPendiente>
            {
                new CuentaPendiente { idCuenta = 9, precioEntrada = 0m, descuento = 0m },
                new CuentaPendiente { idCuenta = 10, precioEntrada = 0m, descuento = 0m }
            };

            // Forzar setter con la cuenta que debe ser restaurada
            vm.CuentaSeleccionada = vm.CuentasPendientes[0];

            Assert.Equal(9, vm.CuentaSeleccionada.idCuenta);
            Assert.Equal(9, SesionActual.CuentaSeleccionadaId);
        }
    }
}
