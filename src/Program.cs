using System;
using System.IO;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace DebloaterPC
{
    class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            try { Console.Title = "Debloater & PC Optimizer Pro"; } catch {}

            try
            {
                // Auto-elevação transparente via UAC caso não esteja executando como Administrador
                bool noElevate = false;
                if (args != null)
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        string a = args[i].ToLowerInvariant();
                        if (a == "--no-elevate" || a == "--status-json" || a == "-s" || a == "--help" || a == "-h")
                        {
                            noElevate = true;
                            break;
                        }
                    }
                }

                if (!Diagnostics.IsAdministrator() && !noElevate)
                {
                    try
                    {
                        var psi = new ProcessStartInfo();
                        psi.FileName = Process.GetCurrentProcess().MainModule.FileName;
                        if (args != null && args.Length > 0)
                        {
                            psi.Arguments = string.Join(" ", args);
                        }
                        psi.Verb = "runas";
                        psi.UseShellExecute = true;
                        Process.Start(psi);
                        return 0; // O processo filho elevado assume a execução
                    }
                    catch (Exception)
                    {
                        // Continua sem elevação se o usuário cancelar o UAC
                    }
                }

                // 1. Process CLI Arguments if provided
                bool isCliCommand = false;
                if (args != null && args.Length > 0)
                {
                    foreach (var arg in args)
                    {
                        string a = arg.ToLowerInvariant();
                        if (a.StartsWith("--standard") || a.StartsWith("--aggressive") || a.StartsWith("--status") || a == "-s" || a.StartsWith("--clean") || a == "--help" || a == "-h")
                        {
                            isCliCommand = true;
                            break;
                        }
                    }
                }

                if (isCliCommand)
                {
                    return RunCli(args);
                }

                // 2. Launch GUI Mode
                return RunGui();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    "Erro ao inicializar o Debloater Master:\n" + ex.Message + "\n\nDetalhes:\n" + ex.StackTrace,
                    "Debloater Master - Erro",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
                return 1;
            }
        }

        private static int RunCli(string[] args)
        {
            string flag = args[0].ToLowerInvariant().Trim();

            if (flag == "--status-json" || flag == "-s")
            {
                int cpu = Diagnostics.GetCpuUsage();
                string model = Diagnostics.GetCpuModel();
                double ramTotal, ramUsed;
                int ramPct;
                Diagnostics.GetRamInfo(out ramTotal, out ramUsed, out ramPct);
                double diskTotal, diskFree;
                int diskPct;
                Diagnostics.GetDiskInfo(out diskTotal, out diskFree, out diskPct);
                string osName, osBuild, uptime;
                Diagnostics.GetOsInfo(out osName, out osBuild, out uptime);

                Console.WriteLine(string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "{{\"cpu\":{0},\"cpuModel\":\"{1}\",\"ramUsedGB\":{2:0.0},\"ramTotalGB\":{3:0.0},\"diskFreeGB\":{4:0.0},\"diskTotalGB\":{5:0.0},\"os\":\"{6}\",\"build\":\"{7}\"}}",
                    cpu, model, ramUsed, ramTotal, diskFree, diskTotal, osName, osBuild));
                return 0;
            }

            if (flag == "--standard")
            {
                Console.WriteLine("Executando Preset Standard em modo silencioso...");
                MemoryManager.ConfigureServices(true, false);
                DiskManager.EnsureTrimEnabled();
                DiskManager.DeepCleanTemps();
                RegistryManager.ApplyPrivacyTweaks(true);
                RegistryManager.SetClassicContextMenu(true);
                RegistryManager.OptimizeExplorerVisuals(false, true, true);
                RegistryManager.FlushDns();
                ChromeOptimizer.ApplyBrowserPolicies(true, true, true);
                ChromeOptimizer.CleanBrowserCaches();
                UwpDebloater.RunDebloat(true, false, false, false);
                RegistryManager.RestartExplorer();
                Console.WriteLine("Preset Standard concluído.");
                return 0;
            }

            if (flag == "--aggressive" || flag == "--pc-fraco")
            {
                Console.WriteLine("Executando Preset Máximo Desempenho (PC Fraco)...");
                MemoryManager.ToggleMemoryCompression(true);
                MemoryManager.ConfigureServices(true, true);
                MemoryManager.SetHighPerformancePlan();
                DiskManager.DisableHibernation(true);
                DiskManager.EnsureTrimEnabled();
                DiskManager.Disable8Dot3Names(true);
                DiskManager.DeepCleanTemps();
                RegistryManager.ApplyPrivacyTweaks(true);
                RegistryManager.SetClassicContextMenu(true);
                RegistryManager.OptimizeExplorerVisuals(true, true, true);
                RegistryManager.FlushDns();
                ChromeOptimizer.ApplyBrowserPolicies(true, true, true);
                ChromeOptimizer.CleanBrowserCaches();
                UwpDebloater.RunDebloat(true, true, true, true);
                RegistryManager.RestartExplorer();
                Console.WriteLine("Preset Máximo Desempenho concluído.");
                return 0;
            }

            if (flag == "--server")
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string uiDir = Path.Combine(baseDir, "ui");
                if (!Directory.Exists(uiDir)) uiDir = Path.Combine(Environment.CurrentDirectory, "ui");

                int port = GetAvailablePort(13570);
                var server = new WebServer(uiDir, port);
                server.Start();
                string appUrl = string.Format("http://127.0.0.1:{0}/index.html", port);
                Console.WriteLine("Servidor Debloater rodando em: " + appUrl);

                // Open browser
                try { Process.Start(appUrl); } catch {}

                Thread.Sleep(Timeout.Infinite);
                return 0;
            }

            if (flag == "--clean-disk")
            {
                Console.WriteLine("Executando limpeza de disco...");
                DiskManager.DeepCleanTemps();
                ChromeOptimizer.CleanBrowserCaches();
                Console.WriteLine("Limpeza concluída.");
                return 0;
            }

            if (flag == "--chrome-opt")
            {
                Console.WriteLine("Aplicando otimizações do Chrome/Edge...");
                ChromeOptimizer.ApplyBrowserPolicies(true, true, true);
                ChromeOptimizer.CleanBrowserCaches();
                Console.WriteLine("Otimização do Chrome/Edge concluída.");
                return 0;
            }

            if (flag == "--rollback")
            {
                Console.WriteLine("Executando Modo Reversão...");
                SafetyManager.ApplyRollback();
                Console.WriteLine("Reversão concluída.");
                return 0;
            }

            if (flag == "--remove-edge")
            {
                Console.WriteLine("Executando remoção do Microsoft Edge...");
                StubbornAppsManager.RemoveMicrosoftEdge();
                Console.WriteLine("Concluído.");
                return 0;
            }

            if (flag == "--remove-onedrive")
            {
                Console.WriteLine("Executando remoção do Microsoft OneDrive...");
                StubbornAppsManager.RemoveOneDrive();
                Console.WriteLine("Concluído.");
                return 0;
            }

            if (flag == "--remove-stubborn")
            {
                Console.WriteLine("Executando remoção completa de aplicativos inamovíveis...");
                StubbornAppsManager.RemoveMicrosoftEdge();
                StubbornAppsManager.RemoveOneDrive();
                StubbornAppsManager.RemoveCopilotDeep();
                StubbornAppsManager.RemoveYourPhone();
                StubbornAppsManager.RemoveXboxOverlays();
                StubbornAppsManager.RemoveQuickAssistAndTips();
                RegistryManager.RestartExplorer();
                Console.WriteLine("Concluído.");
                return 0;
            }

            if (flag == "--services-safe")
            {
                Console.WriteLine("Aplicando Perfil Seguro de Serviços (24 serviços)...");
                ServicesOptimizer.ApplyPreset("safe");
                Console.WriteLine("Concluído.");
                return 0;
            }

            if (flag == "--services-gamer")
            {
                Console.WriteLine("Aplicando Perfil Gamer Máximo Desempenho de Serviços...");
                ServicesOptimizer.ApplyPreset("gamer");
                Console.WriteLine("Concluído.");
                return 0;
            }

            if (flag == "--services-restore")
            {
                Console.WriteLine("Restaurando serviços aos padrões do Windows...");
                ServicesOptimizer.ApplyPreset("restore");
                Console.WriteLine("Concluído.");
                return 0;
            }

            if (flag == "--tasks-disable")
            {
                Console.WriteLine("Desativando as 6 tarefas agendadas de telemetria e CPU...");
                ScheduledTasksManager.ApplyAll(true);
                Console.WriteLine("Concluído.");
                return 0;
            }

            if (flag == "--network-block")
            {
                Console.WriteLine("Bloqueando telemetria em nível de rede (Hosts 0.0.0.0 e Firewall)...");
                NetworkBlockManager.ToggleHostsBlock(true);
                NetworkBlockManager.ToggleFirewallBlock(true);
                Console.WriteLine("Concluído.");
                return 0;
            }

            if (flag == "--gamer-latency")
            {
                Console.WriteLine("Aplicando otimizações gamer de latência e mouse 1:1...");
                GamerOptimizer.ApplyGamerPreset(true);
                Console.WriteLine("Concluído.");
                return 0;
            }

            if (flag == "--w11-recall-off")
            {
                Console.WriteLine("Desativando e bloqueando Windows Recall AI...");
                Windows11ModernManager.SetRecallDisabled(true);
                Console.WriteLine("Concluído.");
                return 0;
            }

            if (flag == "--w11-bypass")
            {
                Console.WriteLine("Aplicando bypass de requisitos de hardware (TPM/CPU/SecureBoot)...");
                Windows11ModernManager.SetLabConfigBypass(true);
                Console.WriteLine("Concluído.");
                return 0;
            }

            if (flag == "--winget-upgrade-all")
            {
                Console.WriteLine("Iniciando atualização de todos os programas via WinGet...");
                WinGetInstaller.UpgradeAll();
                Console.WriteLine("Concluído.");
                return 0;
            }

            if (flag == "--resurrect-store")
            {
                Console.WriteLine("Iniciando recuperação de emergência da Microsoft Store...");
                UwpDebloater.ResurrectWindowsStore();
                Console.WriteLine("Concluído.");
                return 0;
            }

            Console.WriteLine("Uso: DebloaterPC.exe [opções]");
            Console.WriteLine("  --standard            Aplica preset recomendado silenciosamente");
            Console.WriteLine("  --aggressive          Aplica preset de máximo desempenho (PC Fraco)");
            Console.WriteLine("  --clean-disk          Limpeza de arquivos temporários e caches");
            Console.WriteLine("  --chrome-opt          Aplica políticas de economia de RAM no Chrome/Edge");
            Console.WriteLine("  --services-safe       Aplica perfil equilibrado de serviços");
            Console.WriteLine("  --services-gamer      Desativa os 24 serviços para máximo FPS");
            Console.WriteLine("  --services-restore    Restaura padrões de fábrica de todos os serviços");
            Console.WriteLine("  --tasks-disable       Desativa as 6 tarefas agendadas de telemetria");
            Console.WriteLine("  --network-block       Null-routing em Hosts e Firewall para telemetria");
            Console.WriteLine("  --gamer-latency       Desativa Network Throttling e aceleração de mouse");
            Console.WriteLine("  --w11-recall-off      Desativa Windows Recall AI");
            Console.WriteLine("  --w11-bypass          Aplica bypass de hardware (TPM/CPU/SecureBoot)");
            Console.WriteLine("  --winget-upgrade-all  Atualiza todos os programas do PC");
            Console.WriteLine("  --resurrect-store     Restaura e reinstala a Microsoft Store");
            Console.WriteLine("  --rollback            Reverte configurações aos padrões do Windows");
            Console.WriteLine("  --status-json         Exibe diagnóstico de hardware em JSON");
            return 0;
        }

        private static int RunGui()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string uiDir = Path.Combine(baseDir, "ui");

            if (!Directory.Exists(uiDir))
            {
                string altUi = Path.Combine(Environment.CurrentDirectory, "ui");
                if (Directory.Exists(altUi)) uiDir = altUi;
                else
                {
                    System.Windows.Forms.MessageBox.Show(
                        "Diretório 'ui' não encontrado em: " + uiDir + "\nCertifique-se de manter a pasta 'ui' junto ao executável.",
                        "Debloater Master - Erro",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Error);
                    return 1;
                }
            }

            int port = GetAvailablePort(13580);
            var server = new WebServer(uiDir, port);
            server.Start();

            string appUrl = string.Format("http://127.0.0.1:{0}/index.html", port);

            // Tenta abrir em janela isolada de aplicativo (WebView2 / Edge App Mode)
            string edgePath = FindEdgePath();
            bool opened = false;

            if (!string.IsNullOrEmpty(edgePath) && File.Exists(edgePath))
            {
                try
                {
                    string tempProfile = Path.Combine(Path.GetTempPath(), "DebloaterMaster_Profile");
                    string args = string.Format(
                        "--app=\"{0}\" --window-size=1120,760 --user-data-dir=\"{1}\" --no-first-run",
                        appUrl, tempProfile);

                    var psi = new ProcessStartInfo(edgePath, args)
                    {
                        UseShellExecute = false
                    };
                    Process.Start(psi);
                    opened = true;
                }
                catch {}
            }

            // Fallback caso Edge não inicie em app mode
            if (!opened)
            {
                try
                {
                    Process.Start(new ProcessStartInfo(appUrl) { UseShellExecute = true });
                    opened = true;
                }
                catch
                {
                    try
                    {
                        Process.Start("cmd.exe", "/c start " + appUrl);
                        opened = true;
                    }
                    catch {}
                }
            }

            // Mantém o servidor ativo enquanto o navegador estiver conectado
            // 1. Aguarda a primeira requisição do navegador (tolerância inicial de 40s)
            DateTime launchTime = DateTime.UtcNow;
            while (!WebServer.HasReceivedRequests && (DateTime.UtcNow - launchTime).TotalSeconds < 40)
            {
                Thread.Sleep(500);
            }

            // 2. Mantém rodando continuamente. Encerra apenas se a janela for fechada
            // e nenhum heartbeat/requisição for recebido por mais de 20 segundos
            while (true)
            {
                Thread.Sleep(2000);
                if (WebServer.HasReceivedRequests && (DateTime.UtcNow - WebServer.LastActivity).TotalSeconds > 20)
                {
                    break;
                }
            }

            server.Stop();
            return 0;
        }

        private static string FindEdgePath()
        {
            string[] possiblePaths = new string[]
            {
                @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
                @"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
                @"C:\Program Files (x86)\Microsoft\EdgeWebView\Application\msedgewebview2.exe"
            };

            foreach (var p in possiblePaths)
            {
                if (File.Exists(p)) return p;
            }

            // Search in EdgeWebView wildcard directory
            try
            {
                string webviewRoot = @"C:\Program Files (x86)\Microsoft\EdgeWebView\Application";
                if (Directory.Exists(webviewRoot))
                {
                    var found = Directory.GetFiles(webviewRoot, "msedgewebview2.exe", SearchOption.AllDirectories);
                    if (found.Length > 0) return found[0];
                }
            }
            catch {}

            return null;
        }

        private static int GetAvailablePort(int defaultPort)
        {
            for (int p = defaultPort; p < defaultPort + 100; p++)
            {
                try
                {
                    var listener = new TcpListener(IPAddress.Loopback, p);
                    listener.Start();
                    listener.Stop();
                    return p;
                }
                catch {}
            }
            return defaultPort;
        }
    }
}
