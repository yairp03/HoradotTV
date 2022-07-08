namespace HoradotTV.Services;

internal static class ServicesExtensions
{
    public static MauiAppBuilder ConfigureServices(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton(Preferences.Default)
                        .AddSingleton(Browser.Default);

        return builder;
    }
}
