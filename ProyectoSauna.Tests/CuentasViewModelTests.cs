using ProyectoSauna.ViewModels;
using Xunit;

namespace ProyectoSauna.Tests
{
    public class CuentasViewModelTests
    {
        [Fact]
        public void CalcularTotalCuenta_ActualizaTotalCorrecto()
        {
            var vm = new CuentasViewModel();
            vm.CuentaSeleccionada = new CuentaPendiente
            {
                precioEntrada = 20m,
                descuento = 3m
            };

            vm.TotalProductos = 10m;
            vm.TotalServicios = 5m;

            Assert.Equal(20m - 3m + 10m + 5m, vm.TotalCuenta);
        }

        [Fact]
        public void CuentaSeleccionada_PermaneceHastaCambiarla()
        {
            var vm = new CuentasViewModel();
            var cuenta = new CuentaPendiente { idCuenta = 1, precioEntrada = 0m, descuento = 0m };
            vm.CuentaSeleccionada = cuenta;

            vm.TotalProductos = 2m;
            vm.TotalServicios = 3m;

            Assert.Same(cuenta, vm.CuentaSeleccionada);
        }
    }
}