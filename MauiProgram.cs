using EcoHuellaApp.Data;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models.ProcesoComposteraArtesanal;
using EcoHuellaApp.Domain.Models.ProcesoDegradacion;
using EcoHuellaApp.Domain.Models.Recoleccion;
using EcoHuellaApp.Domain.Models.Ventas;
using EcoHuellaApp.Infrastructure.Repositories;
using EcoHuellaApp.Infrastructure.Repositories.ProcesoComposteraArtesanal;
using EcoHuellaApp.Infrastructure.Repositories.ProcesoDegradacion;
using EcoHuellaApp.Infrastructure.Repositories.Ventas;
using EcoHuellaApp.Infrastructure.Services;
using EcoHuellaApp.Presentation.ViewModels;
using EcoHuellaApp.Presentation.Views;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using QuestPDF.Infrastructure;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace EcoHuellaApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            SQLitePCL.Batteries.Init();
            QuestPDF.Settings.License = LicenseType.Community;

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

            // Base de datos.
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "ecoHuella_v2.bd3");
            var database = new AppDatabase(dbPath);
            builder.Services.AddSingleton(database);

            // Inicio de sesión.
            RegisterLoginModule(builder.Services);

            // Casas.
            builder.Services.AddTransient<CasaView>();
            builder.Services.AddSingleton<IRepositoryGeneric<Casa>, CasaRepository>();

            // Puntos de recolección.
            builder.Services.AddTransient<PuntoRecoleccionView>();
            builder.Services.AddSingleton<IRepositoryGeneric<PuntoRecoleccion>, PuntoRecoleccionRepository>();

            // Mapas.
            builder.Services.AddSingleton<OfflineMapTileService>();
            builder.Services.AddTransient<MapPage>();
            builder.Services.AddTransient<LocationPickerPage>();

            // Recolección.
            builder.Services.AddTransient<RecoleccionView>();
            builder.Services.AddTransient<RecoleccionMapPage>();
            builder.Services.AddSingleton<IRepositoryGeneric<Recoleccion>, RecoleccionRepository>();
            builder.Services.AddSingleton<RecoleccionRepository>();

            // Degradación.
            builder.Services.AddTransient<BiodigestoresView>();
            builder.Services.AddTransient<ProcesosBiodigestorView>();
            builder.Services.AddTransient<ProcesosFinalizadosView>();
            builder.Services.AddSingleton<IRepositoryGeneric<Biodigestor>, BiodigestorRepository>();
            builder.Services.AddSingleton<BiodigestorRepository>();
            builder.Services.AddSingleton<IRepositoryGeneric<ProcesoBiodigestor>, ProcesoBiodigestorRepository>();
            builder.Services.AddSingleton<ProcesoBiodigestorRepository>();
            builder.Services.AddSingleton<IRepositoryGeneric<EntradasProcesoBiodigestor>, EntradasProcesoBiodigestorRepository>();
            builder.Services.AddSingleton<EntradasProcesoBiodigestorRepository>();

            // Compostera artesanal.
            builder.Services.AddTransient<ComposterasArtesanalesView>();
            builder.Services.AddSingleton<IRepositoryGeneric<ComposteraArtesanal>, ComposteraArtesanalRepository>();
            builder.Services.AddSingleton<ComposteraArtesanalRepository>();
            builder.Services.AddSingleton<IRepositoryGeneric<AccionCompostera>, AccionComposteraRepository>();
            builder.Services.AddSingleton<AccionComposteraRepository>();

            // Sacos de compost.
            builder.Services.AddTransient<SacosCompostView>();
            builder.Services.AddSingleton<IRepositoryGeneric<SacosCompost>, SacosCompostRepository>();
            builder.Services.AddSingleton<SacosCompostRepository>();

            // Reportes.
            builder.Services.AddTransient<ReporteMensualPdfService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }

        private static void RegisterLoginModule(IServiceCollection services)
        {
            // Sesión y navegación.
            services.AddSingleton<IUserSessionService, UserSessionService>();
            services.AddSingleton<INavigationService, MauiNavigationService>();
            services.AddSingleton<IMockPasswordService, MockPasswordService>();

            // Autenticación por plataforma.
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

            // Usuarios en Firestore.
#if MACCATALYST
            services.AddSingleton<IUserRepository, FakeUserRepository>();
#else
            services.AddSingleton<IUserRepository, FirestoreUserRepository>();
#endif

            // ViewModels.
            services.AddTransient<LoginViewModel>();
            services.AddTransient<ChangePasswordViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<RegistrationViewModel>();

            // Vistas.
            services.AddTransient<LoginPage>();
            services.AddTransient<ChangePasswordPage>();
            services.AddTransient<MainPage>();
            services.AddTransient<RegistrationPage>();
            services.AddTransient<GuestDemoPage>();
        }
    }
}
