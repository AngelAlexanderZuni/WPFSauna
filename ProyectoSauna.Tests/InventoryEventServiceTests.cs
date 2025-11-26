using ProyectoSauna.Services;
using Xunit;

namespace ProyectoSauna.Tests
{
    public class InventoryEventServiceTests
    {
        [Fact]
        public void NotifyStockChanged_RaisesEvent()
        {
            bool raised = false;
            void Handler(object? s, System.EventArgs e) { raised = true; }

            InventoryEventService.StockChanged += Handler;
            try
            {
                InventoryEventService.NotifyStockChanged();
                Assert.True(raised);
            }
            finally
            {
                InventoryEventService.StockChanged -= Handler;
            }
        }
    }
}