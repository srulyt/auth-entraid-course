namespace Labs.Cli.Helpers;

public static class ConsoleOutput
{
    public static void WriteSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓ {message}");
        Console.ResetColor();
    }

    public static void WriteError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"✗ {message}");
        Console.ResetColor();
    }

    public static void WriteWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"⚠ {message}");
        Console.ResetColor();
    }

    public static void WriteInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"ℹ {message}");
        Console.ResetColor();
    }

    public static void WriteHeader(string message)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine(message);
        Console.WriteLine("=".PadRight(80, '='));
        Console.ResetColor();
        Console.WriteLine();
    }

    public static void WriteSubHeader(string message)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(message);
        Console.WriteLine("-".PadRight(message.Length, '-'));
        Console.ResetColor();
    }

    public static void WriteKeyValue(string key, string value, int keyWidth = 20)
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write($"{key.PadRight(keyWidth)}: ");
        Console.ResetColor();
        Console.WriteLine(value);
    }

    public static void WriteDim(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}
