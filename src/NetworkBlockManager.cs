using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DebloaterPC
{
    public static class NetworkBlockManager
    {
        private static readonly string HostsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts");
        private const string DebloaterHeader = "# [DebloaterPC Telemetry Block]";
        private const string DebloaterFooter = "# [End DebloaterPC Telemetry Block]";

        public static readonly string[] BlockedDomains = new string[]
        {
            "telemetry.microsoft.com",
            "vortex.data.microsoft.com",
            "settings-win.data.microsoft.com",
            "watson.telemetry.microsoft.com",
            "activity.windows.com",
            "mobile.pipe.aria.microsoft.com",
            "browser.pipe.aria.microsoft.com",
            "pipe.cloudapp.net",
            "oca.telemetry.microsoft.com",
            "diagnostics.support.microsoft.com"
        };

        public static bool IsHostsBlockActive()
        {
            try
            {
                if (!File.Exists(HostsPath)) return false;
                string content = File.ReadAllText(HostsPath);
                return content.Contains(DebloaterHeader);
            }
            catch {}
            return false;
        }

        public static bool IsHostsBlocked()
        {
            return IsHostsBlockActive();
        }

        public static bool ToggleHostsBlock(bool block)
        {
            try
            {
                if (!Diagnostics.IsAdministrator())
                {
                    LogEngine.Log("Erro ao alterar arquivo hosts: Privilégios de Administrador são necessários. Reinicie o programa como Administrador.", "danger");
                    return false;
                }

                if (!File.Exists(HostsPath)) return false;

                // Remove ReadOnly attribute if present
                var attrs = File.GetAttributes(HostsPath);
                if ((attrs & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    File.SetAttributes(HostsPath, attrs & ~FileAttributes.ReadOnly);
                }

                string content = File.ReadAllText(HostsPath);
                var sb = new StringBuilder();

                // Remove existing block section if present
                int startIdx = content.IndexOf(DebloaterHeader);
                if (startIdx != -1)
                {
                    int endIdx = content.IndexOf(DebloaterFooter);
                    if (endIdx != -1)
                    {
                        endIdx += DebloaterFooter.Length;
                        content = content.Remove(startIdx, endIdx - startIdx).Trim();
                    }
                }

                if (block)
                {
                    LogEngine.Log("Aplicando bloqueio de telemetria por Null-Routing (0.0.0.0) no arquivo hosts...", "info");
                    sb.AppendLine(content);
                    sb.AppendLine();
                    sb.AppendLine(DebloaterHeader);
                    foreach (var domain in BlockedDomains)
                    {
                        sb.AppendLine(string.Format("0.0.0.0 {0}", domain));
                    }
                    sb.AppendLine(DebloaterFooter);
                    File.WriteAllText(HostsPath, sb.ToString().Trim() + Environment.NewLine);
                    LogEngine.Log("Domínios de telemetria da Microsoft bloqueados no arquivo hosts.", "success");
                }
                else
                {
                    LogEngine.Log("Removendo entradas de bloqueio de telemetria do arquivo hosts...", "info");
                    File.WriteAllText(HostsPath, content.Trim() + Environment.NewLine);
                    LogEngine.Log("Bloqueio de hosts restaurado ao padrão.", "info");
                }

                // Flush DNS cache
                RegistryManager.FlushDns();
                return true;
            }
            catch (Exception ex)
            {
                LogEngine.Log("Erro ao alterar arquivo hosts: " + ex.Message, "danger");
                return false;
            }
        }

        public static bool IsFirewallBlockActive()
        {
            try
            {
                var psi = new ProcessStartInfo("netsh.exe", "advfirewall firewall show rule name=\"DebloaterPC_BlockDiagTrack\"")
                {
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using (var p = Process.Start(psi))
                {
                    string outStr = p.StandardOutput.ReadToEnd();
                    p.WaitForExit(3000);
                    return outStr.IndexOf("DebloaterPC_BlockDiagTrack", StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }
            catch {}
            return false;
        }

        public static bool ToggleFirewallBlock(bool block)
        {
            try
            {
                if (block)
                {
                    LogEngine.Log("Criando regras de saída no Windows Defender Firewall para bloquear telemetria...", "info");
                    // Remove first to avoid duplicates
                    RunNetsh("advfirewall firewall delete rule name=\"DebloaterPC_BlockDiagTrack\"");
                    RunNetsh("advfirewall firewall delete rule name=\"DebloaterPC_BlockWerFault\"");

                    // Add block rule for DiagTrack service
                    RunNetsh("advfirewall firewall add rule name=\"DebloaterPC_BlockDiagTrack\" dir=out service=\"DiagTrack\" action=block enable=yes");
                    // Add block rule for WerFault.exe
                    RunNetsh("advfirewall firewall add rule name=\"DebloaterPC_BlockWerFault\" dir=out program=\"%SystemRoot%\\System32\\WerFault.exe\" action=block enable=yes");

                    LogEngine.Log("Regras de saída do Firewall criadas: tráfego do DiagTrack e WerFault bloqueado.", "success");
                }
                else
                {
                    LogEngine.Log("Removendo regras de bloqueio do Firewall...", "info");
                    RunNetsh("advfirewall firewall delete rule name=\"DebloaterPC_BlockDiagTrack\"");
                    RunNetsh("advfirewall firewall delete rule name=\"DebloaterPC_BlockWerFault\"");
                    LogEngine.Log("Regras de Firewall restauradas.", "info");
                }
                return true;
            }
            catch (Exception ex)
            {
                LogEngine.Log("Erro nas regras de firewall: " + ex.Message, "danger");
                return false;
            }
        }

        private static void RunNetsh(string args)
        {
            try
            {
                var psi = new ProcessStartInfo("netsh.exe", args)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using (var p = Process.Start(psi))
                {
                    p.WaitForExit(3000);
                }
            }
            catch {}
        }
    }
}
