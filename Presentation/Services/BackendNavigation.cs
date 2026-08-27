namespace EcoHuellaApp.Presentation.Services;

public static class BackendNavigation
{
    public static async Task PushAsync<TPage>(ContentPage currentPage)
        where TPage : Page
    {
        var services = Application.Current?.Handler?.MauiContext?.Services;
        var targetPage = services?.GetService<TPage>();

        if (targetPage is null)
        {
            await currentPage.DisplayAlertAsync(
                "Navegación",
                "No se pudo abrir el módulo solicitado.",
                "Aceptar");
            return;
        }

        if (targetPage is ContentPage contentPage)
            NavegacionInferior.AgregarBotonPerfil(contentPage);

        await currentPage.Navigation.PushAsync(targetPage);
    }
}
