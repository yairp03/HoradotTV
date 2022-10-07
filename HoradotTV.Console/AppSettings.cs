namespace HoradotTV.Console;

public class AppSettings
{
    [JsonIgnore]
    public static AppSettings Default { get; } = LoadSettings();
    private const string filePath = "appsettings.json";

    public string? LastPath { get; set; }
    public string? SdarotUsername { get; set; }
    public string? SdarotPassword { get; set; }
    public bool ForceDownload { get; set; } = false;

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

    public void Save()
    {
        var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, filePath);
        File.WriteAllText(path, JsonSerializer.Serialize(this, options: new JsonSerializerOptions() { WriteIndented = true }));
    }

    public void SaveCredentials(string username, string password)
    {
        SdarotUsername = username;
        SdarotPassword = password;
        Save();
    }

    public void ResetCredentials() => SaveCredentials(string.Empty, string.Empty);
}
