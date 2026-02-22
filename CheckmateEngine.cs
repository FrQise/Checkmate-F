using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;

public static class CheckmateEngine
{
    public static string computeHash(string filepath)
    {
        if (!File.Exists(filepath))
            throw new FileNotFoundException($"File not found: {filepath}");

        using FileStream stream = File.OpenRead(filepath);
        using SHA256 sha256Hash = SHA256.Create();
        byte[] rawHash = sha256Hash.ComputeHash(stream);
        StringBuilder builder = new StringBuilder();

        foreach (byte b in rawHash)
        {
            builder.Append(b.ToString("x2"));
        }
        return builder.ToString();
    }

    public static List<fileIdentity> createSnapshot(string directoryPath, Action<double>? progressCallback = null, bool skipHash = false)
    {
        ConcurrentBag<fileIdentity> snapshotBag = new ConcurrentBag<fileIdentity>();
        string[] files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
        int processedCount = 0;
        int totalFiles = files.Length;

        Parallel.ForEach(files, (string file) =>
        {
            try
            {
                FileInfo info = new FileInfo(file);
                string relativePath = Path.GetRelativePath(directoryPath, file);

                string hash = skipHash ? "FASH_CHECK" : computeHash(file);

                fileIdentity identity = new fileIdentity(
                    relativePath,
                    hash,
                    info.Length,
                    info.LastWriteTime
                );

                snapshotBag.Add(identity);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[Error] {Path.GetFileName(file)}: {ex.Message}");
            }
            finally
            {
                int currentProcessed = Interlocked.Increment(ref processedCount);
                progressCallback?.Invoke((double)currentProcessed / totalFiles * 100);
            }
        });

        return snapshotBag.ToList();
    }

    public static void saveSnapshot(SnapshotPackage package, string filename)
    {
        string fullPath = filename.EndsWith(".audit") ? filename : $"{filename}.audit";

        if (File.Exists(fullPath))
        {
            throw new IOException($"Conflict: '{fullPath}' already exists.");
        }

        JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(package, options);
        File.WriteAllText(fullPath, json);
    }

    public static SnapshotPackage loadSnapshot(string filepath)
    {
        if (!File.Exists(filepath))
            throw new FileNotFoundException($"File not found: {filepath}");

        string json = File.ReadAllText(filepath);
        return JsonSerializer.Deserialize<SnapshotPackage>(json) ?? throw new InvalidDataException("Failed to deserialize snapshot.");
    }

    public static AuditReport auditSnapshot(List<fileIdentity> baseline, List<fileIdentity> current, bool deepVerify = true)
    {
        AuditReport report = new AuditReport();

        Dictionary<string, fileIdentity> currentLookup = current.ToDictionary(c => c.RelativePath);
        HashSet<string> baselinePaths = baseline.Select(b => b.RelativePath).ToHashSet();
        HashSet<string> currentPaths = current.Select(c => c.RelativePath).ToHashSet();

        foreach (fileIdentity baseFile in baseline)
        {
            if (!currentPaths.Contains(baseFile.RelativePath))
            {
                fileIdentity? renamedFile = current.FirstOrDefault(c =>
                    c.Hash == baseFile.Hash && !baselinePaths.Contains(c.RelativePath));

                if (renamedFile != null)
                {
                    report.Renamed.Add($"{baseFile.RelativePath} -> {renamedFile.RelativePath}");
                }
                else
                {
                    report.Deleted.Add(baseFile.RelativePath);
                    report.totalChangedSize += baseFile.Size;
                }
            }
            else
            {
                fileIdentity currentFile = currentLookup[baseFile.RelativePath];

                bool isModified = deepVerify ? (currentFile.Hash != baseFile.Hash) : (currentFile.Size != baseFile.Size || currentFile.LastModified != baseFile.LastModified);

                if (isModified)
                {
                    report.Modified.Add(baseFile.RelativePath);
                    report.totalChangedSize += currentFile.Size;
                }
            }
        }

        foreach (fileIdentity file in current.Where(c => !baselinePaths.Contains(c.RelativePath)))
        {
            bool isRenameTarget = report.Renamed.Any(r => r.EndsWith($" -> {file.RelativePath}"));
            if (!isRenameTarget)
            {
                report.Added.Add(file.RelativePath);
                report.totalChangedSize += file.Size;
            }
        }

        return report;
    }
}

public record class fileIdentity(string RelativePath, string Hash, long Size, DateTime LastModified);

public class AuditReport
{
    public List<string> Added { get; } = new List<string>();
    public List<string> Deleted { get; } = new List<string>();
    public List<string> Modified { get; } = new List<string>();
    public List<string> Renamed { get; } = new List<string>();
    public long Size { get; set; }
    public long totalChangedSize { get; set; }
    public double totalChangedSizeMB => totalChangedSize / 1024.0 / 1024.0;

    public bool HasChanges => Added.Count + Deleted.Count + Modified.Count + Renamed.Count > 0;
}

public record class SnapshotHeader(
    string SourcePath,
    string Description,
    int FileCount,
    long TotalSize,
    DateTime CreatedAt
);

public class SnapshotPackage
{
    public SnapshotHeader Header { get; set; }
    public List<fileIdentity> Files { get; set; }

    public SnapshotPackage(SnapshotHeader header, List<fileIdentity> files)
    {
        Header = header;
        Files = files;
    }
}