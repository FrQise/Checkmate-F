class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        UI.drawSplashScreen();
        MenuSystem.Pause();
        bool running = true;
        while (running)
        {
            string[] mainOptions = { "Create a snapshot", "Run an audit", "View previous audits", "Inspect a snapshot", "Exit" };
            int choice = MenuSystem.drawMenu("", mainOptions, excludeReturn: true);

            switch (choice)
            {
                case 0: Operations.runCreateSnapshot(); break;
                case 1: Operations.runPerformAudit(); break;
                case 2: Operations.runViewHistory(); break;
                case 3: Operations.runInspectSnapshot(); break;
                case 4: running = false; Console.CursorVisible = true; break;
            }
        }
    }
}