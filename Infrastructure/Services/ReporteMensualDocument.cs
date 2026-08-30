using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using IContainer = QuestPDF.Infrastructure.IContainer;
using Colors = QuestPDF.Helpers.Colors;

namespace EcoHuellaApp.Infrastructure.Services;

public sealed class ReporteMensualDocument : IDocument
{
    private readonly ReporteMensualData _data;

    public ReporteMensualDocument(ReporteMensualData data) => _data = data;

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(30);
            page.DefaultTextStyle(x => x.FontSize(10));

            page.Header().Column(col =>
            {
                col.Item().Text("EcoHuella — Reporte mensual").FontSize(18).Bold();
                col.Item().Text($"Del {_data.Desde:dd/MM/yyyy} al {_data.Hasta:dd/MM/yyyy}").FontSize(10);
            });

            page.Content().PaddingVertical(10).Column(col =>
            {
                col.Spacing(16);
                col.Item().Element(ComponerCasas);
                col.Item().Element(ComponerComposteras);
                col.Item().Element(ComponerBiodigestores);
                col.Item().Element(ComponerSacos);
            });

            page.Footer().AlignCenter().Text(x =>
            {
                x.CurrentPageNumber();
                x.Span(" / ");
                x.TotalPages();
            });
        });
    }

    private void ComponerCasas(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Text("Clientes (casas)").FontSize(14).Bold();

            if (_data.Casas.Count == 0)
            {
                col.Item().Text("Sin registros.").Italic();
                return;
            }

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(2);
                    c.RelativeColumn(3);
                    c.RelativeColumn(2);
                    c.RelativeColumn(1);
                });

                table.Header(header =>
                {
                    header.Cell().Text("Responsable").Bold();
                    header.Cell().Text("Dirección").Bold();
                    header.Cell().Text("Sector").Bold();
                    header.Cell().Text("Activo").Bold();
                });

                foreach (var casa in _data.Casas)
                {
                    table.Cell().Text(casa.NombreResponsable);
                    table.Cell().Text(casa.Direccion);
                    table.Cell().Text(casa.Sector ?? "-");
                    table.Cell().Text(casa.Estado ? "Sí" : "No");
                }
            });
        });
    }

    private void ComponerComposteras(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Text("Composteras artesanales").FontSize(14).Bold();
            col.Item().Text($"{_data.Composteras.Count} composteras registradas · {_data.AccionesCompostera.Count} acciones en el periodo")
                .FontSize(9).FontColor(Colors.Grey.Darken1);

            if (_data.AccionesCompostera.Count == 0)
            {
                col.Item().Text("Sin acciones en el periodo.").Italic();
                return;
            }

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    header.Cell().Text("Compostera").Bold();
                    header.Cell().Text("Fecha").Bold();
                    header.Cell().Text("Acción").Bold();
                    header.Cell().Text("Elemento").Bold();
                });

                foreach (var accion in _data.AccionesCompostera.OrderBy(a => a.FechaAccion))
                {
                    table.Cell().Text($"#{accion.ComposteraArtesanalId}");
                    table.Cell().Text(accion.FechaAccion is { } fecha ? fecha.ToString("dd/MM/yyyy") : "-");
                    table.Cell().Text(accion.TipoAccion ?? "-");
                    table.Cell().Text(accion.TipoElemento ?? "-");
                }
            });
        });
    }

    private void ComponerBiodigestores(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Text("Biodigestores").FontSize(14).Bold();
            col.Item().Text($"{_data.Biodigestores.Count} biodigestores registrados · {_data.Procesos.Count} procesos iniciados en el periodo")
                .FontSize(9).FontColor(Colors.Grey.Darken1);

            if (_data.Procesos.Count == 0)
            {
                col.Item().Text("Sin procesos en el periodo.").Italic();
                return;
            }

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(1);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    header.Cell().Text("Proceso").Bold();
                    header.Cell().Text("Biodigestor").Bold();
                    header.Cell().Text("Inicio").Bold();
                    header.Cell().Text("Estado").Bold();
                    header.Cell().Text("CH₄ / CO₂ evitado").Bold();
                });

                foreach (var proceso in _data.Procesos.OrderBy(p => p.FechaInicio))
                {
                    var estado = proceso.EstadoFinalizado ? "Finalizado" : proceso.EstadoLlenado ? "Lleno" : "Activo";

                    table.Cell().Text($"#{proceso.Id}");
                    table.Cell().Text($"#{proceso.BiodigestorId}");
                    table.Cell().Text(proceso.FechaInicio is { } fecha ? fecha.ToString("dd/MM/yyyy") : "-");
                    table.Cell().Text(estado);
                    table.Cell().Text($"{proceso.MetanoEvitado:N1} kg / {proceso.CarbonoEvitado:N1} kg");
                }
            });
        });
    }

    private void ComponerSacos(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Text("Sacos de compost").FontSize(14).Bold();

            if (_data.Sacos.Count == 0)
            {
                col.Item().Text("Sin movimientos en el periodo.").Italic();
                return;
            }

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(1);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(3);
                });

                table.Header(header =>
                {
                    header.Cell().Text("Saco").Bold();
                    header.Cell().Text("Fecha").Bold();
                    header.Cell().Text("Estado").Bold();
                    header.Cell().Text("Motivo / Cliente").Bold();
                });

                foreach (var saco in _data.Sacos.OrderBy(s => s.Fecha))
                {
                    table.Cell().Text($"#{saco.Id}");
                    table.Cell().Text(saco.Fecha is { } fecha ? fecha.ToString("dd/MM/yyyy") : "-");
                    table.Cell().Text(saco.EstadoTexto);
                    table.Cell().Text(saco.Motivo is null ? "-" : $"{saco.Motivo}{(string.IsNullOrWhiteSpace(saco.ClienteVenta) ? "" : $" · {saco.ClienteVenta}")}");
                }
            });
        });
    }
}
