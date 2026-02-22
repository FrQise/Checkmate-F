public static class Operations
{
    public static void runCreateSnapshot()
    {
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string auditRoot = Path.Combine(documentsPath, "Checkmate-F Audits");
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");

        MenuSystem.clearUI();

        int choice = MenuSystem.drawMenu("Select a directory to snapshot:", new string[] { "Browse for folder" });
        string? directoryPath = (choice == 0) ? IOHandler.folderPicker() : null;

        if (string.IsNullOrEmpty(directoryPath))
        {
            Console.WriteLine("Operation cancelled.");
            return;
        }

        MenuSystem.clearUI();
        Console.Write("Enter name for this snapshot: ");
        Console.CursorVisible = true;
        string snapshotName = Console.ReadLine() ?? "snapshot";
        Console.WriteLine("");

        List<fileIdentity> files = CheckmateEngine.createSnapshot(directoryPath, MenuSystem.progressBar);

        Console.Write("\n\nEnter a brief description for this snapshot: ");
        string description = Console.ReadLine() ?? "No description provided";
        Console.CursorVisible = false;

        long totalSize = 0;
        foreach (fileIdentity file in files) { totalSize += file.Size; }

        SnapshotHeader header = new SnapshotHeader(directoryPath, description, files.Count, totalSize, DateTime.Now);
        SnapshotPackage package = new SnapshotPackage(header, files);

        string sessionPath = Path.Combine(auditRoot, $"{snapshotName}_{timestamp}");
        Directory.CreateDirectory(sessionPath);

        string finalFilePath = Path.Combine(sessionPath, snapshotName);
        CheckmateEngine.saveSnapshot(package, finalFilePath);

        Console.WriteLine($"\nSnapshot saved: {finalFilePath}\n");
        MenuSystem.Pause();
    }

    public static void runPerformAudit()
    {
        MenuSystem.clearUI();

        int choice = MenuSystem.drawMenu("Select Audit intensity", new string[] { "Fast Check (Size & Date only)", "Deep verification (Full hash comparison)", "Return"}, excludeReturn: true);
        if (choice == 2) return;
        bool deepVerify = (choice == 1);

        choice = MenuSystem.drawMenu("Select a snapshot file to audit against:", new string[] { "Browse for snapshot" });
        string? snapshotPath = (choice == 0) ? IOHandler.filePicker() : null;

        if (string.IsNullOrEmpty(snapshotPath)) { Console.WriteLine("No audit file selected."); return; }

        SnapshotPackage package = CheckmateEngine.loadSnapshot(snapshotPath);
        string directoryPath = package.Header.SourcePath;

        Console.WriteLine($"Snapshot originally created from: {directoryPath}");
        int pathChoice = MenuSystem.drawMenu("Use this original path for the audit?", new string[] { "Yes, proceed", "No, browse for a different folder" });

        if (pathChoice == 1)
        {
            string? newPath = IOHandler.folderPicker();

            if (!string.IsNullOrEmpty(newPath))
            {
                directoryPath = newPath;
            }
        }

        MenuSystem.clearUI();
        Console.WriteLine("--- Audit Preparation ---");
        Console.WriteLine($"Description:   {package.Header.Description}");
        Console.WriteLine($"Target Path:   {directoryPath}");
        Console.WriteLine($"Snapshot Date: {package.Header.CreatedAt:yyyy-MM-dd HH:mm}");
        Console.WriteLine("--------------------------");
        MenuSystem.Pause();

        MenuSystem.clearUI();
        Console.WriteLine(deepVerify ? "--- Mode: Deep Audit ---\n" : "--- Mode: Fast Audit ---\n");
        Console.WriteLine("Scanning live directory and comparing hashes...\n");
        List<fileIdentity> currentFiles = CheckmateEngine.createSnapshot(directoryPath, MenuSystem.progressBar, !deepVerify);

        AuditReport report = CheckmateEngine.auditSnapshot(package.Files, currentFiles, deepVerify);
        List<string> reportLines = Report.formatReport(report);

        MenuSystem.clearUI();
        Report.printFormattedReport(reportLines);

        string? sessionFolder = Path.GetDirectoryName(snapshotPath);
        if (sessionFolder != null)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
            string logName = $"AuditResult_{timestamp}.log";
            string logPath = Path.Combine(sessionFolder, logName);
            Report.saveAuditLog(reportLines, logPath);
        }

        Console.WriteLine("\nAudit process finished.");
        MenuSystem.Pause();

        if (report.HasChanges)
        {
            choice = MenuSystem.drawMenu("Discrepancies detected. Open directory in explorer?", new string[] { "View directory", "Exit" }, excludeReturn: true);
            if (choice == 0)
            {
                System.Diagnostics.Process.Start("explorer.exe", directoryPath);
            }
        }
    }

    public static void runViewHistory()
    {
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string auditRoot = Path.Combine(documentsPath, "Checkmate-F Audits");

        if (!Directory.Exists(auditRoot))
        {
            Directory.CreateDirectory(auditRoot);
        }

        string[] logFiles = Directory.GetFiles(auditRoot, "*.log", SearchOption.AllDirectories);

        if (logFiles.Length == 0)
        {
            MenuSystem.clearUI();
            Console.WriteLine("No previous audit logs found.");
            MenuSystem.Pause();
            return;
        }

        string[] menuLabels = logFiles.Select(f =>
        {
            string? folder = Path.GetDirectoryName(f);
            string folderName = folder != null ? Path.GetFileName(folder) : "Root";
            return $"[{folderName}] -> {Path.GetFileName(f)}";
        }).ToArray();

        MenuSystem.clearUI();
        int choice = MenuSystem.drawMenu("Select an audit log to view:", menuLabels);

        MenuSystem.clearUI();
        string selectedLogPath = logFiles[choice];
        List<string> historyLines = File.ReadAllLines(selectedLogPath).ToList();

        Console.WriteLine($"--- Viewing History: {Path.GetFileName(selectedLogPath)} ---");
        Console.WriteLine($"Location: {selectedLogPath}\n");

        Report.printFormattedReport(historyLines);

        Console.WriteLine("\n--- End of Log ---");
        MenuSystem.Pause();
    }

    public static void runInspectSnapshot()
    {
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string auditRoot = Path.Combine(documentsPath, "Checkmate-F Audits");

        string[] auditFiles = Directory.GetFiles(auditRoot, "*.audit", SearchOption.AllDirectories);

        if (auditFiles.Length == 0)
        {
            MenuSystem.clearUI();
            Console.WriteLine("No snapshots found to inspect.");
            MenuSystem.Pause();
            return;
        }

        string[] labels = auditFiles.Select(f => Path.GetFileName(f)).ToArray();
        int choice = MenuSystem.drawMenu("Select a snapshot to inspect:", labels);

        SnapshotPackage package = CheckmateEngine.loadSnapshot(auditFiles[choice]);
        SnapshotHeader header = package.Header;

        MenuSystem.clearUI();

        Console.WriteLine($" NAME:        {Path.GetFileName(auditFiles[choice])}");
        Console.WriteLine($" CREATED:     {header.CreatedAt:f}");
        Console.WriteLine($" SOURCE:      {header.SourcePath}");
        Console.WriteLine($" FILES:       {header.FileCount:N0}");
        Console.WriteLine($" TOTAL SIZE:  {header.TotalSize / 1024.0 / 1024.0:F2} MB");
        Console.WriteLine("--------------------------------------------------");
        Console.WriteLine($" DESCRIPTION: ");
        Console.WriteLine($" {header.Description}");

        MenuSystem.Pause();
    }

}