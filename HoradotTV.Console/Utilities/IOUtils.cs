namespace HoradotTV.Console.Utilities;

internal static class IOUtils
{
    public static void Print(string s) => System.Console.WriteLine(s);
    public static void Log(string s) => Print($"[{DateTime.Now:T}] {s}");

    public static string Input(string s = "")
    {
        System.Console.Write(s);
        try
        {
            string? result = System.Console.ReadLine();
            return result ?? throw new Exception();
        }
        catch
        {
            Environment.Exit(0);
        }

        return "";
    }

    private static int InputInt(string s = "")
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
            }
        }
    }

    public static int InputPositiveInt(string s = "")
    {
        int amount = InputInt(s);
        while (amount < 0)
        {
            Print("Please enter a positive amount.");
            amount = InputInt(s);
        }

        return amount;
    }

    public static string ChooseOption(IEnumerable<string> options, string type = "option",
        string title = "Choose an option")
    {
        var optionsList = options.ToList();
        do
        {
            Print($"\nAvailable {type}s: " + string.Join(", ", optionsList));
            string option = Input($"{title} ({Constants.Commands.Cancel} - cancel): ");
            if (option == Constants.Commands.Cancel || optionsList.Contains(option))
            {
                return option;
            }

            Print($"Please enter an valid {type}.");
        } while (true);
    }

    public static int ChooseOptionIndex(IEnumerable<string> options, string title = "Choose an option")
    {
        var optionsList = options.ToList();
        Print("[0] Back to start");
        for (int i = 0; i < optionsList.Count; i++)
        {
            Print($"[{i + 1}] {optionsList[i]}");
        }

        return ChooseOptionRange(optionsList.Count, title);
    }

    public static int ChooseOptionRange(int max, string title = "Choose an option") => ChooseOptionRange(0, max, title);

    private static int ChooseOptionRange(int min, int max, string title = "Choose an number")
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
