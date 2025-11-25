using System;

namespace ProyectoSauna.Services
{
    public static class InventoryEventService
    {
        public static event EventHandler StockChanged;

        public static void NotifyStockChanged()
        {
            StockChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}