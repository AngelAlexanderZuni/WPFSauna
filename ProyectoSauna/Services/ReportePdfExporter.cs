using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;
using ProyectoSauna.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace ProyectoSauna.Services
{
    public sealed class ReportePdfData
    {
        public required DateTime FechaInicio { get; init; }
        public required DateTime FechaFin { get; init; }

        public required IReadOnlyList<ReporteIngresoDTO> IngresosPorDia { get; init; }
        public required IReadOnlyList<ReporteEgresoDTO> EgresosDelMes { get; init; }
        public required IReadOnlyList<ReporteProductoDTO> TopProductos { get; init; }
        public required IReadOnlyList<ReporteClienteDTO> MejoresClientes { get; init; }

        public required FlujoCajaDTO FlujoCaja { get; init; }
    }

    public static class ReportePdfExporter
    {
        public static void Export(string filePath, ReportePdfData data)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Ruta de archivo inválida.", nameof(filePath));

            EnsureFontResolver();

            var document = new PdfDocument
            {
                Info =
                {
                    Title = "Reporte - ProyectoSauna",
                    Subject = "Reporte generado desde el módulo de Reportes",
                    CreationDate = DateTime.Now
                }
            };

            var page = document.AddPage();
            page.Size = PdfSharpCore.PageSize.A4;
            page.Orientation = PdfSharpCore.PageOrientation.Portrait;

            var gfx = XGraphics.FromPdfPage(page);
            try
            {
                var fontTitle = new XFont("Arial", 16, XFontStyle.Bold);
                var fontSubTitle = new XFont("Arial", 10, XFontStyle.Regular);
                var fontSection = new XFont("Arial", 12, XFontStyle.Bold);
                var fontHeader = new XFont("Arial", 9, XFontStyle.Bold);
                var fontCell = new XFont("Arial", 9, XFontStyle.Regular);

                const double margin = 40;
                double y = margin;

                var culture = CultureInfo.CurrentCulture;

                // Encabezado
                gfx.DrawString("Reporte", fontTitle, XBrushes.Black, new XRect(margin, y, page.Width - margin * 2, 24), XStringFormats.TopLeft);
                y += 22;
                gfx.DrawString($"Rango: {data.FechaInicio:dd/MM/yyyy} - {data.FechaFin:dd/MM/yyyy}", fontSubTitle, XBrushes.Black, new XRect(margin, y, page.Width - margin * 2, 16), XStringFormats.TopLeft);
                y += 14;
                gfx.DrawString($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}", fontSubTitle, XBrushes.Black, new XRect(margin, y, page.Width - margin * 2, 16), XStringFormats.TopLeft);
                y += 18;

                y = DrawSectionTitle(gfx, page, "Ingresos por día", fontSection, margin, y);
                y = DrawTable(
                    document,
                    ref page,
                    ref gfx,
                    "Ingresos por día",
                    new[] { "Fecha", "Total" },
                    new[] { 0.55, 0.45 },
                    data.IngresosPorDia,
                    row => new[] { row.Fecha.ToString("dd/MM/yyyy", culture), row.Total.ToString("C", culture) },
                    fontHeader,
                    fontCell,
                    margin,
                    y);

                y = DrawSectionTitle(gfx, page, "Egresos del mes", fontSection, margin, y + 10);
                y = DrawTable(
                    document,
                    ref page,
                    ref gfx,
                    "Egresos del mes",
                    new[] { "Tipo", "Total" },
                    new[] { 0.70, 0.30 },
                    data.EgresosDelMes,
                    row => new[] { row.TipoEgreso, row.Total.ToString("C", culture) },
                    fontHeader,
                    fontCell,
                    margin,
                    y);

                y = DrawSectionTitle(gfx, page, "Top Productos", fontSection, margin, y + 10);
                y = DrawTable(
                    document,
                    ref page,
                    ref gfx,
                    "Top Productos",
                    new[] { "Producto", "Unidades", "Ingresos" },
                    new[] { 0.55, 0.15, 0.30 },
                    data.TopProductos,
                    row => new[] { row.NombreProducto, row.CantidadVendida.ToString(culture), row.IngresosGenerados.ToString("C", culture) },
                    fontHeader,
                    fontCell,
                    margin,
                    y);

                y = DrawSectionTitle(gfx, page, "Mejores Clientes", fontSection, margin, y + 10);
                y = DrawTable(
                    document,
                    ref page,
                    ref gfx,
                    "Mejores Clientes",
                    new[] { "Cliente", "Visitas", "Gastado" },
                    new[] { 0.55, 0.15, 0.30 },
                    data.MejoresClientes,
                    row => new[] { row.NombreCompleto, row.Visitas.ToString(culture), row.TotalGastado.ToString("C", culture) },
                    fontHeader,
                    fontCell,
                    margin,
                    y);

                y = DrawSectionTitle(gfx, page, "Flujo de Caja", fontSection, margin, y + 10);
                y = EnsureSpace(document, ref page, ref gfx, y, 70, margin);

                DrawKeyValue(gfx, page, "Ingresos", data.FlujoCaja.TotalIngresos.ToString("C", culture), fontCell, margin, y);
                y += 14;
                DrawKeyValue(gfx, page, "Egresos", data.FlujoCaja.TotalEgresos.ToString("C", culture), fontCell, margin, y);
                y += 14;
                DrawKeyValue(gfx, page, "Utilidad", data.FlujoCaja.UtilidadNeta.ToString("C", culture), fontCell, margin, y);
                y += 18;

                // Pie
                DrawFooter(gfx, page, fontSubTitle);

                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(filePath))!);
                document.Save(filePath);
            }
            finally
            {
                gfx.Dispose();
            }
        }

        private static void EnsureFontResolver()
        {
            if (GlobalFontSettings.FontResolver is not null)
                return;

            GlobalFontSettings.FontResolver = new WindowsArialFontResolver();
        }

        private sealed class WindowsArialFontResolver : IFontResolver
        {
            public string DefaultFontName => "Arial";

            public byte[] GetFont(string faceName)
            {
                // En Windows normalmente existen estas fuentes.
                // Si por alguna razón no están, PdfSharpCore lanzará excepción.
                var fontsDir = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);

                var normalized = faceName?.ToLowerInvariant() ?? string.Empty;

                string fileName = normalized switch
                {
                    "arial#b" => "arialbd.ttf",
                    "arial#i" => "ariali.ttf",
                    "arial#bi" => "arialbi.ttf",
                    _ => "arial.ttf"
                };

                var fullPath = Path.Combine(fontsDir, fileName);
                return File.ReadAllBytes(fullPath);
            }

            public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
            {
                if (!string.Equals(familyName, "Arial", StringComparison.OrdinalIgnoreCase))
                    familyName = "Arial";

                if (isBold && isItalic) return new FontResolverInfo("Arial#bi");
                if (isBold) return new FontResolverInfo("Arial#b");
                if (isItalic) return new FontResolverInfo("Arial#i");
                return new FontResolverInfo("Arial");
            }
        }

        private static void DrawFooter(XGraphics gfx, PdfPage page, XFont font)
        {
            var footer = "ProyectoSauna";
            gfx.DrawString(footer, font, XBrushes.Gray, new XRect(40, page.Height - 35, page.Width - 80, 20), XStringFormats.BottomRight);
        }

        private static void DrawKeyValue(XGraphics gfx, PdfPage page, string key, string value, XFont font, double margin, double y)
        {
            gfx.DrawString($"{key}:", font, XBrushes.Black, new XRect(margin, y, page.Width - margin * 2, 12), XStringFormats.TopLeft);
            gfx.DrawString(value, font, XBrushes.Black, new XRect(margin, y, page.Width - margin * 2, 12), XStringFormats.TopRight);
        }

        private static double DrawSectionTitle(XGraphics gfx, PdfPage page, string title, XFont fontSection, double margin, double y)
        {
            gfx.DrawString(title, fontSection, XBrushes.Black, new XRect(margin, y, page.Width - margin * 2, 18), XStringFormats.TopLeft);
            return y + 18;
        }

        private static double EnsureSpace(PdfDocument document, ref PdfPage page, ref XGraphics gfx, double y, double requiredHeight, double margin)
        {
            if (y + requiredHeight <= page.Height - margin)
                return y;

            // Nueva página
            gfx.Dispose();
            page = document.AddPage();
            page.Size = PdfSharpCore.PageSize.A4;
            page.Orientation = PdfSharpCore.PageOrientation.Portrait;
            gfx = XGraphics.FromPdfPage(page);

            return margin;
        }

        private static double DrawTable<T>(
            PdfDocument document,
            ref PdfPage page,
            ref XGraphics gfx,
            string sectionName,
            string[] headers,
            double[] widthFractions,
            IReadOnlyList<T> rows,
            Func<T, string[]> mapRow,
            XFont fontHeader,
            XFont fontCell,
            double margin,
            double y)
        {
            const double rowHeight = 16;
            const double headerHeight = 18;

            double tableWidth = page.Width - margin * 2;

            if (headers.Length != widthFractions.Length)
                throw new ArgumentException($"Tabla '{sectionName}': headers y widthFractions deben tener el mismo tamaño.");

            // Encabezado
            y = EnsureSpace(document, ref page, ref gfx, y, headerHeight + rowHeight, margin);
            DrawRow(gfx, page, headers, widthFractions, tableWidth, margin, y, headerHeight, fontHeader, isHeader: true);
            y += headerHeight;

            if (rows.Count == 0)
            {
                y = EnsureSpace(document, ref page, ref gfx, y, rowHeight, margin);
                gfx.DrawString("(Sin datos)", fontCell, XBrushes.Gray, new XRect(margin, y + 2, tableWidth, rowHeight), XStringFormats.TopLeft);
                return y + rowHeight;
            }

            foreach (var row in rows)
            {
                var cells = mapRow(row);
                y = EnsureSpace(document, ref page, ref gfx, y, rowHeight, margin);
                DrawRow(gfx, page, cells, widthFractions, tableWidth, margin, y, rowHeight, fontCell, isHeader: false);
                y += rowHeight;
            }

            return y;
        }

        private static void DrawRow(
            XGraphics gfx,
            PdfPage page,
            string[] cells,
            double[] widthFractions,
            double tableWidth,
            double margin,
            double y,
            double height,
            XFont font,
            bool isHeader)
        {
            // Fondo y línea superior
            if (isHeader)
            {
                gfx.DrawRectangle(XBrushes.LightGray, margin, y, tableWidth, height);
            }

            double x = margin;
            for (int i = 0; i < widthFractions.Length; i++)
            {
                double w = tableWidth * widthFractions[i];

                // bordes
                gfx.DrawRectangle(XPens.LightGray, x, y, w, height);

                // texto
                var text = i < cells.Length ? (cells[i] ?? string.Empty) : string.Empty;
                text = text.Replace("\r", " ").Replace("\n", " ");

                var rect = new XRect(x + 4, y + 3, w - 8, height - 6);

                // Alineación básica: última columna a la derecha si parece numérica
                var isLast = i == widthFractions.Length - 1;
                var align = isLast ? XStringFormats.TopRight : XStringFormats.TopLeft;

                gfx.DrawString(text, font, XBrushes.Black, rect, align);

                x += w;
            }
        }
    }
}
