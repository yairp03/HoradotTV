namespace HoradotTV.Console;

internal static class Constants
{
    public const string SoftwareVersion = "2.0.0";

    public const int QueryMinLength = 2;
    public const string SettingsFileName = "appsettings.json";

    public const string DefaultMediaFormat = "mp4";

    public const string MediaFileExtension = "media";

    public static readonly string DefaultDownloadLocation = KnownFolders.Downloads.Path;

    public static class Commands
    {
        public const string Cancel = "c";
    }
}

internal static class Menus
{
    public const string MainMenu = $"""
        -- Main Menu --
        [0] Quit
        [1] Search Show
        [2] Download from .{Constants.MediaFileExtension} file
        """;

    public const string ModesMenu = """
        -- Download Modes --
        [0] Back to start
        [1] Download Episode
        [2] Download Episodes
        [3] Download Season
        [4] Download Show
        """;

    public const string FailedMenu = """
        [0] Nothing
        [1] Try again
        [2] Export to file
        """;
}

internal enum MainMenuOption
{
    Quit,
    Search,
    DownloadFromFile
}

internal enum Mode
{
    None,
    Episode,
    Episodes,
    Season,
    Show
}

internal enum FailedOption
{
    None,
    TryAgain,
    Export
}
