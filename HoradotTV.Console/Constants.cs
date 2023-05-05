namespace HoradotTV.Console;

internal static class Constants
{
    public const string SoftwareVersion = "1.4.0";

    public const int QueryMinLength = 2;

    public static readonly string DefaultDownloadLocation = KnownFolders.Downloads.Path;

    public const string SdarotTVConnectionProblemGuide = "https://github.com/yairp03/HoradotTV/wiki/SdarotTV-connection-problem";

    public static class Commands
    {
        public const string Quit = "q";
        public const string Cancel = "c";
    }
}

internal static class Menus
{
    public const string ModesMenu = "-- Download Modes --\n" +
                                      "[0] Back to start\n" +
                                      "[1] Download Episode\n" +
                                      "[2] Download Episodes\n" +
                                      "[3] Download Season\n" +
                                      "[4] Download Show";

    public const string FailedMenu = "[0] Nothing\n" +
                                      "[1] Try again\n" +
                                      "[2] Export to file";
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
