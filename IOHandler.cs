using System.Runtime.InteropServices;

public static class IOHandler
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    private class WindowWrapper : IWin32Window
    {
        public IntPtr Handle { get; }
        public WindowWrapper(IntPtr handle) { Handle = handle; }
    }

    public static string? filePicker()
    {
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string auditFolder = Path.Combine(documentsPath, "Checkmate-F Audits");

        using OpenFileDialog ofd = new OpenFileDialog();
        ofd.Title = "Select a file";
        ofd.Filter = "Audit files (*.audit)|*.audit|All files (*.*)|*.*";
        ofd.InitialDirectory = auditFolder;
        ofd.CheckFileExists = true;
        IntPtr consoleHandle = GetForegroundWindow();
        WindowWrapper owner = new WindowWrapper(consoleHandle);

        if (ofd.ShowDialog(owner) == DialogResult.OK)
        {
            return ofd.FileName;
        }

        return null;
    }

    public static string? folderPicker()
    {
        using FolderBrowserDialog fbd = new FolderBrowserDialog();
        fbd.Description = "Select a folder";
        fbd.ShowNewFolderButton = false;
        IntPtr consoleHandle = GetForegroundWindow();
        WindowWrapper owner = new WindowWrapper(consoleHandle);

        if (fbd.ShowDialog(owner) == DialogResult.OK)
        {
            return fbd.SelectedPath;
        }

        return null;
    }
}