using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DebloaterPC
{
    public class ScheduledTaskItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }
        public bool Disabled { get; set; }
    }

    public static class ScheduledTasksManager
    {
        public static readonly List<ScheduledTaskItem> Tasks = new List<ScheduledTaskItem>
        {
            new ScheduledTaskItem {
                Id = "appraiser",
                Name = "Microsoft Compatibility Appraiser",
                Path = @"\Microsoft\Windows\Application Experience\Microsoft Compatibility Appraiser",
                Description = "Vilão nº 1 de picos de 100% de CPU/Disco. Analisa programas e telemetria para compatibilidade."
            },
            new ScheduledTaskItem {
                Id = "programdata",
                Name = "ProgramDataUpdater",
                Path = @"\Microsoft\Windows\Application Experience\ProgramDataUpdater",
                Description = "Coleta informações de telemetria sobre programas instalados no sistema."
            },
            new ScheduledTaskItem {
                Id = "consolidator",
                Name = "Consolidator (CEIP)",
                Path = @"\Microsoft\Windows\Customer Experience Improvement Program\Consolidator",
                Description = "Consolida e envia métricas de uso do usuário à Microsoft se o CEIP estiver ativo."
            },
            new ScheduledTaskItem {
                Id = "usbceip",
                Name = "UsbCeip (CEIP USB)",
                Path = @"\Microsoft\Windows\Customer Experience Improvement Program\UsbCeip",
                Description = "Coleta telemetria e estatísticas de uso de periféricos e dispositivos USB."
            },
            new ScheduledTaskItem {
                Id = "diskdiag",
                Name = "DiskDiagnosticDataCollector",
                Path = @"\Microsoft\Windows\DiskDiagnostic\Microsoft-Windows-DiskDiagnosticDataCollector",
                Description = "Coleta dados de integridade e diagnósticos de discos para os servidores da MS."
            },
            new ScheduledTaskItem {
                Id = "dmclient",
                Name = "DmClient (Feedback)",
                Path = @"\Microsoft\Windows\Feedback\Siuf\DmClient",
                Description = "Tarefa de feedback de usuário e coleta periódica de dados de diagnóstico do sistema."
            }
        };

        public static bool IsTaskDisabled(string path)
        {
            try
            {
                var psi = new ProcessStartInfo("schtasks.exe", string.Format("/query /tn \"{0}\" /fo CSV /nh", path))
                {
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using (var p = Process.Start(psi))
                {
                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit(3000);
                    if (output.IndexOf("Disabled", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        output.IndexOf("Desativado", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }
            }
            catch {}
            return false;
        }

        public static bool ToggleTask(string path, bool disable)
        {
            try
            {
                if (!Diagnostics.IsAdministrator())
                {
                    LogEngine.Log(string.Format("Tarefa '{0}': Permissão negada. É necessário executar o Debloater como Administrador.", path), "warning");
                    return false;
                }

                string action = disable ? "/disable" : "/enable";
                LogEngine.Log(string.Format("Tarefa Agendada '{0}' -> Definindo para {1}...", path, disable ? "DESATIVADA" : "ATIVADA"), "info");

                var psi = new ProcessStartInfo("schtasks.exe", string.Format("/change /tn \"{0}\" {1}", path, action))
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using (var p = Process.Start(psi))
                {
                    p.WaitForExit(4000);
                    bool ok = (p.ExitCode == 0);
                    if (ok)
                    {
                        LogEngine.Log(string.Format("Tarefa '{0}' {1} com sucesso.", path, disable ? "desativada" : "ativada"), "success");
                        return true;
                    }
                }

                // Fallback via PowerShell Disable-ScheduledTask
                string taskName = System.IO.Path.GetFileName(path);
                string psCmd = string.Format("{0}-ScheduledTask -TaskName \"{1}\" -ErrorAction SilentlyContinue", disable ? "Disable" : "Enable", taskName);
                var psPsi = new ProcessStartInfo("powershell.exe", "-NoProfile -NonInteractive -Command \"" + psCmd + "\"")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using (var p = Process.Start(psPsi))
                {
                    p.WaitForExit(4000);
                    if (p.ExitCode == 0)
                    {
                        LogEngine.Log(string.Format("Tarefa '{0}' {1} com sucesso via PowerShell.", path, disable ? "desativada" : "ativada"), "success");
                        return true;
                    }
                }

                LogEngine.Log(string.Format("Aviso: Não foi possível alterar a tarefa '{0}'. Verifique as permissões de Administrador do sistema.", path), "warning");
                return false;
            }
            catch (Exception ex)
            {
                LogEngine.Log("Erro ao alterar tarefa: " + ex.Message, "danger");
                return false;
            }
        }

        public static bool IsAppraiserDisabled()
        {
            return IsTaskDisabled(@"\Microsoft\Windows\Application Experience\Microsoft Compatibility Appraiser");
        }

        public static bool ToggleCompatibilityAppraiser(bool enable)
        {
            // enable = true means active/enabled in system, so disable = !enable
            return ToggleTask(@"\Microsoft\Windows\Application Experience\Microsoft Compatibility Appraiser", !enable);
        }

        public static void ApplyAll(bool disable)
        {
            LogEngine.Log(string.Format("Configurando todas as 6 tarefas agendadas de telemetria ({0})...", disable ? "DESATIVAR" : "ATIVAR"), "info");
            int count = 0;
            foreach (var t in Tasks)
            {
                if (ToggleTask(t.Path, disable)) count++;
            }
            LogEngine.Log(string.Format("{0} de {1} tarefas agendadas foram atualizadas.", count, Tasks.Count), "success");
        }
    }
}
