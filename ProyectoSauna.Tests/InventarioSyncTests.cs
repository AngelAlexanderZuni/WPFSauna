using ProyectoSauna.Services;
using ProyectoSauna.Models.Entities;
using Xunit;

namespace ProyectoSauna.Tests
{
    public class InventarioSyncTests
    {
        [Fact]
        public void NotifyStockChanged_RaisesEvent_MultipleTimes_Idempotent()
        {
            int count = 0;
            void Handler(object? s, System.EventArgs e) { count++; }
            InventoryEventService.StockChanged += Handler;
            try
            {
                InventoryEventService.NotifyStockChanged();
                InventoryEventService.NotifyStockChanged();
                Assert.Equal(2, count);
            }
            finally
            {
                InventoryEventService.StockChanged -= Handler;
            }
        }
    }
}