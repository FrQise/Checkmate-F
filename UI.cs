using System;

public static class UI
{
    public static void drawHeader()
    {
        Console.CursorVisible = false;
        Console.ForegroundColor = ConsoleColor.Cyan;

        Console.WriteLine(@"
   █████████  █████                        █████                                 █████                        ███████████
  ███▒▒▒▒▒███▒▒███                        ▒▒███                                 ▒▒███                        ▒▒███▒▒▒▒▒▒█
 ███     ▒▒▒  ▒███████    ██████   ██████  ▒███ █████ █████████████    ██████   ███████    ██████             ▒███   █ ▒ 
▒███          ▒███▒▒███  ███▒▒███ ███▒▒███ ▒███▒▒███ ▒▒███▒▒███▒▒███  ▒▒▒▒▒███ ▒▒▒███▒    ███▒▒███ ██████████ ▒███████   
▒███          ▒███ ▒███ ▒███████ ▒███ ▒▒▒  ▒██████▒   ▒███ ▒███ ▒███   ███████   ▒███    ▒███████ ▒▒▒▒▒▒▒▒▒▒  ▒███▒▒▒█   
▒▒███     ███ ▒███ ▒███ ▒███▒▒▒  ▒███  ███ ▒███▒▒███  ▒███ ▒███ ▒███  ███▒▒███   ▒███ ███▒███▒▒▒              ▒███  ▒    
 ▒▒█████████  ████ █████▒▒██████ ▒▒██████  ████ █████ █████▒███ █████▒▒████████  ▒▒█████ ▒▒██████             █████      
  ▒▒▒▒▒▒▒▒▒  ▒▒▒▒ ▒▒▒▒▒  ▒▒▒▒▒▒   ▒▒▒▒▒▒  ▒▒▒▒ ▒▒▒▒▒ ▒▒▒▒▒ ▒▒▒ ▒▒▒▒▒  ▒▒▒▒▒▒▒▒    ▒▒▒▒▒   ▒▒▒▒▒▒             ▒▒▒▒▒       ");

        Console.ResetColor();
        Console.WriteLine(new string('-', 116));
        Console.WriteLine();
    }

    public static void drawSplashScreen()
    {
        Console.Clear();
        drawHeader();
        int width = 116;
        string line1 = "FRQISE'S INTEGRITY AUDITOR\n";
        string line2 = "Version alpha 0.1\n";
        Console.ForegroundColor = ConsoleColor.Cyan;

        Console.WriteLine(line1.PadLeft((width + line1.Length) / 2).PadRight(width));
        Console.WriteLine(line2.PadLeft((width + line2.Length) / 2).PadRight(width));

        Console.ResetColor();
        Console.WriteLine(new string('-', width));
        Console.WriteLine();
    }
}