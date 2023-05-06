namespace HoradotTV.Console;

public class AppSettings
{
    [JsonIgnore]
    public static AppSettings Default { get; } = LoadSettings();
    private const string filePath = "appsettings.json";

    public string? LastPath { get; set; }
    public string? SdarotUsername { get; set; }
    public string? SdarotPassword { get; set; }
    public bool ForceDownload { get; set; }

    private static AppSettings LoadSettings()
    {
        var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, filePath);
        try
        {
            return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path)) ?? new();
        }
        catch (FileNotFoundException)
        {
            return new();
        }
    }

    public async Task SaveAsync()
    {
        var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, filePath);
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(this, options: new JsonSerializerOptions { WriteIndented = true }));
    }

    public async Task SaveCredentialsAsync(string username, string password)
    {
        SdarotUsername = username;
        SdarotPassword = password;
        await SaveAsync();
    }

    public async Task ResetCredentialsAsync() => await SaveCredentialsAsync(string.Empty, string.Empty);
}
