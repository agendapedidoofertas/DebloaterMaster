using System;
using System.Diagnostics;
using System.ServiceProcess;

namespace DebloaterPC
{
    public static class MemoryManager
    {
        [System.Runtime.InteropServices.DllImport("psapi.dll")]
        private static extern int EmptyWorkingSet(IntPtr hwProc);

        public static void FlushWorkingSets()
        {
            try
            {
                LogEngine.Log("Iniciando esvaziamento imediato de WorkingSet de processos (liberando RAM)...", "info");
                int cleanedCount = 0;
                var procList = Process.GetProcesses();
                foreach (var p in procList)
                {
                    try
                    {
                        if (!p.HasExited)
                        {
                            EmptyWorkingSet(p.Handle);
                            cleanedCount++;
                        }
                    }
                    catch {}
                    finally
                    {
                        try { p.Dispose(); } catch {}
                    }
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
                LogEngine.Log(string.Format("Memória RAM otimizada com sucesso! Memória de {0} processos foi limpa.", cleanedCount), "success");
            }
            catch (Exception ex)
            {
                LogEngine.Log("Aviso ao otimizar memória RAM: " + ex.Message, "warning");
            }
        }

        public static bool ToggleMemoryCompression(bool disable)
        {
            try
            {
                string cmd = disable ? "Disable-MMAgent -mc" : "Enable-MMAgent -mc";
                LogEngine.Log(string.Format("Configurando compressão de memória: {0}...", disable ? "DESATIVAR" : "ATIVAR"), "info");

                var psi = new ProcessStartInfo("powershell.exe", string.Format("-NoProfile -NonInteractive -Command \"{0}\"", cmd))
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var proc = Process.Start(psi))
                {
                    proc.WaitForExit(10000);
                    if (proc.ExitCode == 0)
                    {
                        LogEngine.Log(string.Format("Compressão de memória {0} com sucesso.", disable ? "desativada (alívio de CPU em dual-core)" : "reativada"), "success");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogEngine.Log("Erro ao alterar MMAgent: " + ex.Message, "danger");
            }
            return false;
        }

        public static void ConfigureServices(bool disableTelemetry, bool disableSysMain)
        {
            if (disableTelemetry)
            {
                ManageService("DiagTrack", false);
                ManageService("dmwappushservice", false);
            }

            if (disableSysMain)
            {
                ManageService("SysMain", false);
            }
        }

        public static void RestoreServices()
        {
            LogEngine.Log("Restaurando serviços para o padrão do Windows...", "info");
            ManageService("DiagTrack", true);
            ManageService("dmwappushservice", true);
            ManageService("SysMain", true);
        }

        private static void ManageService(string serviceName, bool enable)
        {
            try
            {
                string startType = enable ? "auto" : "disabled";
                LogEngine.Log(string.Format("Serviço '{0}' -> Definindo inicialização como: {1}", serviceName, startType), "info");

                var psiConfig = new ProcessStartInfo("sc.exe", string.Format("config {0} start={1}", serviceName, startType))
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using (var p = Process.Start(psiConfig))
                {
                    p.WaitForExit(4000);
                }

                if (!enable)
                {
                    var psiStop = new ProcessStartInfo("sc.exe", string.Format("stop {0}", serviceName))
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    using (var p = Process.Start(psiStop))
                    {
                        p.WaitForExit(4000);
                    }
                    LogEngine.Log(string.Format("Serviço '{0}' interrompido e desativado com sucesso.", serviceName), "success");
                }
                else
                {
                    var psiStart = new ProcessStartInfo("sc.exe", string.Format("start {0}", serviceName))
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    using (var p = Process.Start(psiStart))
                    {
                        p.WaitForExit(4000);
                    }
                    LogEngine.Log(string.Format("Serviço '{0}' reativado.", serviceName), "success");
                }
            }
            catch (Exception ex)
            {
                LogEngine.Log(string.Format("Aviso ao gerenciar serviço {0}: {1}", serviceName, ex.Message), "warning");
            }
        }

        public static void SetHighPerformancePlan()
        {
            try
            {
                LogEngine.Log("Aplicando Plano de Energia: Alto Desempenho...", "info");
                var psi = new ProcessStartInfo("powercfg.exe", "/setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using (var p = Process.Start(psi))
                {
                    p.WaitForExit(3000);
                    LogEngine.Log("Plano de Alto Desempenho ativado com sucesso.", "success");
                }
            }
            catch (Exception ex)
            {
                LogEngine.Log("Aviso ao alterar plano de energia: " + ex.Message, "warning");
            }
        }
    }
}
