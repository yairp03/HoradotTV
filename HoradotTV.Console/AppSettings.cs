namespace HoradotTV.Console;

public class AppSettings
{
    [JsonIgnore] public static AppSettings Default { get; } = LoadSettings();

    public string? LastPath { get; set; }
    public Dictionary<string, (string username, string password)> Credentials { get; init; } = new();
    public bool ForceDownload { get; set; }

    private static AppSettings LoadSettings()
    {
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            Constants.SettingsFileName);

        if (File.Exists(path))
        {
            return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path),
                new JsonSerializerOptions() { IncludeFields = true }) ?? new AppSettings();
        }

        return new AppSettings();
    }

    public async Task SaveAsync()
    {
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            Constants.SettingsFileName);
        await File.WriteAllTextAsync(path,
            JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true }));
    }

    public Task SaveCredentialsAsync(string providerName, string username, string password)
    {
        Credentials[providerName] = (username, password);
        return SaveAsync();
    }

    public Task ResetCredentialsAsync(string providerName) =>
        SaveCredentialsAsync(providerName, string.Empty, string.Empty);
}
