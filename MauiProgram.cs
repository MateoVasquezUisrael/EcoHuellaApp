using EcoHuellaApp.Data;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.Recoleccion;
using EcoHuellaApp.Domain.Models.ProcesoDegradacion;
using EcoHuellaApp.Domain.Models.ProcesoComposteraArtesanal;
using EcoHuellaApp.Domain.Models.Ventas;
using EcoHuellaApp.Infrastructure.Repositories;
using EcoHuellaApp.Infrastructure.Repositories.ProcesoDegradacion;
using EcoHuellaApp.Infrastructure.Repositories.ProcesoComposteraArtesanal;
using EcoHuellaApp.Infrastructure.Repositories.Ventas;
using EcoHuellaApp.Infrastructure.Services;
using EcoHuellaApp.Presentation.ViewModels;
using EcoHuellaApp.Presentation.Views;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using SkiaSharp.Views.Maui.Controls.Hosting;
namespace EcoHuellaApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            SQLitePCL.Batteries.Init();
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
                .UseLocalNotification()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // ── Base de datos compartida ───────────────────────────────────────
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "ecoHuella_v2.bd3");
            Console.WriteLine(dbPath);
            var database = new AppDatabase(dbPath);
            builder.Services.AddSingleton(database);

            // ── Módulo Login ──────────────────────────────────────────────────
            RegisterLoginModule(builder.Services);

            // ── Módulo Casas ──────────────────────────────────────────────────
            builder.Services.AddTransient<CasaView>();
            builder.Services.AddSingleton<IRepositoryGeneric<Casa>, CasaRepository>();

            // ── Módulo Puntos de Recolección ──────────────────────────────────
            builder.Services.AddTransient<PuntoRecoleccionView>();
            builder.Services.AddSingleton<IRepositoryGeneric<PuntoRecoleccion>, PuntoRecoleccionRepository>();

            // ── Módulo Mapas ──────────────────────────────────────────────────
            builder.Services.AddSingleton<OfflineMapTileService>();
            builder.Services.AddTransient<MapPage>();
            builder.Services.AddTransient<LocationPickerPage>();

            // ── Módulo Recolección ────────────────────────────────────────────
            builder.Services.AddTransient<RecoleccionView>();
            builder.Services.AddTransient<RecoleccionMapPage>();
            builder.Services.AddSingleton<IRepositoryGeneric<Recoleccion>, RecoleccionRepository>();
            builder.Services.AddSingleton<RecoleccionRepository>();

            // ── Módulo Proceso de Degradación ─────────────────────────────────
            builder.Services.AddTransient<BiodigestoresView>();
            builder.Services.AddTransient<ProcesosBiodigestorView>();
            builder.Services.AddTransient<ProcesosFinalizadosView>();
            builder.Services.AddSingleton<IRepositoryGeneric<Biodigestor>, BiodigestorRepository>();
            builder.Services.AddSingleton<BiodigestorRepository>();
            builder.Services.AddSingleton<IRepositoryGeneric<ProcesoBiodigestor>, ProcesoBiodigestorRepository>();
            builder.Services.AddSingleton<ProcesoBiodigestorRepository>();
            builder.Services.AddSingleton<IRepositoryGeneric<EntradasProcesoBiodigestor>, EntradasProcesoBiodigestorRepository>();
            builder.Services.AddSingleton<EntradasProcesoBiodigestorRepository>();

            // ── Módulo Proceso Compostera Artesanal ───────────────────────────
            builder.Services.AddTransient<ComposterasArtesanalesView>();
            builder.Services.AddSingleton<IRepositoryGeneric<ComposteraArtesanal>, ComposteraArtesanalRepository>();
            builder.Services.AddSingleton<ComposteraArtesanalRepository>();
            builder.Services.AddSingleton<IRepositoryGeneric<AccionCompostera>, AccionComposteraRepository>();
            builder.Services.AddSingleton<AccionComposteraRepository>();

            // ── Módulo Sacos de Compost ───────────────────────────────────────
            builder.Services.AddTransient<SacosCompostView>();
            builder.Services.AddSingleton<IRepositoryGeneric<SacosCompost>, SacosCompostRepository>();
            builder.Services.AddSingleton<SacosCompostRepository>();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }

        private static void RegisterLoginModule(IServiceCollection services)
        {
            // Sesión y navegación — Singleton: estado compartido entre ViewModels
            services.AddSingleton<IUserSessionService, UserSessionService>();
            services.AddSingleton<INavigationService, MauiNavigationService>();

            // IAuthService — implementación nativa por plataforma
#if ANDROID
            services.AddSingleton<IAuthService,
                EcoHuellaApp.Platforms.Android.FirebaseAuthService>();
#elif IOS
            services.AddSingleton<IAuthService,
                EcoHuellaApp.Platforms.iOS.FirebaseAuthService>();
#elif WINDOWS
            services.AddSingleton<IAuthService, FirebaseRestAuthService>();
#else
            services.AddSingleton<IAuthService, FakeAuthService>();
#endif

            // IUserRepository — Firestore REST en todas las plataformas reales,
            // Fake solo en MacCatalyst (sin google-services.json configurado)
#if MACCATALYST
            services.AddSingleton<IUserRepository, FakeUserRepository>();
#else
            services.AddSingleton<IUserRepository, FirestoreUserRepository>();
#endif

            // ViewModels — Transient: nueva instancia por página
            services.AddTransient<LoginViewModel>();
            services.AddTransient<ChangePasswordViewModel>();
            services.AddTransient<MainViewModel>();

            // Views — Transient: ciclo de vida ligado al ViewModel
            services.AddTransient<LoginPage>();
            services.AddTransient<ChangePasswordPage>();
            services.AddTransient<MainPage>();
        }
    }
}