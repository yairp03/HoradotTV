namespace HoradotTV.Pages;

internal static class PagesExtensions
{
    public static MauiAppBuilder ConfigurePages(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<StartupPage>()
                        .AddSingleton<MainPage>();

        return builder;
    }
}
