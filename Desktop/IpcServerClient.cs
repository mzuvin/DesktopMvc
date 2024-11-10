using System.Net;

namespace Web.Helper;

public enum Command
{
    Stop,
    Restart,
    StopIpc
}

public static class IpcManager
{
    private static HttpListener _listener;
    private static CancellationTokenSource? _ipcCts;
    private static CancellationTokenSource? _cts;
    private static int _port = 5001; 
    private static readonly HttpClient client = new HttpClient();
    private static string BaseUrl => $"http://localhost:{_port}/control/";

    public static void SetCancellationTokenSourceKestrelServer(CancellationTokenSource cts)
    {
        _cts = cts;
    }

    public static CancellationTokenSource GetTokenKestrel => _cts;
    
    public static void StartServer(int port)
    {
        _port = port;
        _ipcCts = new CancellationTokenSource();
        Task.Run(()=>RunServer(_port));
    }

    public static async Task SendCommandAsync(Command command)
    {
        try
        {
            var response = await client.GetAsync(BaseUrl + command.ToString().ToLower());
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"{command.ToString().ToUpper()} komutu başarıyla gönderildi.");
            }
            else
            {
                Console.WriteLine(
                    $"{command.ToString().ToUpper()} komutu gönderilirken hata oluştu. Sunucu yanıt kodu: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Bir hata oluştu: {ex.Message}");
        }
    }

    private static void RunServer(int port)
    {
        while (!_ipcCts.Token.IsCancellationRequested)
        {
            try
            {
                if (_listener == null)
                {
                    _listener = new HttpListener();
                    _listener.Prefixes.Add(BaseUrl); 
                    _listener.Start();
                    Console.WriteLine($"IPC Sunucusu başlatıldı. Dinleniyor: {BaseUrl}");
                }
                Console.WriteLine($"IPC Sunucusu başlatıldı. Dinleniyor: {BaseUrl}");
                var context = _listener.GetContext();
                var command = context.Request.Url?.AbsolutePath.ToLower().Replace("/control/", "");

                if (Enum.TryParse(command, true, out Command parsedCommand))
                {
                    switch (parsedCommand)
                    {
                        case Command.StopIpc:
                            Console.WriteLine($"IPC Sunucusu kapatıldı. {BaseUrl}");
                            _ipcCts?.Cancel();
                            break;
                        case Command.Stop:
                            Console.WriteLine("Kestrel durduruluyor...");
                            _cts?.Cancel();
                            context.Response.StatusCode = 200;
                            context.Response.Close();
                            break;
                        case Command.Restart:
                            Console.WriteLine("Kestrel yeniden başlatılıyor...");
                            _cts?.Cancel();
                            _cts = new CancellationTokenSource();
                            context.Response.StatusCode = 200;
                            context.Response.Close();
                            break;
                        default:
                            context.Response.StatusCode = 404;
                            context.Response.Close();
                            break;
                    }
                }
                else
                {
                    context.Response.StatusCode = 404;
                    context.Response.Close();
                }
            }
            catch (Exception ex) when (ex is System.IO.IOException || ex is System.Net.Sockets.SocketException)
            {
                Console.WriteLine($"Port {port} kullanımda. {port + 1} numaralı porta geçiliyor...");
                port++; 
                _port = port; 
            }
        }
    }
}