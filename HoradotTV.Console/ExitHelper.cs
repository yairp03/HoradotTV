namespace HoradotTV.Console;
internal static class ExitHelper
{
    [DllImport("Kernel32")]
    public static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

    public delegate bool EventHandler(CtrlType sig);
    private static EventHandler? _handler;
    private static Action? func;

    public enum CtrlType
    {
        CTRL_C_EVENT = 0,
        CTRL_BREAK_EVENT = 1,
        CTRL_CLOSE_EVENT = 2,
        CTRL_LOGOFF_EVENT = 5,
        CTRL_SHUTDOWN_EVENT = 6
    }

    public static void Initialize(Action shutDown)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _handler += Handler;
            _ = SetConsoleCtrlHandler(_handler, true);
            func = shutDown;
        }
    }

    public static bool Handler(CtrlType sig)
    {
        func?.Invoke();

        return true;
    }
}
