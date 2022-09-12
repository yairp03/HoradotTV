namespace HoradotTV.Console;

internal static class Constants
{
    public static string SOFTWARE_VERSION = "1.0.3";

    public static int QUERY_MIN_LENGTH = 2;

    public static string DEFAULT_DOWNLOAD_LOCATION = KnownFolders.Downloads.Path;
}

internal static class Menus
{
    public static string MODES_MENU = "-- Download Modes --\n" +
                                      "[0] Back to start\n" +
                                      "[1] Download Episode\n" +
                                      "[2] Download Episodes\n" +
                                      "[3] Download Season\n" +
                                      "[4] Download Series";
}

internal enum Modes
{
    None,
    Episode,
    Episodes,
    Season,
    Series
}