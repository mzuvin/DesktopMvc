namespace Desktop;

public class LogHelper
{
    private string logDirectory;
    private string logFileName;
    private long maxSizeBytes = 1 * 1024 * 1024; // 1 MB
    private int fileIndex = 0;

    public LogHelper()
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string dateFolder = DateTime.Now.ToString("yyyy-MM-dd");
        logDirectory = Path.Combine(baseDirectory, "logs", dateFolder);

        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        logFileName = GetLogFileName();
    }

    private string GetLogFileName()
    {
        return Path.Combine(logDirectory, $"log_{fileIndex}.txt");
    }

    private void CheckLogFileSize()
    {
        FileInfo fileInfo = new FileInfo(logFileName);

        if (fileInfo.Exists && fileInfo.Length >= maxSizeBytes)
        {
            fileIndex++;
            logFileName = GetLogFileName();
        }
    }

    public void Log(string message)
    {
        CheckLogFileSize();

        using (StreamWriter writer = new StreamWriter(logFileName, true))
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
            writer.WriteLine(logEntry);
        }
    }
}
