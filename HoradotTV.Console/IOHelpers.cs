namespace HoradotTV.Console;

internal static class IOHelpers
{
    public static void Print(string s) => System.Console.WriteLine(s);
    public static string Input(string s = "")
    {
        System.Console.Write(s);
        return System.Console.ReadLine()!;
    }
    public static int InputInt(string s = "")
    {
        while (true)
        {
            try
            {
                return int.Parse(Input(s));
            }
            catch (FormatException)
            {
                Print("Please enter a number.");
                continue;
            }
        }
    }

    public static string ChooseOption(IEnumerable<string> options, string type = "option", string title = "Choose an option")
    {
        string option;
        do
        {
            Print($"\nAvailable {type}s: " + string.Join(", ", options));
            option = Input($"{title}: ");
            if (options.Contains(option))
                return option;
            Print($"Please enter an valid {type}.");
        } while (true);
    }
    public static int ChooseOptionRange(int max, string title = "Choose a number") => ChooseOptionRange(0, max, title);
    public static int ChooseOptionRange(int min, int max, string title = "Choose a number")
    {
        int option;
        do
        {
            option = InputInt($"{title} ({min}-{max}): ");

            if (option > max || option < min)
            {
                Print("Please choose a number within the range.");
            }
        } while (option > max || option < min);

        return option;
    }
}
