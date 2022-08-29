namespace HoradotTV.Pages;

internal static class ViewModelsExtensions
{
    public static MauiAppBuilder ConfigureViewModels(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<StartupViewModel>()
                        .AddSingleton<MainViewModel>();

        return builder;
    }
}
