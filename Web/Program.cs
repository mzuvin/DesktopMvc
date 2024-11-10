using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Start Desktop App via");
        CreateHostBuilder(args).Build().Run();
    }
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
/*
public class Program
{
    private static CancellationTokenSource? _cts;
    private static int _startingPort = 5000;

    public static string GetUrl => "http://localhost:" + _startingPort;
    public static CancellationTokenSource? GetCancellationTokenSource => _cts;


    public static void SetCancellationTokenSource(CancellationTokenSource cts)
    {
        _cts = cts;
    }
    public static async Task Main(string[] args)
    {
        _cts = new CancellationTokenSource();

        IpcManager.SetCancellationTokenSourceKestrelServer(_cts);
        // Portu dışarıdan al
        if (args.Length > 0 && int.TryParse(args[0], out int parsedPort))
        {
            _startingPort = parsedPort;
        }

        // Kestrel’i başlat
        await RunKestrel(_startingPort);

        AppDomain.CurrentDomain.ProcessExit += async (sender, e) =>
        {
            await IpcManager.SendCommandAsync(Command.StopIpc);
            Console.WriteLine("Uygulama kapanıyor...");
            // Uygulama kapandığında yapılacak işlemleri buraya ekleyebilirsiniz.
        };
    }


    public static void OpenBrowser(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            //MessageBox.Show($"Tarayıcı açılamadı: {ex.Message}");
        }
    }
}*/