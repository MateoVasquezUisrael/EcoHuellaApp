using EcoHuellaApp.Data;
using EcoHuellaApp.Domain.Models;
using EcoHuellaApp.Presentation.Views;
using EcoHuellaApp.Repositories.Implementations;
using EcoHuellaApp.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace EcoHuellaApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            //path de base de datos
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "ecoHuella.bd3");

            var database = new AppDatabase(dbPath);

            builder.Services.AddSingleton(database);
            builder.Services.AddTransient<CasaView>();
            builder.Services.AddSingleton<IRepositoryGeneric<Casa>,CasaRepository>();   
            builder.Services.AddSingleton<RecoleccionRepositoriy>();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }
}
