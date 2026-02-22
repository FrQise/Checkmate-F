public static class MenuSystem
{
    public static int drawMenu(string msg, string[] options, bool excludeReturn = false)
    {
        int selectedIndex = 0;
        ConsoleKey key;
        List<string> optionsList = options.ToList();

        if (!excludeReturn) optionsList.Add("Return");

        Console.Clear();

        do
        {
            Console.SetCursorPosition(0, 0);
            UI.drawHeader();
            Console.WriteLine($"{msg}\n");
            for (int i = 0; i < optionsList.Count; i++)
            {
                if (i == selectedIndex)
                {
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.WriteLine($"> {optionsList[i]}");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"  {optionsList[i]}");
                }
            }

            key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.UpArrow)
                selectedIndex = Math.Max(0, selectedIndex - 1);
            else if (key == ConsoleKey.DownArrow)
                selectedIndex = Math.Min(optionsList.Count - 1, selectedIndex + 1);

        } while (key != ConsoleKey.Enter);

        if (!excludeReturn && selectedIndex == optionsList.Count - 1) return -1; // To handle return option

        return selectedIndex;
    }

    public static void Pause()
    {
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);
    }

    public static void progressBar(double percentage)
    {
        int totalBlocks = 50;
        int filledBlocks = Math.Clamp((int)(percentage / 100 * totalBlocks), 0, totalBlocks);
        string bar = "[" + new string('#', filledBlocks) + new string('-', totalBlocks - filledBlocks) + $"] {percentage:F2}%";
        Console.Write($"\r{bar}");
    }

    public static void clearUI()
    {
        Console.Clear();
        UI.drawHeader();
    }
}