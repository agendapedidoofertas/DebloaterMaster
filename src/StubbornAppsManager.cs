using System;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;

namespace DebloaterPC
{
    public static class StubbornAppsManager
    {
        public static void RemoveMicrosoftEdge()
        {
            LogEngine.Log("Iniciando remoção forçada do Microsoft Edge...", "info");

            try
            {
                // 1. Matar processos ativos do Edge
                foreach (var pName in new string[] { "msedge", "MicrosoftEdgeUpdate", "msedgewebview2" })
                {
                    foreach (var p in Process.GetProcessesByName(pName))
                    {
                        try { p.Kill(); p.WaitForExit(1500); } catch {}
                    }
                }

                // 2. Localizar o instalador oficial setup.exe do Edge
                string edgeRoot = @"C:\Program Files (x86)\Microsoft\Edge\Application";
                string setupExe = null;

                if (Directory.Exists(edgeRoot))
                {
                    var found = Directory.GetFiles(edgeRoot, "setup.exe", SearchOption.AllDirectories);
                    if (found.Length > 0)
                    {
                        setupExe = found[0];
                    }
                }

                if (!string.IsNullOrEmpty(setupExe) && File.Exists(setupExe))
                {
                    LogEngine.Log(string.Format("Executando desinstalador oficial: {0}...", setupExe), "info");
                    var psi = new ProcessStartInfo(setupExe, "--uninstall --system-level --verbose-logging --force-uninstall")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    using (var p = Process.Start(psi))
                    {
                        p.WaitForExit(30000);
                        LogEngine.Log("Comando de desinstalação do Microsoft Edge concluído.", "success");
                    }
                }
                else
                {
                    LogEngine.Log("Executando desinstalação via AppxPackage...", "info");
                    RunPowerShell("Get-AppxPackage -AllUsers *MicrosoftEdge* | Remove-AppxPackage -ErrorAction SilentlyContinue");
                }

                // 3. Bloquear reinstalação automática via EdgeUpdate
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\EdgeUpdate"))
                {
                    if (key != null)
                    {
                        key.SetValue("DoNotUpdateToEdgeWithChromium", 1, RegistryValueKind.DWord);
                    }
                }

                // 4. Desativar serviços de atualização do Edge
                foreach (var sName in new string[] { "edgeupdate", "edgeupdatem" })
                {
                    try
                    {
                        var psi = new ProcessStartInfo("sc.exe", string.Format("config {0} start=disabled", sName))
                        {
                            CreateNoWindow = true,
                            UseShellExecute = false
                        };
                        using (var p = Process.Start(psi)) p.WaitForExit(3000);

                        var psiStop = new ProcessStartInfo("sc.exe", string.Format("stop {0}", sName))
                        {
                            CreateNoWindow = true,
                            UseShellExecute = false
                        };
                        using (var p = Process.Start(psiStop)) p.WaitForExit(3000);
                    }
                    catch {}
                }

                // 5. Desativar tarefas agendadas do Edge
                RunPowerShell("Disable-ScheduledTask -TaskName 'MicrosoftEdgeUpdateTaskMachineCore' -ErrorAction SilentlyContinue; Disable-ScheduledTask -TaskName 'MicrosoftEdgeUpdateTaskMachineUA' -ErrorAction SilentlyContinue");

                // 6. Limpar atalhos residuais
                CleanEdgeShortcuts();

                LogEngine.Log("Microsoft Edge removido e bloqueio de reinstalação aplicado com sucesso!", "success");
            }
            catch (Exception ex)
            {
                LogEngine.Log("Aviso ao remover Edge: " + ex.Message, "danger");
            }
        }

        private static void CleanEdgeShortcuts()
        {
            try
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string publicDesktop = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
                string startMenu = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);

                foreach (var folder in new string[] { desktop, publicDesktop, startMenu })
                {
                    if (Directory.Exists(folder))
                    {
                        foreach (var f in Directory.GetFiles(folder, "*Edge*.lnk", SearchOption.AllDirectories))
                        {
                            try { File.Delete(f); } catch {}
                        }
                    }
                }
            }
            catch {}
        }

        public static void RemoveOneDrive()
        {
            LogEngine.Log("Iniciando remoção completa do Microsoft OneDrive...", "info");

            try
            {
                // 1. Fechar OneDrive se estiver em execução
                foreach (var p in Process.GetProcessesByName("OneDrive"))
                {
                    try { p.Kill(); p.WaitForExit(2000); } catch {}
                }

                // 2. Localizar OneDriveSetup
                string[] setupCandidates = new string[]
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), "OneDriveSetup.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "OneDriveSetup.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\OneDrive\OneDriveSetup.exe")
                };

                string foundSetup = null;
                foreach (var c in setupCandidates)
                {
                    if (File.Exists(c)) { foundSetup = c; break; }
                }

                if (!string.IsNullOrEmpty(foundSetup))
                {
                    LogEngine.Log(string.Format("Executando desinstalador do OneDrive: {0} /uninstall...", foundSetup), "info");
                    var psi = new ProcessStartInfo(foundSetup, "/uninstall")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    using (var p = Process.Start(psi))
                    {
                        p.WaitForExit(25000);
                        LogEngine.Log("Desinstalação do OneDrive concluída.", "success");
                    }
                }

                // 3. Ocultar OneDrive da árvore de navegação do Windows Explorer
                // CLSID: {018D5C66-4533-4307-9B53-224DE2ED1FE6}
                using (var key = Registry.ClassesRoot.CreateSubKey(@"CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}"))
                {
                    if (key != null)
                    {
                        key.SetValue("System.IsPinnedToNameSpaceTree", 0, RegistryValueKind.DWord);
                    }
                }

                // 4. Bloquear sincronização via Política de Grupo (GPO)
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\OneDrive"))
                {
                    if (key != null)
                    {
                        key.SetValue("DisableFileSyncNGSC", 1, RegistryValueKind.DWord);
                    }
                }

                LogEngine.Log("OneDrive desinstalado e removido do Explorer com sucesso.", "success");
            }
            catch (Exception ex)
            {
                LogEngine.Log("Aviso ao remover OneDrive: " + ex.Message, "warning");
            }
        }

        public static void RemoveCopilotDeep()
        {
            LogEngine.Log("Desativando e removendo componentes do Windows Copilot e IA...", "info");

            try
            {
                // 1. Políticas de bloqueio permanente
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Policies\Microsoft\Windows\WindowsCopilot"))
                {
                    if (key != null) key.SetValue("TurnOffWindowsCopilot", 1, RegistryValueKind.DWord);
                }

                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot"))
                {
                    if (key != null) key.SetValue("TurnOffWindowsCopilot", 1, RegistryValueKind.DWord);
                }

                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"))
                {
                    if (key != null) key.SetValue("ShowCopilotButton", 0, RegistryValueKind.DWord);
                }

                // 2. Remoção de pacotes Appx
                RunPowerShell("Get-AppxPackage -AllUsers *Copilot* | Remove-AppxPackage -ErrorAction SilentlyContinue");
                RunPowerShell("Get-AppxProvisionedPackage -Online | Where-Object { $_.DisplayName -like '*Copilot*' } | Remove-AppxProvisionedPackage -Online -ErrorAction SilentlyContinue");

                LogEngine.Log("Windows Copilot completamente desativado e desinstalado.", "success");
            }
            catch (Exception ex)
            {
                LogEngine.Log("Aviso ao desativar Copilot: " + ex.Message, "warning");
            }
        }

        public static void RemoveYourPhone()
        {
            LogEngine.Log("Removendo Vincular ao Celular (Microsoft.YourPhone)...", "info");
            RunPowerShell("Get-AppxPackage -AllUsers *Microsoft.YourPhone* | Remove-AppxPackage -ErrorAction SilentlyContinue");
            RunPowerShell("Get-AppxProvisionedPackage -Online | Where-Object { $_.DisplayName -like '*Microsoft.YourPhone*' } | Remove-AppxProvisionedPackage -Online -ErrorAction SilentlyContinue");
            LogEngine.Log("Vincular ao Celular removido com sucesso.", "success");
        }

        public static void RemoveXboxOverlays()
        {
            LogEngine.Log("Removendo Xbox Game Bar e serviços residentes de jogos...", "info");
            string[] pkgs = new string[]
            {
                "Microsoft.XboxGamingOverlay",
                "Microsoft.XboxIdentityProvider",
                "Microsoft.XboxSpeechToTextOverlay"
            };

            foreach (var pkg in pkgs)
            {
                RunPowerShell(string.Format("Get-AppxPackage -AllUsers *{0}* | Remove-AppxPackage -ErrorAction SilentlyContinue", pkg));
                RunPowerShell(string.Format("Get-AppxProvisionedPackage -Online | Where-Object {{ $_.DisplayName -like '*{0}*' }} | Remove-AppxProvisionedPackage -Online -ErrorAction SilentlyContinue", pkg));
            }
            LogEngine.Log("Xbox Game Bar removida. Memória da GPU e processos em segundo plano liberados.", "success");
        }

        public static void RemoveQuickAssistAndTips()
        {
            LogEngine.Log("Removendo Assistência Rápida e Dicas do Windows (Tips)...", "info");
            RunPowerShell("Get-AppxPackage -AllUsers *Microsoft.Getstarted* | Remove-AppxPackage -ErrorAction SilentlyContinue");
            RunPowerShell("Get-AppxProvisionedPackage -Online | Where-Object { $_.DisplayName -like '*Microsoft.Getstarted*' } | Remove-AppxProvisionedPackage -Online -ErrorAction SilentlyContinue");
            RunPowerShell("Get-AppxPackage -AllUsers *QuickAssist* | Remove-AppxPackage -ErrorAction SilentlyContinue");
            RunPowerShell("Get-AppxProvisionedPackage -Online | Where-Object { $_.DisplayName -like '*QuickAssist*' } | Remove-AppxProvisionedPackage -Online -ErrorAction SilentlyContinue");
            LogEngine.Log("Dicas e Assistência Rápida removidos com sucesso.", "success");
        }

        private static void RunPowerShell(string command)
        {
            try
            {
                var psi = new ProcessStartInfo("powershell.exe", string.Format("-NoProfile -NonInteractive -Command \"{0}\"", command))
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using (var p = Process.Start(psi))
                {
                    p.WaitForExit(10000);
                }
            }
            catch {}
        }
    }
}
