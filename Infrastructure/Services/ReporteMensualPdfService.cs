using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.ProcesoComposteraArtesanal;
using EcoHuellaApp.Domain.Models.ProcesoDegradacion;
using EcoHuellaApp.Domain.Models.Recoleccion;
using EcoHuellaApp.Domain.Models.Ventas;
using EcoHuellaApp.Infrastructure.Repositories.ProcesoDegradacion;
using QuestPDF.Fluent;

namespace EcoHuellaApp.Infrastructure.Services;

public sealed class ReporteMensualPdfService
{
    private readonly IRepositoryGeneric<Casa> _casaRepository;
    private readonly IRepositoryGeneric<ComposteraArtesanal> _composteraRepository;
    private readonly IRepositoryGeneric<AccionCompostera> _accionComposteraRepository;
    private readonly IRepositoryGeneric<Biodigestor> _biodigestorRepository;
    private readonly IRepositoryGeneric<ProcesoBiodigestor> _procesoRepository;
    private readonly ProcesoBiodigestorRepository _procesoRepositoryEspecifico;
    private readonly IRepositoryGeneric<SacosCompost> _sacosRepository;

    public ReporteMensualPdfService(
        IRepositoryGeneric<Casa> casaRepository,
        IRepositoryGeneric<ComposteraArtesanal> composteraRepository,
        IRepositoryGeneric<AccionCompostera> accionComposteraRepository,
        IRepositoryGeneric<Biodigestor> biodigestorRepository,
        IRepositoryGeneric<ProcesoBiodigestor> procesoRepository,
        ProcesoBiodigestorRepository procesoRepositoryEspecifico,
        IRepositoryGeneric<SacosCompost> sacosRepository)
    {
        _casaRepository = casaRepository;
        _composteraRepository = composteraRepository;
        _accionComposteraRepository = accionComposteraRepository;
        _biodigestorRepository = biodigestorRepository;
        _procesoRepository = procesoRepository;
        _procesoRepositoryEspecifico = procesoRepositoryEspecifico;
        _sacosRepository = sacosRepository;
    }

    public async Task<byte[]> GenerarAsync(DateTime desde, DateTime hasta)
    {
        var casas = await _casaRepository.ObtenerTodosAsync();
        var composteras = await _composteraRepository.ObtenerTodosAsync();

        var acciones = (await _accionComposteraRepository.ObtenerTodosAsync())
            .Where(a => a.FechaAccion is not null && a.FechaAccion.Value >= desde && a.FechaAccion.Value <= hasta)
            .ToList();

        var biodigestores = await _biodigestorRepository.ObtenerTodosAsync();

        var procesosActivos = await _procesoRepository.ObtenerTodosAsync();
        var procesosFinalizados = await _procesoRepositoryEspecifico.ObtenerFinalizadosAsync();
        var procesos = procesosActivos.Concat(procesosFinalizados)
            .Where(p => p.FechaInicio is not null && p.FechaInicio.Value >= desde && p.FechaInicio.Value <= hasta)
            .ToList();

        var sacos = (await _sacosRepository.ObtenerTodosAsync())
            .Where(s => s.Fecha is not null && s.Fecha.Value >= desde && s.Fecha.Value <= hasta)
            .ToList();

        var data = new ReporteMensualData
        {
            Desde = desde,
            Hasta = hasta,
            Casas = casas,
            Composteras = composteras,
            AccionesCompostera = acciones,
            Biodigestores = biodigestores,
            Procesos = procesos,
            Sacos = sacos
        };

        return new ReporteMensualDocument(data).GeneratePdf();
    }
}
