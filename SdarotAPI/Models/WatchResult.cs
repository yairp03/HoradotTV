namespace SdarotAPI.Models;

public class WatchResult
{
    public string VID { get; set; } = string.Empty;
    public Dictionary<int, string> Watch { get; set; } = new();
}
