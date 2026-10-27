using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DebloaterPC
{
    public class AppxItem
    {
        public string Name { get; set; }
        public string PackageFullName { get; set; }
        public string Version { get; set; }
        public double SizeMb { get; set; }
        public string Category { get; set; }
        public bool IsWhitelisted { get; set; }
    }

    public static class UwpDebloater
    {
        // Unbreakable Whitelist - NUNCA REMOVER
        public static readonly HashSet<string> Whitelist = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Microsoft.WindowsStore",
            "Microsoft.StorePurchaseApp",
            "Microsoft.WindowsCalculator",
            "Microsoft.Windows.Photos",
            "Microsoft.Paint",
            "Microsoft.DesktopAppInstaller",
            "Microsoft.UI.Xaml",
            "Microsoft.NET.Native",
            "Microsoft.VCLibs"
        };

        public static readonly string[] SponsoredApps = new string[]
        {
            "Clipchamp.Clipchamp",
            "SpotifyAB.SpotifyMusic",
            "ByteDance.TikTok",
            "Facebook.Instagram",
            "Disney.37853FC22B2CE",
            "Amazon.AmazonPrimeVideo",
            "Netflix",
            "Microsoft.MicrosoftSolitaireCollection"
        };

        public static readonly string[] AssistantApps = new string[]
        {
            "Microsoft.549981C3F5F10", // Cortana
            "Microsoft.WindowsFeedbackHub",
            "Microsoft.GetHelp",
            "Microsoft.Getstarted"
        };

        public static readonly string[] WidgetApps = new string[]
        {
            "Microsoft.BingWeather",
            "Microsoft.BingNews",
            "Microsoft.MixedReality.Portal",
            "Microsoft.People",
            "Microsoft.YourPhone"
        };

        public static List<AppxItem> ScanInstalledApps()
        {
            var list = new List<AppxItem>();
            try
            {
                // Script otimizado que retorna CSV com Name, PackageFullName, Version, InstallLocation
                string script = "Get-AppxPackage -AllUsers | Where-Object { -not $_.IsFramework -and $_.NonRemovable -ne $true } | Select-Object Name, PackageFullName, Version, InstallLocation | ForEach-Object { \"$($_.Name)|$($_.PackageFullName)|$($_.Version)|$($_.InstallLocation)\" }";
                var psi = new ProcessStartInfo("powershell.exe", "-NoProfile -NonInteractive -Command \"" + script + "\"")
                {
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                using (var p = Process.Start(psi))
                {
                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit(6000);

                    var lines = output.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        var parts = line.Split('|');
                        if (parts.Length >= 3)
                        {
                            string name = parts[0].Trim();
                            string fullName = parts[1].Trim();
                            string version = parts[2].Trim();
                            string installLoc = parts.Length > 3 ? parts[3].Trim() : "";

                            if (string.IsNullOrEmpty(name)) continue;

                            double sizeMb = 0;
                            if (!string.IsNullOrEmpty(installLoc) && Directory.Exists(installLoc))
                            {
                                try
                                {
                                    var dirInfo = new DirectoryInfo(installLoc);
                                    long bytes = 0;
                                    foreach (var file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                                    {
                                        bytes += file.Length;
                                    }
                                    sizeMb = Math.Round((double)bytes / (1024 * 1024), 1);
                                }
                                catch {}
                            }

                            string category = "Sistema / Utilitário";
                            string lower = name.ToLowerInvariant();
                            if (lower.Contains("xbox") || lower.Contains("game") || lower.Contains("solitaire")) category = "Jogos / Xbox";
                            else if (lower.Contains("bing") || lower.Contains("news") || lower.Contains("weather")) category = "Notícias / Clima";
                            else if (lower.Contains("spotify") || lower.Contains("netflix") || lower.Contains("disney") || lower.Contains("tiktok") || lower.Contains("instagram") || lower.Contains("clipchamp")) category = "Patrocinados / Mídia";
                            else if (lower.Contains("cortana") || lower.Contains("feedback") || lower.Contains("help") || lower.Contains("getstarted")) category = "Assistentes / Telemetria";

                            list.Add(new AppxItem
                            {
                                Name = name,
                                PackageFullName = fullName,
                                Version = version,
                                SizeMb = sizeMb,
                                Category = category,
                                IsWhitelisted = IsWhitelisted(name)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogEngine.Log("Erro ao escanear aplicativos UWP instalados: " + ex.Message, "danger");
            }
            return list;
        }

        public static bool RemovePackage(string packageName, bool deprovision)
        {
            if (IsWhitelisted(packageName))
            {
                LogEngine.Log(string.Format("Aplicativo '{0}' está na Whitelist protegida e não pode ser removido.", packageName), "warning");
                return false;
            }

            try
            {
                LogEngine.Log(string.Format("Removendo aplicativo '{0}'...", packageName), "info");
                string psCmd = string.Format("Get-AppxPackage -Name *{0}* | Remove-AppxPackage -ErrorAction SilentlyContinue", packageName);
                RunPowerShell(psCmd);

                if (deprovision)
                {
                    string psProv = string.Format("Get-AppxProvisionedPackage -Online | Where-Object {{ $_.DisplayName -like '*{0}*' }} | Remove-AppxProvisionedPackage -Online -ErrorAction SilentlyContinue", packageName);
                    RunPowerShell(psProv);
                }

                LogEngine.Log(string.Format("Aplicativo '{0}' removido com sucesso.", packageName), "success");
                return true;
            }
            catch (Exception ex)
            {
                LogEngine.Log(string.Format("Erro ao remover {0}: {1}", packageName, ex.Message), "danger");
                return false;
            }
        }

        public static void RunDebloat(bool removeSponsored, bool removeAssistants, bool removeWidgets, bool deprovision)
        {
            var appsToRemove = new List<string>();
            if (removeSponsored) appsToRemove.AddRange(SponsoredApps);
            if (removeAssistants) appsToRemove.AddRange(AssistantApps);
            if (removeWidgets) appsToRemove.AddRange(WidgetApps);

            LogEngine.Log(string.Format("Iniciando Debloat UWP de {0} aplicativos selecionados...", appsToRemove.Count), "info");

            int removedCount = 0;
            foreach (var app in appsToRemove)
            {
                if (IsWhitelisted(app))
                {
                    LogEngine.Log(string.Format("Protegido pela Whitelist: '{0}' mantido.", app), "warning");
                    continue;
                }

                if (RemovePackage(app, deprovision))
                {
                    removedCount++;
                }
            }

            LogEngine.Log(string.Format("Debloat UWP finalizado com sucesso. {0} pacotes removidos.", removedCount), "success");
        }

        public static void ResurrectWindowsStore()
        {
            try
            {
                LogEngine.Log("Iniciando Procedimento de Emergência: Ressuscitar e Reinstalar Microsoft Store e Apps Nativos...", "info");

                // 1. Executa wsreset -i (comando oficial da Microsoft para reinstalação do Store)
                try
                {
                    var psi = new ProcessStartInfo("wsreset.exe", "-i")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    using (var p = Process.Start(psi))
                    {
                        p.WaitForExit(5000);
                    }
                }
                catch {}

                // 2. Re-registra todos os pacotes nativos da Microsoft Store
                string registerScript = "Get-AppxPackage -AllUsers *WindowsStore* | Foreach {Add-AppxPackage -DisableDevelopmentMode -Register \"$($_.InstallLocation)\\AppxManifest.xml\" -ErrorAction SilentlyContinue}";
                RunPowerShell(registerScript);

                LogEngine.Log("Comando de recuperação da Microsoft Store executado. Aguarde alguns instantes para a reinstalação em segundo plano pelo Windows.", "success");
            }
            catch (Exception ex)
            {
                LogEngine.Log("Erro ao ressuscitar Microsoft Store: " + ex.Message, "danger");
            }
        }

        private static bool IsWhitelisted(string appName)
        {
            if (Whitelist.Contains(appName)) return true;
            string lower = appName.ToLowerInvariant();
            if (lower.Contains("store") || lower.Contains("calculator") || lower.Contains("photos") || lower.Contains("paint")
                || lower.Contains("dell") || lower.Contains("realtek") || lower.Contains("waves") || lower.Contains("intel")
                || lower.Contains("nvidia") || lower.Contains("amd") || lower.Contains("vclibs") || lower.Contains("xaml"))
            {
                return true;
            }
            return false;
        }

        private static void RunPowerShell(string command)
        {
            var psi = new ProcessStartInfo("powershell.exe", string.Format("-NoProfile -NonInteractive -Command \"{0}\"", command))
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            using (var p = Process.Start(psi))
            {
                p.WaitForExit(7000);
            }
        }
    }
}
