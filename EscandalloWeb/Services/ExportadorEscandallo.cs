using ClosedXML.Excel;
using EscandalloWeb.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EscandalloWeb.Services;

public static class ExportadorEscandallo
{
    public static byte[] GenerarExcel(CentroEstetica centro)
    {
        using var wb = new XLWorkbook();

        var wsPvr = wb.Worksheets.Add("PVR");
        var wsServicios = wb.Worksheets.Add("Servicios");
        var wsProductos = wb.Worksheets.Add("Productos");
        var wsPersonal = wb.Worksheets.Add("Personal");
        var wsGastos = wb.Worksheets.Add("Gastos Generales");
        var wsDatos = wb.Worksheets.Add("Datos");

        // 1. Hoja Datos
        wsDatos.Cell(1, 1).Value = "IVA";
        wsDatos.Cell(1, 2).Value = centro.Iva;
        wsDatos.Cell(2, 1).Value = "Beneficio";
        wsDatos.Cell(2, 2).Value = centro.Beneficio;

        // 2. Hoja Gastos Generales
        string[] headersGastos = ["Id", "Nombre", "Cantidad", "Ud medida", "Coste para empresa", "Precio Unitario", "Ud"];
        for (int i = 0; i < headersGastos.Length; i++)
            wsGastos.Cell(1, i + 1).Value = headersGastos[i];

        int rowGasto = 2;
        for (int i = 0; i < centro.GastosGenerales.Count; i++)
        {
            var g = centro.GastosGenerales[i];
            wsGastos.Cell(rowGasto, 1).Value = i + 1;
            wsGastos.Cell(rowGasto, 2).Value = g.Nombre;
            wsGastos.Cell(rowGasto, 3).Value = g.Cantidad;
            wsGastos.Cell(rowGasto, 4).Value = g.UnidadMedida;
            wsGastos.Cell(rowGasto, 5).Value = g.CosteEmpresa;
            wsGastos.Cell(rowGasto, 6).FormulaA1 = $"E{rowGasto}/C{rowGasto}";
            wsGastos.Cell(rowGasto, 7).FormulaA1 = $"D{rowGasto}";
            rowGasto++;
        }

        int totalRowGastos = Math.Max(rowGasto, 2);
        wsGastos.Cell(totalRowGastos, 1).Value = "Total";
        wsGastos.Cell(totalRowGastos, 6).FormulaA1 = centro.GastosGenerales.Count > 0
            ? $"SUM(F2:F{rowGasto - 1})"
            : "0";

        // 3. Hoja Personal
        string[] headersPersonal = ["Id", "Nombre", "Cantidad", "Ud medida", "Neto", "Retenciones", "Coste para empresa", "Precio Unitario", "Ud"];
        for (int i = 0; i < headersPersonal.Length; i++)
            wsPersonal.Cell(1, i + 1).Value = headersPersonal[i];

        for (int i = 0; i < centro.Personal.Count; i++)
        {
            int r = i + 2;
            var p = centro.Personal[i];
            wsPersonal.Cell(r, 1).Value = i + 1;
            wsPersonal.Cell(r, 2).Value = p.Nombre;
            wsPersonal.Cell(r, 3).Value = p.Cantidad;
            wsPersonal.Cell(r, 4).Value = p.UnidadMedida;
            wsPersonal.Cell(r, 5).Value = p.Neto;
            wsPersonal.Cell(r, 6).Value = p.Retenciones;
            wsPersonal.Cell(r, 7).FormulaA1 = $"((E{r}*F{r})+E{r})*1.5";
            wsPersonal.Cell(r, 8).FormulaA1 = $"G{r}/C{r}";
            wsPersonal.Cell(r, 9).FormulaA1 = $"D{r}";
        }

        // 4. Hoja Productos
        string[] headersProductos = ["Id", "Nombre", "Cantidad", "Ud medida", "Precio", "Precio Unitario", "Ud"];
        for (int i = 0; i < headersProductos.Length; i++)
            wsProductos.Cell(1, i + 1).Value = headersProductos[i];

        for (int i = 0; i < centro.Productos.Count; i++)
        {
            int r = i + 2;
            var prod = centro.Productos[i];
            wsProductos.Cell(r, 1).Value = i + 1;
            wsProductos.Cell(r, 2).Value = prod.Nombre;
            wsProductos.Cell(r, 3).Value = prod.Cantidad;
            wsProductos.Cell(r, 4).Value = prod.UnidadMedida;
            wsProductos.Cell(r, 5).Value = prod.Precio;
            wsProductos.Cell(r, 6).FormulaA1 = $"E{r}/C{r}";
            wsProductos.Cell(r, 7).FormulaA1 = $"D{r}";
        }

        // 5. Hoja Servicios (estructura compatible con CalculadoraEscandallo: 30 bloques de Producto/Cantidad/Coste)
        int maxProductos = Math.Max(30, centro.Servicios.Select(s => s.LineasProducto.Count).DefaultIfEmpty(0).Max());
        string[] headersServiciosBase = ["Id", "Servicio", "Minutos", "Escandallo Total", "Gastos Generales", "Persona", "Coste Persona"];
        for (int i = 0; i < headersServiciosBase.Length; i++)
            wsServicios.Cell(1, i + 1).Value = headersServiciosBase[i];

        for (int k = 1; k <= maxProductos; k++)
        {
            int colBase = 8 + (k - 1) * 3;
            wsServicios.Cell(1, colBase).Value = $"Producto {k}";
            wsServicios.Cell(1, colBase + 1).Value = $"Cantidad {k}";
            wsServicios.Cell(1, colBase + 2).Value = $"Coste {k}";
        }

        // 6. Hoja PVR cabeceras
        string[] headersPvr = ["Id", "Servicio", "Escandallo Total", "Beneficio", "IVA", "PVR"];
        for (int i = 0; i < headersPvr.Length; i++)
            wsPvr.Cell(1, i + 1).Value = headersPvr[i];

        int lastPersonalRow = Math.Max(2, centro.Personal.Count + 1);
        int lastProductoRow = Math.Max(2, centro.Productos.Count + 1);

        for (int i = 0; i < centro.Servicios.Count; i++)
        {
            int r = i + 2;
            var s = centro.Servicios[i];

            wsServicios.Cell(r, 1).Value = i;
            wsServicios.Cell(r, 2).Value = s.Nombre;
            wsServicios.Cell(r, 3).Value = s.Minutos;
            wsServicios.Cell(r, 5).FormulaA1 = $"C{r}*'Gastos Generales'!$F${totalRowGastos}";
            wsServicios.Cell(r, 6).Value = s.CategoriaPersonal?.Nombre ?? string.Empty;
            wsServicios.Cell(r, 7).FormulaA1 = $"IFERROR(VLOOKUP(F{r},Personal!$B$2:$I${lastPersonalRow},7,FALSE)*C{r},0)";

            var costCells = new List<string> { $"E{r}", $"G{r}" };

            for (int k = 1; k <= maxProductos; k++)
            {
                int colProd = 8 + (k - 1) * 3;
                int colCant = colProd + 1;
                int colCoste = colProd + 2;

                string prodCoord = wsServicios.Cell(r, colProd).Address.ToStringRelative();
                string cantCoord = wsServicios.Cell(r, colCant).Address.ToStringRelative();
                string costeCoord = wsServicios.Cell(r, colCoste).Address.ToStringRelative();

                if (k <= s.LineasProducto.Count)
                {
                    var lp = s.LineasProducto[k - 1];
                    wsServicios.Cell(r, colProd).Value = lp.Producto?.Nombre ?? string.Empty;
                    wsServicios.Cell(r, colCant).Value = lp.Cantidad;
                }

                wsServicios.Cell(r, colCoste).FormulaA1 =
                    $"IFERROR(VLOOKUP({prodCoord},Productos!$B$2:$G${lastProductoRow},5,FALSE)*{cantCoord},0)";

                costCells.Add(costeCoord);
            }

            wsServicios.Cell(r, 4).FormulaA1 = string.Join("+", costCells);

            // Fila correspondiente en PVR
            wsPvr.Cell(r, 1).Value = i;
            wsPvr.Cell(r, 2).FormulaA1 = $"Servicios!B{r}";
            wsPvr.Cell(r, 3).FormulaA1 = $"Servicios!D{r}";
            wsPvr.Cell(r, 4).FormulaA1 = $"C{r}*Datos!$B$2+C{r}";
            wsPvr.Cell(r, 5).FormulaA1 = $"D{r}*Datos!$B$1+D{r}";
            if (s.Pvr.HasValue)
            {
                wsPvr.Cell(r, 6).Value = s.Pvr.Value;
            }
        }

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public static byte[] GenerarPdf(CentroEstetica centro)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var totalGastosUnit = centro.TotalPrecioUnitarioGastosGenerales;
        var totalGastosMes = centro.GastosGenerales.Sum(g => g.CosteEmpresa);

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.2f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9.5f));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Informe Ejecutivo y Desglose de Escandallos").Bold().FontSize(15).FontColor(Colors.Indigo.Darken3);
                            c.Item().Text($"Centro: {centro.Nombre} {(string.IsNullOrWhiteSpace(centro.Direccion) ? "" : $"— {centro.Direccion}")}").FontSize(10).FontColor(Colors.Grey.Darken2);
                        });
                        row.ConstantItem(120).AlignRight().Text($"Fecha: {DateTime.Now:dd/MM/yyyy}").FontSize(9).FontColor(Colors.Grey.Darken1);
                    });
                    col.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    // 1. Resumen Ejecutivo
                    col.Item().Text("1. Resumen Ejecutivo").Bold().FontSize(12).FontColor(Colors.Indigo.Darken2);
                    col.Item().PaddingTop(4).PaddingBottom(8).Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                        });

                        t.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                        {
                            c.Item().Text("IVA / Beneficio Objetivo").FontSize(8).FontColor(Colors.Grey.Darken1);
                            c.Item().Text($"{centro.Iva * 100:N0}% / {centro.Beneficio * 100:N0}%").Bold().FontSize(11);
                        });
                        t.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                        {
                            c.Item().Text("Gastos Grales / Minuto").FontSize(8).FontColor(Colors.Grey.Darken1);
                            c.Item().Text($"{totalGastosUnit:N4} €/min ({totalGastosMes:N0} €/mes)").Bold().FontSize(11);
                        });
                        t.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                        {
                            c.Item().Text("Servicios Analizados").FontSize(8).FontColor(Colors.Grey.Darken1);
                            c.Item().Text($"{centro.Servicios.Count} tratamientos").Bold().FontSize(11);
                        });
                        t.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                        {
                            c.Item().Text("Catálogo y Personal").FontSize(8).FontColor(Colors.Grey.Darken1);
                            c.Item().Text($"{centro.Productos.Count} prod. · {centro.Personal.Count} perfiles").Bold().FontSize(11);
                        });
                    });

                    // Tabla PVR Resumen
                    col.Item().PaddingBottom(12).Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2.2f);
                            c.RelativeColumn(0.9f);
                            c.RelativeColumn(1.4f);
                            c.RelativeColumn(1.2f);
                            c.RelativeColumn(1.2f);
                            c.RelativeColumn(1.2f);
                            c.RelativeColumn(1.1f);
                        });

                        t.Header(h =>
                        {
                            string[] cols = ["Servicio", "Minutos", "Personal", "Escandallo", "Con Beneficio", "Con IVA", "PVR"];
                            foreach (var title in cols)
                            {
                                h.Cell().Background(Colors.Indigo.Lighten5).BorderBottom(1).BorderColor(Colors.Indigo.Lighten3).Padding(4).Text(title).Bold().FontSize(8.5f);
                            }
                        });

                        foreach (var s in centro.Servicios)
                        {
                            var esc = s.CalcularEscandalloTotal(totalGastosUnit);
                            var ben = s.CalcularPrecioConBeneficio(totalGastosUnit, centro.Beneficio);
                            var iva = s.CalcularPrecioConIva(totalGastosUnit, centro.Beneficio, centro.Iva);

                            t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(s.Nombre).Bold();
                            t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text($"{s.Minutos:N0} min");
                            t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(s.CategoriaPersonal?.Nombre ?? "-");
                            t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text($"{esc:N2} €");
                            t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text($"{ben:N2} €");
                            t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text($"{iva:N2} €");
                            t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(s.Pvr.HasValue ? $"{s.Pvr.Value:N2} €" : "-");
                        }
                    });

                    // 2. Desglose de Escandallos
                    col.Item().Text("2. Desglose Detallado de Escandallos por Servicio").Bold().FontSize(12).FontColor(Colors.Indigo.Darken2);

                    foreach (var s in centro.Servicios)
                    {
                        var costeGastos = s.CalcularGastosGenerales(totalGastosUnit);
                        var costePers = s.CostePersona;
                        var costeProd = s.CosteProductos;
                        var escTotal = s.CalcularEscandalloTotal(totalGastosUnit);
                        var pConIva = s.CalcularPrecioConIva(totalGastosUnit, centro.Beneficio, centro.Iva);

                        col.Item().PaddingTop(8).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(sc =>
                        {
                            sc.Item().Row(r =>
                            {
                                r.RelativeItem().Text($"{s.Nombre} ({s.Minutos:N0} min — {s.CategoriaPersonal?.Nombre ?? "Sin personal"})").Bold().FontSize(10.5f);
                                r.ConstantItem(180).AlignRight().Text($"Escandallo Total: {escTotal:N2} € | Sugerido IVA: {pConIva:N2} €").Bold().FontSize(9f).FontColor(Colors.Indigo.Darken2);
                            });

                            sc.Item().PaddingVertical(3).Text(
                                $"Gastos Generales: {costeGastos:N2} €  |  Coste Personal: {costePers:N2} €  |  Coste Productos ({s.LineasProducto.Count}): {costeProd:N2} €  |  PVR: {(s.Pvr.HasValue ? $"{s.Pvr.Value:N2} €" : "Sin fijar")}")
                                .FontSize(8.5f).FontColor(Colors.Grey.Darken2);

                            if (s.LineasProducto.Any())
                            {
                                sc.Item().PaddingTop(4).Table(pt =>
                                {
                                    pt.ColumnsDefinition(c =>
                                    {
                                        c.ConstantColumn(25);
                                        c.RelativeColumn(3);
                                        c.RelativeColumn(1.5f);
                                        c.RelativeColumn(1.5f);
                                        c.RelativeColumn(1.5f);
                                    });

                                    pt.Header(h =>
                                    {
                                        h.Cell().Background(Colors.Grey.Lighten4).Padding(3).Text("#").Bold().FontSize(8);
                                        h.Cell().Background(Colors.Grey.Lighten4).Padding(3).Text("Producto").Bold().FontSize(8);
                                        h.Cell().Background(Colors.Grey.Lighten4).Padding(3).Text("Precio Unit.").Bold().FontSize(8);
                                        h.Cell().Background(Colors.Grey.Lighten4).Padding(3).Text("Cantidad").Bold().FontSize(8);
                                        h.Cell().Background(Colors.Grey.Lighten4).Padding(3).Text("Coste").Bold().FontSize(8);
                                    });

                                    int idx = 1;
                                    foreach (var lp in s.LineasProducto)
                                    {
                                        pt.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).Padding(3).Text($"{idx++}").FontSize(8);
                                        pt.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).Padding(3).Text(lp.Producto?.Nombre ?? "-").FontSize(8);
                                        pt.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).Padding(3).Text($"{(lp.Producto?.PrecioUnitario ?? 0m):N4} €").FontSize(8);
                                        pt.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).Padding(3).Text($"{lp.Cantidad:N2} {lp.Producto?.UnidadMedida}").FontSize(8);
                                        pt.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4).Padding(3).Text($"{lp.Coste:N4} €").FontSize(8);
                                    }
                                });
                            }
                        });
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Página ");
                    x.CurrentPageNumber();
                    x.Span(" de ");
                    x.TotalPages();
                });
            });
        });

        return doc.GeneratePdf();
    }
}
