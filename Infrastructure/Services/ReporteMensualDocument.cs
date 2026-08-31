using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;
using Colors = MigraDocCore.DocumentObjectModel.Colors;

namespace EcoHuellaApp.Infrastructure.Services;

public static class ReporteMensualDocument
{
    public static Document Construir(ReporteMensualData data)
    {
        var document = new Document();
        document.Info.Title = "Reporte mensual EcoHuella";

        var estiloNormal = document.Styles["Normal"];
        estiloNormal.Font.Name = EcoHuellaFontResolver.FamilyName;
        estiloNormal.Font.Size = 9;

        var section = document.AddSection();
        section.PageSetup.PageFormat = PageFormat.A4;
        section.PageSetup.TopMargin = Unit.FromCentimeter(1.5);
        section.PageSetup.BottomMargin = Unit.FromCentimeter(1.5);
        section.PageSetup.LeftMargin = Unit.FromCentimeter(1.5);
        section.PageSetup.RightMargin = Unit.FromCentimeter(1.5);

        var titulo = section.AddParagraph("EcoHuella - Reporte mensual");
        titulo.Format.Font.Name = EcoHuellaFontResolver.FamilyName;
        titulo.Format.Font.Size = 16;
        titulo.Format.Font.Bold = true;

        var subtitulo = section.AddParagraph($"Del {data.Desde:dd/MM/yyyy} al {data.Hasta:dd/MM/yyyy}");
        subtitulo.Format.Font.Color = Colors.Gray;
        subtitulo.Format.SpaceAfter = Unit.FromCentimeter(0.5);

        AgregarCasas(section, data);
        AgregarRecolecciones(section, data);
        AgregarComposteras(section, data);
        AgregarBiodigestores(section, data);
        AgregarSacos(section, data);

        return document;
    }

    private static void AgregarTitulo(Section section, string texto)
    {
        var parrafo = section.AddParagraph(texto);
        parrafo.Format.Font.Size = 13;
        parrafo.Format.Font.Bold = true;
        parrafo.Format.SpaceBefore = Unit.FromCentimeter(0.6);
        parrafo.Format.SpaceAfter = Unit.FromCentimeter(0.2);
    }

    private static void AgregarSinDatos(Section section)
    {
        var parrafo = section.AddParagraph("Sin registros en el periodo.");
        parrafo.Format.Font.Italic = true;
        parrafo.Format.Font.Color = Colors.Gray;
    }

    private static Table CrearTabla(Section section, params (string Texto, double AnchoCm)[] columnas)
    {
        var table = section.AddTable();
        table.Borders.Width = 0.5;
        table.Borders.Color = Colors.LightGray;
        table.Format.Font.Size = 8;

        foreach (var columna in columnas)
        {
            var col = table.AddColumn();
            col.Width = Unit.FromCentimeter(columna.AnchoCm);
        }

        var encabezado = table.AddRow();
        encabezado.Shading.Color = Colors.WhiteSmoke;
        encabezado.Format.Font.Bold = true;

        for (var i = 0; i < columnas.Length; i++)
        {
            encabezado.Cells[i].AddParagraph(columnas[i].Texto);
        }

        return table;
    }

    private static void AgregarCasas(Section section, ReporteMensualData data)
    {
        AgregarTitulo(section, "Clientes (casas)");

        if (data.Casas.Count == 0)
        {
            AgregarSinDatos(section);
            return;
        }

        var table = CrearTabla(section,
            ("Responsable", 4),
            ("Dirección", 6),
            ("Sector", 3),
            ("Activo", 2));

        foreach (var casa in data.Casas)
        {
            var fila = table.AddRow();
            fila.Cells[0].AddParagraph(casa.NombreResponsable);
            fila.Cells[1].AddParagraph(casa.Direccion);
            fila.Cells[2].AddParagraph(casa.Sector ?? "-");
            fila.Cells[3].AddParagraph(casa.Estado ? "Sí" : "No");
        }
    }

    private static void AgregarRecolecciones(Section section, ReporteMensualData data)
    {
        AgregarTitulo(section, "Recolecciones realizadas");

        if (data.Recolecciones.Count == 0)
        {
            AgregarSinDatos(section);
            return;
        }

        var table = CrearTabla(section,
            ("Fecha", 3),
            ("Casa", 5),
            ("Punto de recolección", 5),
            ("Cubetas", 2),
            ("Litros", 2),
            ("Masa (kg)", 2));

        foreach (var recoleccion in data.Recolecciones.OrderBy(r => r.Fecha))
        {
            var fila = table.AddRow();
            fila.Cells[0].AddParagraph(recoleccion.Fecha is { } fecha ? fecha.ToString("dd/MM/yyyy") : "-");
            fila.Cells[1].AddParagraph(recoleccion.Casa?.NombreResponsable ?? $"#{recoleccion.CasaId}");
            fila.Cells[2].AddParagraph(recoleccion.PuntoRecoleccion?.Direccion ?? $"#{recoleccion.PuntoRecoleccionId}");
            fila.Cells[3].AddParagraph(recoleccion.CantidadCubetas.ToString());
            fila.Cells[4].AddParagraph(recoleccion.LitrosEstimados.ToString("N0"));
            fila.Cells[5].AddParagraph(recoleccion.MasaEstimada.ToString("N1"));
        }
    }

    private static void AgregarComposteras(Section section, ReporteMensualData data)
    {
        AgregarTitulo(section, "Composteras artesanales");

        var resumen = section.AddParagraph(
            $"{data.Composteras.Count} composteras registradas · {data.AccionesCompostera.Count} acciones en el periodo");
        resumen.Format.Font.Size = 8;
        resumen.Format.Font.Color = Colors.Gray;
        resumen.Format.SpaceAfter = Unit.FromCentimeter(0.15);

        if (data.AccionesCompostera.Count == 0)
        {
            AgregarSinDatos(section);
            return;
        }

        var table = CrearTabla(section,
            ("Compostera", 3),
            ("Fecha", 3),
            ("Acción", 4),
            ("Elemento", 4));

        foreach (var accion in data.AccionesCompostera.OrderBy(a => a.FechaAccion))
        {
            var fila = table.AddRow();
            fila.Cells[0].AddParagraph($"#{accion.ComposteraArtesanalId}");
            fila.Cells[1].AddParagraph(accion.FechaAccion is { } fecha ? fecha.ToString("dd/MM/yyyy") : "-");
            fila.Cells[2].AddParagraph(accion.TipoAccion ?? "-");
            fila.Cells[3].AddParagraph(accion.TipoElemento ?? "-");
        }
    }

    private static void AgregarBiodigestores(Section section, ReporteMensualData data)
    {
        AgregarTitulo(section, "Biodigestores");

        var resumen = section.AddParagraph(
            $"{data.Biodigestores.Count} biodigestores registrados · {data.Procesos.Count} procesos iniciados en el periodo");
        resumen.Format.Font.Size = 8;
        resumen.Format.Font.Color = Colors.Gray;
        resumen.Format.SpaceAfter = Unit.FromCentimeter(0.15);

        if (data.Procesos.Count == 0)
        {
            AgregarSinDatos(section);
            return;
        }

        var table = CrearTabla(section,
            ("Proceso", 2),
            ("Biodigestor", 3),
            ("Inicio", 3),
            ("Estado", 3),
            ("CH4 / CO2 evitado (kg)", 5));

        foreach (var proceso in data.Procesos.OrderBy(p => p.FechaInicio))
        {
            var estado = proceso.EstadoFinalizado ? "Finalizado" : proceso.EstadoLlenado ? "Lleno" : "Activo";

            var fila = table.AddRow();
            fila.Cells[0].AddParagraph($"#{proceso.Id}");
            fila.Cells[1].AddParagraph($"#{proceso.BiodigestorId}");
            fila.Cells[2].AddParagraph(proceso.FechaInicio is { } fecha ? fecha.ToString("dd/MM/yyyy") : "-");
            fila.Cells[3].AddParagraph(estado);
            fila.Cells[4].AddParagraph($"{proceso.MetanoEvitado:N1} / {proceso.CarbonoEvitado:N1}");
        }
    }

    private static void AgregarSacos(Section section, ReporteMensualData data)
    {
        AgregarTitulo(section, "Sacos de compost");

        if (data.Sacos.Count == 0)
        {
            AgregarSinDatos(section);
            return;
        }

        var table = CrearTabla(section,
            ("Saco", 2),
            ("Fecha", 3),
            ("Estado", 3),
            ("Motivo / Cliente", 6));

        foreach (var saco in data.Sacos.OrderBy(s => s.Fecha))
        {
            var motivoTexto = saco.Motivo is null
                ? "-"
                : $"{saco.Motivo}{(string.IsNullOrWhiteSpace(saco.ClienteVenta) ? "" : $" - {saco.ClienteVenta}")}";

            var fila = table.AddRow();
            fila.Cells[0].AddParagraph($"#{saco.Id}");
            fila.Cells[1].AddParagraph(saco.Fecha is { } fecha ? fecha.ToString("dd/MM/yyyy") : "-");
            fila.Cells[2].AddParagraph(saco.EstadoTexto);
            fila.Cells[3].AddParagraph(motivoTexto);
        }
    }
}
