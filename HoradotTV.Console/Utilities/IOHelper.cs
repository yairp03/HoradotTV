namespace HoradotTV.Console.Utilities;

internal static class IOHelper
{
    public static async Task<string> SelectMediaLibraryLocationAsync()
    {
        string path;
        do
        {
            var settings = AppSettings.Default;
            string? defaultPath = !string.IsNullOrWhiteSpace(settings.LastPath)
                ? settings.LastPath
                : Constants.DefaultDownloadLocation;
            path = IOUtils.Input($"\nEnter path for media library (empty - {defaultPath}): ").Trim();
            if (string.IsNullOrWhiteSpace(path))
            {
                path = defaultPath;
            }

            try
            {
                string fullPath = Path.GetFullPath(path);
                if (!File.GetAttributes(fullPath).HasFlag(FileAttributes.Directory))
                {
                    IOUtils.Print("Please enter a path to a directory.");
                    path = "";
                }
                else
                {
                    path = fullPath;
                    settings.LastPath = path;
                    await settings.SaveAsync();
                }
            }

            catch
            {
                IOUtils.Print("Please enter a valid path.");
                path = "";
            }
        } while (string.IsNullOrWhiteSpace(path));

        return path;
    }

    public static MainMenuOption MainMenuChoose()
    {
        IOUtils.Print($"{Environment.NewLine}{Menus.MainMenu}");
        return (MainMenuOption)IOUtils.ChooseOptionRange(Enum.GetNames(typeof(MainMenuOption)).Length - 1);
    }

    public static Mode ChooseMode()
    {
        IOUtils.Print($"{Environment.NewLine}{Menus.ModesMenu}");
        return (Mode)IOUtils.ChooseOptionRange(Enum.GetNames(typeof(Mode)).Length - 1, "Choose a mode");
    }

    public static void SummarizeDownload(int total, IEnumerable<MediaInformation>? failed = null)
    {
        var failedList = failed?.ToList() ?? new List<MediaInformation>();
        int success = total - failedList.Count;
        int successPercentage = total > 0 ? (int)((double)success / total * 100) : 0;
        IOUtils.Print("\nDownload Summary:");
        IOUtils.Print($"Total   = {total}");
        IOUtils.Print($"Success = {success}\t({successPercentage}%)");
        IOUtils.Print($"Fail    = {failedList.Count}\t({100 - successPercentage}%)");
        if (!failedList.Any())
        {
            return;
        }

        IOUtils.Print("Failed media:");
        foreach (var media in failedList)
        {
            IOUtils.Print($"\t{media}");
        }
    }
}
