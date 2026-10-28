using System.Globalization;
using ClosedXML.Excel;
using Planilla.Aplicacion.DTOs;
using Planilla.Aplicacion.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Planilla.Infraestructura.Reportes;

public class GeneradorDocumentos : IGeneradorDocumentos
{
    private static readonly string[] Meses =
    [
        "enero", "febrero", "marzo", "abril", "mayo", "junio",
        "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre"
    ];

    private static readonly string[] TitulosPlanilla =
    [
        "Código", "Empleado", "Puesto", "Días", "Horas extra", "Salario devengado", "Monto horas extra",
        "Bonificación", "Otros ingresos", "Total ingresos", "IGSS", "ISR", "Otros descuentos",
        "Total descuentos", "Líquido a recibir"
    ];

    public byte[] PlanillaPdf(ReportePlanillaDto reporte)
    {
        return Document.Create(documento =>
        {
            documento.Page(pagina =>
            {
                pagina.Size(PageSizes.Letter.Landscape());
                pagina.Margin(1, Unit.Centimetre);
                pagina.DefaultTextStyle(t => t.FontSize(7));
                pagina.Header().Element(c => EncabezadoPdf(c, reporte.Encabezado, "PLANILLA GENERAL"));
                pagina.Content().PaddingVertical(8).Column(columna =>
                {
                    columna.Item().Element(c => TablaPlanilla(c, reporte));
                    columna.Item().PaddingTop(12).ShowEntire().Element(c => CuotaPatronalPdf(c, reporte.CuotaPatronal));
                    columna.Item().PaddingTop(40).ShowEntire().Row(fila =>
                    {
                        fila.RelativeItem().PaddingHorizontal(40).Element(c => Firma(c, "Elaborado por"));
                        fila.RelativeItem().PaddingHorizontal(40).Element(c => Firma(c, "Aprobado por"));
                    });
                });
                pagina.Footer().Element(PiePagina);
                MarcaPreliminar(pagina, reporte.Encabezado);
            });
        }).GeneratePdf();
    }

    public byte[] BoletasPdf(ReporteBoletasDto reporte)
    {
        return Document.Create(documento =>
        {
            documento.Page(pagina =>
            {
                pagina.Size(PageSizes.Letter);
                pagina.Margin(1, Unit.Centimetre);
                pagina.DefaultTextStyle(t => t.FontSize(8));
                pagina.Content().Column(columna =>
                {
                    foreach (var boleta in reporte.Boletas)
                    {
                        columna.Item().MinHeight(12.9f, Unit.Centimetre).PreventPageBreak()
                            .Element(c => BoletaPdf(c, reporte.Encabezado, boleta));
                    }
                });
                MarcaPreliminar(pagina, reporte.Encabezado);
            });
        }).GeneratePdf();
    }

    public byte[] PlanillaExcel(ReportePlanillaDto reporte)
    {
        using var libro = new XLWorkbook();
        var hoja = libro.Worksheets.Add("Planilla");
        var encabezado = reporte.Encabezado;
        var columnas = TitulosPlanilla.Length;

        hoja.Cell(1, 1).Value = encabezado.NombreEmpresa;
        hoja.Cell(1, 1).Style.Font.Bold = true;
        hoja.Cell(1, 1).Style.Font.FontSize = 14;
        hoja.Cell(2, 1).Value = $"NIT: {encabezado.NitEmpresa}";
        hoja.Cell(3, 1).Value = encabezado.DireccionEmpresa;
        hoja.Cell(4, 1).Value = $"{encabezado.Codigo} - {encabezado.Descripcion}";
        hoja.Cell(4, 1).Style.Font.Bold = true;
        hoja.Cell(5, 1).Value = Estado(encabezado);
        if (encabezado.Preliminar)
        {
            hoja.Cell(5, 1).Style.Font.FontColor = XLColor.Red;
            hoja.Cell(5, 1).Style.Font.Bold = true;
        }

        var fila = 7;
        for (var i = 0; i < columnas; i++)
        {
            hoja.Cell(fila, i + 1).Value = TitulosPlanilla[i];
        }
        var titulos = hoja.Range(fila, 1, fila, columnas);
        titulos.Style.Font.Bold = true;
        titulos.Style.Fill.BackgroundColor = XLColor.LightGray;
        titulos.Style.Alignment.WrapText = true;
        titulos.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        hoja.SheetView.FreezeRows(fila);

        foreach (var departamento in reporte.Departamentos)
        {
            fila++;
            hoja.Cell(fila, 1).Value = departamento.NombreDepartamento;
            hoja.Range(fila, 1, fila, columnas).Style.Font.Bold = true;

            foreach (var empleado in departamento.Filas)
            {
                fila++;
                hoja.Cell(fila, 1).Value = empleado.CodigoEmpleado;
                hoja.Cell(fila, 2).Value = empleado.NombreCompleto;
                hoja.Cell(fila, 3).Value = empleado.NombrePuesto;
                hoja.Cell(fila, 4).Value = empleado.DiasPagar;
                hoja.Cell(fila, 5).Value = empleado.HorasExtra;
                EscribirMontos(hoja, fila, MontosFila(empleado));
            }

            fila++;
            hoja.Cell(fila, 2).Value = $"Subtotal {departamento.NombreDepartamento} ({departamento.Subtotal.CantidadEmpleados})";
            EscribirMontos(hoja, fila, MontosTotales(departamento.Subtotal));
            hoja.Range(fila, 1, fila, columnas).Style.Font.Bold = true;
            hoja.Range(fila, 6, fila, columnas).Style.Border.TopBorder = XLBorderStyleValues.Thin;
        }

        fila += 2;
        hoja.Cell(fila, 2).Value = $"Total general ({reporte.Total.CantidadEmpleados})";
        EscribirMontos(hoja, fila, MontosTotales(reporte.Total));
        hoja.Range(fila, 1, fila, columnas).Style.Font.Bold = true;
        hoja.Range(fila, 6, fila, columnas).Style.Border.TopBorder = XLBorderStyleValues.Double;

        fila += 2;
        hoja.Cell(fila, 2).Value = "Cuota patronal";
        hoja.Cell(fila, 2).Style.Font.Bold = true;
        var cuota = new (string Concepto, decimal Monto)[]
        {
            ("IGSS patronal", reporte.CuotaPatronal.IgssPatronal),
            ("Total cuota patronal", reporte.CuotaPatronal.Total)
        };
        foreach (var (concepto, monto) in cuota)
        {
            fila++;
            hoja.Cell(fila, 2).Value = concepto;
            hoja.Cell(fila, 3).Value = monto;
            hoja.Cell(fila, 3).Style.NumberFormat.Format = "#,##0.00";
        }
        hoja.Cell(fila, 2).Style.Font.Bold = true;
        hoja.Cell(fila, 3).Style.Font.Bold = true;

        hoja.Columns(1, columnas).AdjustToContents(8, fila);
        hoja.Columns(4, columnas).Width = 13;
        hoja.Row(7).Height = 30;

        using var flujo = new MemoryStream();
        libro.SaveAs(flujo);
        return flujo.ToArray();
    }

    public byte[] TablaPdf(ReporteTablaDto reporte)
    {
        var columnas = reporte.Columnas.Select((c, i) => (Columna: c, Indice: i)).Where(c => !c.Columna.SoloExcel).ToList();
        return Document.Create(documento =>
        {
            documento.Page(pagina =>
            {
                pagina.Size(columnas.Count > 7 ? PageSizes.Letter.Landscape() : PageSizes.Letter);
                pagina.Margin(1, Unit.Centimetre);
                pagina.DefaultTextStyle(t => t.FontSize(columnas.Count > 10 ? 7 : 8));
                pagina.Header().Element(c => EncabezadoTabla(c, reporte));
                pagina.Content().PaddingVertical(8).Element(c => TablaGenerica(c, reporte, columnas));
                pagina.Footer().Element(PiePagina);
                if (reporte.Preliminar)
                {
                    pagina.Background().AlignCenter().AlignMiddle().Rotate(-30)
                        .Text("PRELIMINAR").FontSize(90).Bold().FontColor(Colors.Red.Lighten5);
                }
            });
        }).GeneratePdf();
    }

    public byte[] TablaExcel(ReporteTablaDto reporte)
    {
        using var libro = new XLWorkbook();
        var hoja = libro.Worksheets.Add(NombreHoja(reporte.Titulo));
        var total = reporte.Columnas.Count;

        hoja.Cell(1, 1).Value = reporte.NombreEmpresa;
        hoja.Cell(1, 1).Style.Font.Bold = true;
        hoja.Cell(1, 1).Style.Font.FontSize = 14;
        hoja.Cell(2, 1).Value = $"NIT: {reporte.NitEmpresa}";
        hoja.Cell(3, 1).Value = reporte.DireccionEmpresa;
        hoja.Cell(4, 1).Value = string.IsNullOrWhiteSpace(reporte.Subtitulo) ? reporte.Titulo : $"{reporte.Titulo} - {reporte.Subtitulo}";
        hoja.Cell(4, 1).Style.Font.Bold = true;
        var fila = 5;
        if (reporte.Filtros.Count > 0)
        {
            hoja.Cell(fila++, 1).Value = string.Join(" · ", reporte.Filtros);
        }
        hoja.Cell(fila, 1).Value = reporte.Preliminar
            ? "PRELIMINAR (planilla no cerrada)"
            : $"Generado el {reporte.FechaGeneracion:dd/MM/yyyy HH:mm}";
        if (reporte.Preliminar)
        {
            hoja.Cell(fila, 1).Style.Font.FontColor = XLColor.Red;
            hoja.Cell(fila, 1).Style.Font.Bold = true;
        }

        fila += 2;
        var filaTitulos = fila;
        for (var i = 0; i < total; i++)
        {
            hoja.Cell(fila, i + 1).Value = reporte.Columnas[i].Titulo;
        }
        var titulos = hoja.Range(fila, 1, fila, total);
        titulos.Style.Font.Bold = true;
        titulos.Style.Fill.BackgroundColor = XLColor.LightGray;
        hoja.SheetView.FreezeRows(fila);

        var indices = Enumerable.Range(0, total).ToList();
        foreach (var grupo in Grupos(reporte))
        {
            if (grupo.Nombre != null)
            {
                fila++;
                hoja.Cell(fila, 1).Value = grupo.Nombre;
                hoja.Range(fila, 1, fila, total).Style.Font.Bold = true;
            }
            foreach (var registro in grupo.Filas)
            {
                fila++;
                for (var i = 0; i < total; i++)
                {
                    EscribirCelda(hoja.Cell(fila, i + 1), reporte.Columnas[i], registro.Valores[i]);
                }
            }
            if (grupo.Nombre != null && HaySumas(reporte))
            {
                fila++;
                EscribirTotales(hoja, fila, reporte, indices, grupo.Filas, $"Subtotal {grupo.Nombre} ({grupo.Filas.Count})");
                hoja.Range(fila, 1, fila, total).Style.Border.TopBorder = XLBorderStyleValues.Thin;
            }
        }
        if (HaySumas(reporte) && reporte.Filas.Count > 0)
        {
            fila += 2;
            EscribirTotales(hoja, fila, reporte, indices, reporte.Filas, $"Total ({reporte.Filas.Count})");
            hoja.Range(fila, 1, fila, total).Style.Border.TopBorder = XLBorderStyleValues.Double;
        }

        hoja.Columns(1, total).AdjustToContents(filaTitulos, fila);
        foreach (var columna in hoja.Columns(1, total))
        {
            if (columna.Width > 45)
            {
                columna.Width = 45;
            }
        }

        using var flujo = new MemoryStream();
        libro.SaveAs(flujo);
        return flujo.ToArray();
    }

    private static void EncabezadoTabla(IContainer contenedor, ReporteTablaDto reporte)
    {
        contenedor.BorderBottom(1).PaddingBottom(4).Row(fila =>
        {
            fila.RelativeItem().Column(c =>
            {
                c.Item().Text(reporte.NombreEmpresa ?? "").Bold().FontSize(11);
                c.Item().Text($"NIT: {reporte.NitEmpresa}");
                c.Item().Text(reporte.DireccionEmpresa ?? "");
            });
            fila.RelativeItem().AlignRight().Column(c =>
            {
                c.Item().AlignRight().Text(reporte.Titulo.ToUpper()).Bold().FontSize(11);
                if (!string.IsNullOrWhiteSpace(reporte.Subtitulo))
                {
                    c.Item().AlignRight().Text(reporte.Subtitulo);
                }
                if (reporte.Filtros.Count > 0)
                {
                    c.Item().AlignRight().Text(string.Join(" · ", reporte.Filtros));
                }
                var estado = c.Item().AlignRight().Text(reporte.Preliminar
                    ? "PRELIMINAR (planilla no cerrada)"
                    : $"Generado el {reporte.FechaGeneracion:dd/MM/yyyy HH:mm}");
                if (reporte.Preliminar)
                {
                    estado.Bold().FontColor(Colors.Red.Medium);
                }
            });
        });
    }

    private static void TablaGenerica(IContainer contenedor, ReporteTablaDto reporte,
        List<(ColumnaReporteDto Columna, int Indice)> columnas)
    {
        if (reporte.Filas.Count == 0)
        {
            contenedor.PaddingTop(20).AlignCenter().Text("No hay datos para los filtros elegidos.");
            return;
        }

        var indices = columnas.Select(c => c.Indice).ToList();
        contenedor.Table(tabla =>
        {
            tabla.ColumnsDefinition(c =>
            {
                foreach (var (columna, _) in columnas)
                {
                    c.RelativeColumn(columna.Tipo == "TEXTO" ? 2 : 1.2f);
                }
            });

            tabla.Header(h =>
            {
                foreach (var (columna, _) in columnas)
                {
                    var celda = h.Cell().Background(Colors.Grey.Lighten2).BorderBottom(1).Padding(2).AlignMiddle();
                    if (columna.Tipo != "TEXTO")
                    {
                        celda = celda.AlignRight();
                    }
                    celda.Text(columna.Titulo).Bold();
                }
            });

            foreach (var grupo in Grupos(reporte))
            {
                if (grupo.Nombre != null)
                {
                    tabla.Cell().ColumnSpan((uint)columnas.Count).PaddingTop(6).PaddingBottom(2)
                        .Text(grupo.Nombre).Bold().FontSize(8);
                }
                foreach (var registro in grupo.Filas)
                {
                    foreach (var (columna, indice) in columnas)
                    {
                        Celda(tabla, Formato(columna, registro.Valores[indice]), columna.Tipo != "TEXTO");
                    }
                }
                if (grupo.Nombre != null && HaySumas(reporte))
                {
                    FilaTotalesTabla(tabla, reporte, columnas, grupo.Filas, $"Subtotal {grupo.Nombre} ({grupo.Filas.Count})");
                }
            }

            if (HaySumas(reporte))
            {
                FilaTotalesTabla(tabla, reporte, columnas, reporte.Filas, $"TOTAL ({reporte.Filas.Count})");
            }
        });
    }

    private static void FilaTotalesTabla(TableDescriptor tabla, ReporteTablaDto reporte,
        List<(ColumnaReporteDto Columna, int Indice)> columnas, List<FilaReporteDto> filas, string titulo)
    {
        var primeraSuma = columnas.FindIndex(c => c.Columna.Sumar);
        var etiqueta = Math.Max(primeraSuma, 1);
        tabla.Cell().ColumnSpan((uint)etiqueta).BorderTop(0.8f).Padding(2).Text(titulo).Bold();
        foreach (var (columna, indice) in columnas.Skip(etiqueta))
        {
            var celda = tabla.Cell().BorderTop(0.8f).Padding(2).AlignRight();
            celda.Text(columna.Sumar ? Formato(columna, Sumar(filas, indice)) : "").Bold();
        }
    }

    private static void EscribirTotales(IXLWorksheet hoja, int fila, ReporteTablaDto reporte, List<int> indices,
        List<FilaReporteDto> filas, string titulo)
    {
        hoja.Cell(fila, 1).Value = titulo;
        foreach (var indice in indices.Where(i => reporte.Columnas[i].Sumar))
        {
            EscribirCelda(hoja.Cell(fila, indice + 1), reporte.Columnas[indice], Sumar(filas, indice));
        }
        hoja.Range(fila, 1, fila, reporte.Columnas.Count).Style.Font.Bold = true;
    }

    private static void EscribirCelda(IXLCell celda, ColumnaReporteDto columna, object valor)
    {
        switch (valor)
        {
            case null:
                return;
            case DateOnly fecha:
                celda.Value = fecha.ToDateTime(TimeOnly.MinValue);
                celda.Style.NumberFormat.Format = "dd/mm/yyyy";
                return;
            case decimal or int or double:
                celda.Value = Convert.ToDecimal(valor);
                celda.Style.NumberFormat.Format = columna.Tipo == "MONTO" ? "#,##0.00" : "0.##";
                return;
            default:
                celda.Value = valor.ToString();
                return;
        }
    }

    private static string Formato(ColumnaReporteDto columna, object valor)
    {
        return valor switch
        {
            null => "",
            DateOnly fecha => fecha.ToString("dd/MM/yyyy"),
            decimal or int or double when columna.Tipo == "MONTO" => Convert.ToDecimal(valor).ToString("#,##0.00", CultureInfo.InvariantCulture),
            decimal or int or double => Convert.ToDecimal(valor).ToString("0.##", CultureInfo.InvariantCulture),
            _ => valor.ToString()
        };
    }

    private static decimal Sumar(List<FilaReporteDto> filas, int indice)
    {
        return filas.Sum(f => f.Valores[indice] is decimal or int or double ? Convert.ToDecimal(f.Valores[indice]) : 0);
    }

    private static bool HaySumas(ReporteTablaDto reporte)
    {
        return reporte.Columnas.Any(c => c.Sumar);
    }

    private static List<(string Nombre, List<FilaReporteDto> Filas)> Grupos(ReporteTablaDto reporte)
    {
        var grupos = new List<(string Nombre, List<FilaReporteDto> Filas)>();
        foreach (var registro in reporte.Filas)
        {
            if (grupos.Count == 0 || grupos[^1].Nombre != registro.Grupo)
            {
                grupos.Add((registro.Grupo, []));
            }
            grupos[^1].Filas.Add(registro);
        }
        return grupos;
    }

    private static string NombreHoja(string titulo)
    {
        var limpio = new string(titulo.Where(c => !"[]:*?/\\".Contains(c)).ToArray());
        return limpio.Length > 31 ? limpio[..31] : limpio;
    }

    private static void TablaPlanilla(IContainer contenedor, ReportePlanillaDto reporte)
    {
        contenedor.Table(tabla =>
        {
            tabla.ColumnsDefinition(c =>
            {
                c.ConstantColumn(40);
                c.RelativeColumn(3);
                c.RelativeColumn(2);
                c.ConstantColumn(22);
                c.ConstantColumn(30);
                for (var i = 0; i < 10; i++)
                {
                    c.ConstantColumn(50);
                }
            });

            tabla.Header(h =>
            {
                for (var i = 0; i < TitulosPlanilla.Length; i++)
                {
                    var celda = h.Cell().Background(Colors.Grey.Lighten2).BorderBottom(1).Padding(2).AlignMiddle();
                    if (i >= 3)
                    {
                        celda = celda.AlignRight();
                    }
                    celda.Text(TitulosPlanilla[i]).Bold();
                }
            });

            foreach (var departamento in reporte.Departamentos)
            {
                tabla.Cell().ColumnSpan((uint)TitulosPlanilla.Length).PaddingTop(6).PaddingBottom(2)
                    .Text(departamento.NombreDepartamento).Bold().FontSize(8);

                foreach (var empleado in departamento.Filas)
                {
                    Celda(tabla, empleado.CodigoEmpleado);
                    Celda(tabla, empleado.NombreCompleto);
                    Celda(tabla, empleado.NombrePuesto);
                    Celda(tabla, empleado.DiasPagar.ToString(), true);
                    Celda(tabla, empleado.HorasExtra.ToString("0.##", CultureInfo.InvariantCulture), true);
                    foreach (var monto in MontosFila(empleado))
                    {
                        Celda(tabla, Monto(monto), true);
                    }
                }

                FilaTotales(tabla, $"Subtotal {departamento.NombreDepartamento} ({departamento.Subtotal.CantidadEmpleados})", departamento.Subtotal);
            }

            FilaTotales(tabla, $"TOTAL GENERAL ({reporte.Total.CantidadEmpleados})", reporte.Total);
        });
    }

    private static void Celda(TableDescriptor tabla, string texto, bool derecha = false)
    {
        var celda = tabla.Cell().BorderBottom(0.3f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(2).PaddingHorizontal(1.5f);
        if (derecha)
        {
            celda = celda.AlignRight();
        }
        celda.Text(texto);
    }

    private static void FilaTotales(TableDescriptor tabla, string titulo, TotalesReporteDto totales)
    {
        tabla.Cell().ColumnSpan(5).BorderTop(0.8f).Padding(2).Text(titulo).Bold();
        foreach (var monto in MontosTotales(totales))
        {
            tabla.Cell().BorderTop(0.8f).Padding(2).AlignRight().Text(Monto(monto)).Bold();
        }
    }

    private static void CuotaPatronalPdf(IContainer contenedor, CuotaPatronalDto cuota)
    {
        contenedor.Width(220).Table(tabla =>
        {
            tabla.ColumnsDefinition(c =>
            {
                c.RelativeColumn(2);
                c.RelativeColumn();
            });
            tabla.Cell().ColumnSpan(2).BorderBottom(1).Padding(2).Text("CUOTA PATRONAL").Bold();
            LineaMonto(tabla, "IGSS patronal", cuota.IgssPatronal);
            tabla.Cell().BorderTop(0.8f).Padding(2).Text("Total").Bold();
            tabla.Cell().BorderTop(0.8f).Padding(2).AlignRight().Text(Monto(cuota.Total)).Bold();
        });
    }

    private static void BoletaPdf(IContainer contenedor, EncabezadoReporteDto encabezado, BoletaDto boleta)
    {
        contenedor.PaddingBottom(14).Column(columna =>
        {
            columna.Item().Element(c => EncabezadoPdf(c, encabezado, "BOLETA DE PAGO"));

            columna.Item().PaddingVertical(6).Table(tabla =>
            {
                tabla.ColumnsDefinition(c =>
                {
                    c.ConstantColumn(70);
                    c.RelativeColumn();
                    c.ConstantColumn(70);
                    c.RelativeColumn();
                });
                DatoEmpleado(tabla, "Código:", boleta.CodigoEmpleado);
                DatoEmpleado(tabla, "Empleado:", boleta.NombreCompleto);
                DatoEmpleado(tabla, "Puesto:", boleta.NombrePuesto);
                DatoEmpleado(tabla, "Departamento:", boleta.NombreDepartamento);
                DatoEmpleado(tabla, "Días pagados:", boleta.DiasPagar.ToString());
                DatoEmpleado(tabla, "Forma de pago:", boleta.TipoPago);
            });

            columna.Item().Row(fila =>
            {
                fila.RelativeItem().Element(c => LineasBoleta(c, "INGRESOS", boleta.Ingresos, "Total ingresos", boleta.TotalIngresos));
                fila.ConstantItem(16);
                fila.RelativeItem().Element(c => LineasBoleta(c, "DESCUENTOS", boleta.Descuentos, "Total descuentos", boleta.TotalDescuentos));
            });

            columna.Item().PaddingTop(8).Background(Colors.Grey.Lighten3).Padding(4).Row(fila =>
            {
                fila.RelativeItem().Text("LÍQUIDO A RECIBIR").Bold().FontSize(10);
                fila.RelativeItem().AlignRight().Text($"Q {Monto(boleta.LiquidoRecibir)}").Bold().FontSize(10);
            });

            columna.Item().PaddingTop(34).AlignCenter().Width(220).Element(c => Firma(c, $"Recibí conforme: {boleta.NombreCompleto}"));
            columna.Item().PaddingTop(14).LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
        });
    }

    private static void DatoEmpleado(TableDescriptor tabla, string etiqueta, string valor)
    {
        tabla.Cell().PaddingVertical(1).Text(etiqueta).Bold();
        tabla.Cell().PaddingVertical(1).Text(valor ?? "");
    }

    private static void LineasBoleta(IContainer contenedor, string titulo, List<LineaBoletaDto> lineas, string tituloTotal, decimal total)
    {
        contenedor.Table(tabla =>
        {
            tabla.ColumnsDefinition(c =>
            {
                c.RelativeColumn(3);
                c.RelativeColumn();
            });
            tabla.Cell().ColumnSpan(2).BorderBottom(1).Padding(2).Text(titulo).Bold();
            foreach (var linea in lineas)
            {
                tabla.Cell().Padding(2).Text(t =>
                {
                    t.Span(linea.Concepto);
                    if (!string.IsNullOrWhiteSpace(linea.Detalle))
                    {
                        t.Span($" ({linea.Detalle})").FontColor(Colors.Grey.Darken1);
                    }
                });
                tabla.Cell().Padding(2).AlignRight().Text(Monto(linea.Monto));
            }
            tabla.Cell().BorderTop(0.8f).Padding(2).Text(tituloTotal).Bold();
            tabla.Cell().BorderTop(0.8f).Padding(2).AlignRight().Text(Monto(total)).Bold();
        });
    }

    private static void EncabezadoPdf(IContainer contenedor, EncabezadoReporteDto encabezado, string titulo)
    {
        contenedor.BorderBottom(1).PaddingBottom(4).Row(fila =>
        {
            fila.RelativeItem().Column(c =>
            {
                c.Item().Text(encabezado.NombreEmpresa ?? "").Bold().FontSize(11);
                c.Item().Text($"NIT: {encabezado.NitEmpresa}");
                c.Item().Text(encabezado.DireccionEmpresa ?? "");
            });
            fila.RelativeItem().AlignRight().Column(c =>
            {
                c.Item().AlignRight().Text(titulo).Bold().FontSize(11);
                c.Item().AlignRight().Text($"{encabezado.Codigo} - {encabezado.Descripcion}");
                c.Item().AlignRight().Text($"Periodo: {Periodo(encabezado)} ({encabezado.FechaInicio:dd/MM/yyyy} al {encabezado.FechaFin:dd/MM/yyyy})");
                var estado = c.Item().AlignRight().Text(Estado(encabezado));
                if (encabezado.Preliminar)
                {
                    estado.Bold().FontColor(Colors.Red.Medium);
                }
            });
        });
    }

    private static void LineaMonto(TableDescriptor tabla, string concepto, decimal monto)
    {
        tabla.Cell().Padding(2).Text(concepto);
        tabla.Cell().Padding(2).AlignRight().Text(Monto(monto));
    }

    private static void Firma(IContainer contenedor, string texto)
    {
        contenedor.Column(c =>
        {
            c.Item().LineHorizontal(0.5f);
            c.Item().AlignCenter().Text(texto);
        });
    }

    private static void PiePagina(IContainer contenedor)
    {
        contenedor.AlignCenter().Text(t =>
        {
            t.Span("Página ");
            t.CurrentPageNumber();
            t.Span(" de ");
            t.TotalPages();
        });
    }

    private static void MarcaPreliminar(PageDescriptor pagina, EncabezadoReporteDto encabezado)
    {
        if (encabezado.Preliminar)
        {
            pagina.Background().AlignCenter().AlignMiddle().Rotate(-30)
                .Text("PRELIMINAR").FontSize(90).Bold().FontColor(Colors.Red.Lighten5);
        }
    }

    private static decimal[] MontosFila(FilaPlanillaDto f)
    {
        return [f.SalarioDevengado, f.MontoHorasExtra, f.Bonificacion, f.OtrosIngresos, f.TotalIngresos,
            f.IgssLaboral, f.Isr, f.OtrosDescuentos, f.TotalDescuentos, f.LiquidoRecibir];
    }

    private static decimal[] MontosTotales(TotalesReporteDto t)
    {
        return [t.SalarioDevengado, t.MontoHorasExtra, t.Bonificacion, t.OtrosIngresos, t.TotalIngresos,
            t.IgssLaboral, t.Isr, t.OtrosDescuentos, t.TotalDescuentos, t.LiquidoRecibir];
    }

    private static void EscribirMontos(IXLWorksheet hoja, int fila, decimal[] montos)
    {
        for (var i = 0; i < montos.Length; i++)
        {
            hoja.Cell(fila, i + 6).Value = montos[i];
            hoja.Cell(fila, i + 6).Style.NumberFormat.Format = "#,##0.00";
        }
    }

    private static string Monto(decimal monto)
    {
        return monto.ToString("#,##0.00", CultureInfo.InvariantCulture);
    }

    private static string Periodo(EncabezadoReporteDto encabezado)
    {
        return $"{Meses[encabezado.Mes - 1]} {encabezado.Anio}";
    }

    private static string Estado(EncabezadoReporteDto encabezado)
    {
        return encabezado.Preliminar
            ? "PRELIMINAR (planilla no cerrada)"
            : $"Cerrada el {encabezado.FechaCierre:dd/MM/yyyy HH:mm} por {encabezado.UsuarioCierre}";
    }
}
