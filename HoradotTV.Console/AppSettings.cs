namespace HoradotTV.Console;

public class AppSettings
{
    [JsonIgnore] public static AppSettings Default { get; } = LoadSettings();

    public string? LastPath { get; set; }
    public string? SdarotUsername { get; set; }
    public string? SdarotPassword { get; set; }
    public bool ForceDownload { get; set; }

    private static AppSettings LoadSettings()
    {
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            Constants.SettingsFileName);

        if (File.Exists(path))
        {
            return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path)) ?? new AppSettings();
        }

        return new AppSettings();
    }

    public async Task SaveAsync()
    {
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            Constants.SettingsFileName);
        await File.WriteAllTextAsync(path,
            JsonSerializer.Serialize(this, options: new JsonSerializerOptions { WriteIndented = true }));
    }

    public async Task SaveCredentialsAsync(string username, string password)
    {
        SdarotUsername = username;
        SdarotPassword = password;
        await SaveAsync();
    }

    public async Task ResetCredentialsAsync() => await SaveCredentialsAsync(string.Empty, string.Empty);
}
