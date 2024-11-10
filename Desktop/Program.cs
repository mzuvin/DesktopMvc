using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using PureManApplicationDeployment;
using Web;
using Web.Helper;

namespace Desktop;

static class Program
{
    static NotifyIcon trayIcon;
    public static int _startingPort = 3434;
    public static string GetUrl => "http://localhost:" + _startingPort;

    [STAThread]
    static Task Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Form1 form1 = new Form1();
        form1.FormBorderStyle = FormBorderStyle.FixedSingle;
        IpcManager.StartServer(3838);
        Task.Run(() => RunKestrel(3834, () =>
        {
            form1.Hide();
        }));
        trayIcon = new NotifyIcon
        {
            Icon = new Icon(SystemIcons.GetStockIcon(StockIconId.Server), 40, 40),
            Visible = true,
            Text = "MVC Uygulaması v1.22"
        };
        trayIcon.ContextMenuStrip = new ContextMenuStrip();
        trayIcon.ContextMenuStrip.Items.Add("Uygulamayı Aç", null, (sender, args) => OpenBrowser(GetUrl));
        trayIcon.ContextMenuStrip.Items.Add("Yenile", null,
            async (sender, args) => { await IpcManager.SendCommandAsync(Command.Restart); });
        trayIcon.ContextMenuStrip.Items.Add("Güncelle", null,
            async (sender, args) =>
            {
                var clickOnce = new PureManClickOnce("https://masaustu.pages.dev/");
                var isUpdateAvailable = await clickOnce.CheckUpdateAvailableAsync();
                var serverVersion = await clickOnce.RefreshServerVersion();
                if (isUpdateAvailable)
                {
                    var result = MessageBox.Show("Yeni bir güncelleme var! " + serverVersion.ToString(),
                        "Güncellensin mi?", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        await clickOnce.UpdateAsync();
                        Application.Exit();
                    }
                }
                else
                {
                    MessageBox.Show("Uygulamanız güncel.");
                }
            });
        trayIcon.ContextMenuStrip.Items.Add("Çıkış", null, async (sender, args) =>
        {
            await IpcManager.SendCommandAsync(Command.Stop);
            trayIcon.Visible = false;
            Application.Exit();
        });
        Application.Run(form1);
        return Task.CompletedTask;
    }

    public static async Task RunKestrel(int port,Action action)
    {
        while (true)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                IHostBuilder hostBuilder = Host.CreateDefaultBuilder()
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseKestrel(options => options.ListenAnyIP(port));
                        webBuilder.UseStartup<Startup>();
                    });

                _startingPort = port;
                var host = hostBuilder.Build();
                action();
                OpenBrowser(GetUrl);
                IpcManager.SetCancellationTokenSourceKestrelServer(new CancellationTokenSource());
                await host.RunAsync(IpcManager.GetTokenKestrel.Token);
            }
            catch (OperationCanceledException)
            {
                new LogHelper().Log("Kestrel sunucusu iptal edildi. Yeniden başlatılıyor...");
                Console.WriteLine("Kestrel sunucusu iptal edildi. Yeniden başlatılıyor...");
                continue; 
            }

            catch (Exception ex) when (ex is System.IO.IOException || ex is System.Net.Sockets.SocketException)
            {
                new LogHelper().Log(ex.Message + " " + ex.StackTrace);
                Console.WriteLine($"Port {port} is in use. Trying port {port + 1}...");
                port++;
            }
            catch (Exception e)
            {
                new LogHelper().Log(e.Message + " " + e.StackTrace);
                Console.WriteLine(e);
                continue;
            }
        }
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
            MessageBox.Show($"Tarayıcı açılamadı: {ex.Message}");
        }
    }
}