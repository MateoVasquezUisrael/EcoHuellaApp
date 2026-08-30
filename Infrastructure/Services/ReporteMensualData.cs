using EcoHuellaApp.Domain.Models.ProcesoComposteraArtesanal;
using EcoHuellaApp.Domain.Models.ProcesoDegradacion;
using EcoHuellaApp.Domain.Models.Recoleccion;
using EcoHuellaApp.Domain.Models.Ventas;

namespace EcoHuellaApp.Infrastructure.Services;

public sealed class ReporteMensualData
{
    public DateTime Desde { get; init; }
    public DateTime Hasta { get; init; }

    public List<Casa> Casas { get; init; } = [];
    public List<ComposteraArtesanal> Composteras { get; init; } = [];
    public List<AccionCompostera> AccionesCompostera { get; init; } = [];
    public List<Biodigestor> Biodigestores { get; init; } = [];
    public List<ProcesoBiodigestor> Procesos { get; init; } = [];
    public List<SacosCompost> Sacos { get; init; } = [];
}
