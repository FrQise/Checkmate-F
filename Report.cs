public static class Report
{

    private static readonly Dictionary<string, ConsoleColor> theme = new Dictionary<string, ConsoleColor>
    {
        { "[MODIFIED]", ConsoleColor.DarkYellow },
        { "[DELETED]",  ConsoleColor.Red },
        { "[RENAMED]",  ConsoleColor.Magenta },
        { "[ADDED]",    ConsoleColor.Blue },
        { "VERDICT:",   ConsoleColor.White }
    };

    public static List<string> formatReport(AuditReport report)
    {
        List<string> lines = new List<string>();
        lines.Add($"AUDIT SESSION: {DateTime.Now}");
        lines.Add(new string('-', 50));

        foreach (string file in report.Modified) { lines.Add($"[MODIFIED] {file}"); }
        foreach (string file in report.Deleted) { lines.Add($"[DELETED]  {file}"); }
        foreach (string file in report.Renamed) { lines.Add($"[RENAMED]  {file}"); }
        foreach (string file in report.Added) { lines.Add($"[ADDED]    {file}"); }

        lines.Add("\n" + new string('-', 50));
        lines.Add("AUDIT SUMMARY");
        lines.Add(new string('-', 50));
        lines.Add($"  {"MODIFIED:",-12} {report.Modified.Count}");
        lines.Add($"  {"DELETED:",-12} {report.Deleted.Count}");
        lines.Add($"  {"RENAMED:",-12} {report.Renamed.Count}");
        lines.Add($"  {"ADDED:",-12} {report.Added.Count}");
        lines.Add(new string('-', 50));

        string sizeDisplay = report.totalChangedSizeMB >= 0.1 ? $"{report.totalChangedSizeMB:F2} MB" : $"{report.totalChangedSize / 1024.0:F2} KB";
        lines.Add($"  {"TOTAL SIZE:",-12} {sizeDisplay}");

        string verdict = report.HasChanges ? "VERDICT: Discrepancies detected." : "VERDICT: Directory is unchanged.";
        lines.Add(verdict);
        lines.Add(new string('-', 50));

        return lines;
    }

    public static void printFormattedReport(List<string> lines)
    {

        foreach (string line in lines)
        {
            string? key = theme.Keys.FirstOrDefault(k => line.StartsWith(k));

            if (key != null)
            {
                Console.ForegroundColor = theme[key];

                if (key == "VERDICT:" && line.Contains("unchanged"))
                    Console.ForegroundColor = ConsoleColor.Green;
                else if (key == "VERDICT:")
                    Console.ForegroundColor = ConsoleColor.Red;
            }
            else
            {
                Console.ResetColor();
            }

            Console.WriteLine(line);
        }
        Console.ResetColor();
    }
    public static void saveAuditLog(List<string> lines, string fullPath)
    {
        try
        {
            File.WriteAllLines(fullPath, lines);
            Console.WriteLine($"\nLog saved: {Path.GetFileName(fullPath)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError saving log: {ex.Message}");
        }
    }
}