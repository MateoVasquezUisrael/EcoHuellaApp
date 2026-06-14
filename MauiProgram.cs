using EcoHuellaApp.Data;
using EcoHuellaApp.Domain.Interfaces;
using EcoHuellaApp.Domain.Models;
using EcoHuellaApp.Infrastructure.Repositories;
using EcoHuellaApp.Infrastructure.Services;
using EcoHuellaApp.Presentation.ViewModels;
using EcoHuellaApp.Presentation.Views;
using EcoHuellaApp.Repositories;
using EcoHuellaApp.Repositories.Implementations;
using EcoHuellaApp.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;

namespace EcoHuellaApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseLocalNotification()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // ── Base de datos compartida ───────────────────────────────────────
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "ecoHuella.bd3");
            var database = new AppDatabase(dbPath);
            builder.Services.AddSingleton(database);

            // ── Módulo Login ──────────────────────────────────────────────────
            RegisterLoginModule(builder.Services);

            // ── Módulo Casas ──────────────────────────────────────────────────
            builder.Services.AddTransient<CasaView>();
            builder.Services.AddSingleton<IRepositoryGeneric<Casa>, CasaRepository>();

            // ── Módulo Recolección ────────────────────────────────────────────
            builder.Services.AddTransient<RecoleccionView>();
            builder.Services.AddSingleton<IRepositoryGeneric<Recoleccion>, RecoleccionRepository>();
            builder.Services.AddSingleton<RecoleccionRepository>();

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