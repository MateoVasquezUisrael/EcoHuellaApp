namespace EcoHuellaApp.Presentation.Services;

using EcoHuellaApp.Presentation.Views.Front;
using Microsoft.Maui.Devices;

public static class NavegacionInferior
{
    public static void Conectar(ContentPage page)
    {
        if (page.Content is Element contenido)
        {
            ConectarElemento(contenido, page);
        }

        ConectarGestoVolver(page);
        page.Loaded += AnimarEntrada;
    }

    private static void ConectarElemento(Element elemento, ContentPage page)
    {
        if (elemento is Label label)
        {
            ConectarLabel(label, page);
        }

        foreach (Element hijo in ObtenerHijos(elemento))
        {
            ConectarElemento(hijo, page);
        }
    }

    private static IEnumerable<Element> ObtenerHijos(Element elemento)
    {
        switch (elemento)
        {
            case Border border when border.Content is Element contenido:
                yield return contenido;
                break;
            case ContentView contentView when contentView.Content is Element contenido:
                yield return contenido;
                break;
            case ScrollView scrollView when scrollView.Content is Element contenido:
                yield return contenido;
                break;
            case Layout layout:
                foreach (IView vista in layout.Children)
                {
                    if (vista is Element hijo)
                    {
                        yield return hijo;
                    }
                }
                break;
        }
    }

    private static void ConectarLabel(Label label, ContentPage page)
    {
        var destino = ObtenerDestino(label);

        if (destino is null)
        {
            return;
        }

        label.GestureRecognizers.Clear();
        label.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () =>
            {
                await AnimarToqueAsync(label);
                await NavegarAsync(destino, page);
            })
        });
    }

    private static string? ObtenerDestino(Label label)
    {
        if (label.Text is "Dashboard" or "Historial" or "Entregas" or "Procesos" or "Ajustes" or "Perfil")
        {
            return label.Text;
        }

        if (label.Text is "👤")
        {
            return "Ajustes";
        }

        if (label.Parent is not Grid grid)
        {
            return null;
        }

        var columna = Grid.GetColumn(label);
        var textoDestino = grid.Children
            .OfType<Label>()
            .FirstOrDefault(l => Grid.GetRow(l) == 1 && Grid.GetColumn(l) == columna);

        return textoDestino?.Text is "Dashboard" or "Historial" or "Entregas" or "Procesos" or "Ajustes" or "Perfil"
            ? textoDestino.Text
            : null;
    }

    private static async Task NavegarAsync(string destino, ContentPage page)
    {
        Page? nuevaPagina = destino switch
        {
            "Dashboard" when page is not vHome => new vHome(),
            "Historial" when page is not vHistorialEntregas => new vHistorialEntregas(),
            "Entregas" when page is not vGestionResiduos => new vGestionResiduos(),
            "Procesos" when page is not vProcesos => new vProcesos(),
            "Ajustes" when page is not vAjustesPerfil => new vAjustesPerfil(),
            _ => null
        };

        if (nuevaPagina is not null)
        {
            FeedbackTactil();
            await page.Navigation.PushAsync(nuevaPagina);
        }
    }

    private static void ConectarGestoVolver(ContentPage page)
    {
        var swipe = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
        swipe.Swiped += async (_, _) =>
        {
            if (page.Navigation.NavigationStack.Count <= 1)
            {
                return;
            }

            FeedbackTactil();
            await page.Navigation.PopAsync();
        };
        if (page.Content is View contenido)
        {
            contenido.GestureRecognizers.Add(swipe);
        }
    }

    private static async void AnimarEntrada(object? sender, EventArgs e)
    {
        if (sender is not ContentPage page || page.Content is null)
        {
            return;
        }

        page.Loaded -= AnimarEntrada;
        page.Content.Opacity = 0;
        page.Content.TranslationY = 10;
        await Task.WhenAll(
            page.Content.FadeTo(1, 230, Easing.CubicOut),
            page.Content.TranslateTo(0, 0, 280, Easing.CubicOut));
    }

    private static async Task AnimarToqueAsync(VisualElement elemento)
    {
        await elemento.ScaleTo(0.90, 70, Easing.CubicOut);
        await elemento.ScaleTo(1, 110, Easing.CubicOut);
    }

    private static void FeedbackTactil()
    {
        try
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }
        catch (FeatureNotSupportedException)
        {
            // Algunas computadoras no ofrecen vibración; la navegación continúa normalmente.
        }
    }
}
