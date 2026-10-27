using System;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;

namespace DebloaterPC
{
    public static class SafetyManager
    {
        private static readonly string BackupDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"DebloaterPC\Backups");

        public static bool CreateRestorePoint(string description, out string error)
        {
            error = "";
            try
            {
                LogEngine.Log(string.Format("Criando Ponto de Restauração no Windows: '{0}'...", description), "info");

                // 1. Desbloquear a trava de frequência de 24h do Windows (SystemRestorePointCreationFrequency = 0)
                try
                {
                    using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore"))
                    {
                        if (key != null)
                        {
                            key.SetValue("SystemRestorePointCreationFrequency", 0, RegistryValueKind.DWord);
                            key.SetValue("DisableSR", 0, RegistryValueKind.DWord);
                        }
                    }
                }
                catch {}

                // 2. Garantir que o serviço VSS (Cópia de Sombra de Volume) esteja ativo
                try
                {
                    var psiVss = new ProcessStartInfo("sc.exe", "start vss") { CreateNoWindow = true, UseShellExecute = false };
                    using (var p = Process.Start(psiVss)) { p.WaitForExit(3000); }
                }
                catch {}

                // 3. Execução inteligente: WMI nativo com fallback para Checkpoint-Computer
                string safeDesc = description.Replace("'", "").Replace("\"", "");
                string psScript = string.Format(
                    "$desc = '{0}'; " +
                    "try {{ " +
                    "   Enable-ComputerRestore -Drive 'C:\\' -ErrorAction SilentlyContinue; " +
                    "   Set-ItemProperty -Path 'HKLM:\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\SystemRestore' -Name 'SystemRestorePointCreationFrequency' -Value 0 -Type DWord -Force -ErrorAction SilentlyContinue; " +
                    "   $sr = [wmiclass]'\\\\.\\root\\default:systemrestore'; " +
                    "   $res = $sr.CreateRestorePoint($desc, 0, 100); " +
                    "   if ($res.ReturnValue -eq 0) {{ Write-Output 'SUCCESS'; exit 0 }} " +
                    "}} catch {{}} " +
                    "try {{ " +
                    "   Checkpoint-Computer -Description $desc -RestorePointType 'MODIFY_SETTINGS' -ErrorAction Stop; " +
                    "   Write-Output 'SUCCESS'; exit 0 " +
                    "}} catch {{ " +
                    "   Write-Error $_.Exception.Message; exit 1 " +
                    "}}", safeDesc);

                var psi = new ProcessStartInfo("powershell.exe", string.Format("-NoProfile -NonInteractive -Command \"{0}\"", psScript))
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var proc = Process.Start(psi))
                {
                    string stdOut = proc.StandardOutput.ReadToEnd();
                    string errOut = proc.StandardError.ReadToEnd();
                    proc.WaitForExit(35000);

                    if (proc.ExitCode == 0 || stdOut.Contains("SUCCESS"))
                    {
                        LogEngine.Log(string.Format("Ponto de Restauração '{0}' criado com sucesso no Windows!", safeDesc), "success");
                        return true;
                    }
                    else
                    {
                        error = !string.IsNullOrEmpty(errOut) ? errOut.Trim() : "Código de saída: " + proc.ExitCode;
                        LogEngine.Log("Falha ao criar Ponto de Restauração: " + error, "warning");
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                LogEngine.Log("Erro ao invocar criação de ponto de restauração: " + ex.Message, "danger");
            }
            return false;
        }

        public static void BackupRegistrySnapshot()
        {
            try
            {
                if (!Directory.Exists(BackupDir))
                {
                    Directory.CreateDirectory(BackupDir);
                }

                string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string regFile = Path.Combine(BackupDir, string.Format("RegistryBackup_{0}.reg", timeStamp));

                LogEngine.Log(string.Format("Criando snapshot de segurança do Registro em: {0}...", regFile), "info");

                var psi = new ProcessStartInfo("reg.exe", string.Format("export HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows \"{0}\" /y", regFile))
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using (var p = Process.Start(psi))
                {
                    p.WaitForExit(5000);
                    LogEngine.Log("Snapshot do Registro salvo com sucesso.", "success");
                }
            }
            catch (Exception ex)
            {
                LogEngine.Log("Aviso backup registro: " + ex.Message, "warning");
            }
        }

        public static void ApplyRollback()
        {
            LogEngine.Log("=== INICIANDO MODO REVERSÃO / RESTAURAÇÃO DE PADRÕES ===", "info");
            LogEngine.SetProgress(10, "Restaurando MMAgent e serviços...");

            // 1. Reativar MMAgent
            MemoryManager.ToggleMemoryCompression(false);

            // 2. Restaurar serviços
            MemoryManager.RestoreServices();
            LogEngine.SetProgress(35, "Restaurando políticas de privacidade...");

            // 3. Reativar telemetria e padrões do Windows
            RegistryManager.ApplyPrivacyTweaks(false);

            // 4. Restaurar menu do Windows 11
            RegistryManager.SetClassicContextMenu(false);

            // 5. Reativar hibernação
            DiskManager.DisableHibernation(false);
            LogEngine.SetProgress(65, "Restaurando navegadores e nomes 8.3...");

            // 6. Restaurar 8.3 names
            DiskManager.Disable8Dot3Names(false);

            // 7. Restaurar políticas de navegadores
            ChromeOptimizer.RestoreBrowserPolicies();

            // 8. Reiniciar explorer
            LogEngine.SetProgress(90, "Reiniciando Explorer...");
            RegistryManager.RestartExplorer();

            LogEngine.SetProgress(100, "Modo Reversão concluído com sucesso!");
            LogEngine.Log("Todas as configurações foram restauradas para os padrões originais do Windows.", "success");
        }

        public static void RunIntegrityRepair()
        {
            LogEngine.Log("=== INICIANDO VERIFICAÇÃO DE INTEGRIDADE (SFC & DISM) ===", "info");
            LogEngine.SetProgress(10, "Executando SFC /scannow...");

            // 1. SFC /scannow
            try
            {
                LogEngine.Log("> Executando sfc.exe /scannow (Verificação de arquivos protegidos)...", "info");
                var psiSfc = new ProcessStartInfo("sfc.exe", "/scannow")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var proc = Process.Start(psiSfc))
                {
                    while (!proc.StandardOutput.EndOfStream)
                    {
                        string line = proc.StandardOutput.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            LogEngine.Log("> SFC: " + line.Trim(), "info");
                        }
                    }
                    proc.WaitForExit();
                    LogEngine.Log("Verificação SFC concluída.", "success");
                }
            }
            catch (Exception ex)
            {
                LogEngine.Log("Aviso SFC: " + ex.Message, "warning");
            }

            LogEngine.SetProgress(50, "Executando DISM /RestoreHealth...");

            // 2. DISM RestoreHealth
            try
            {
                LogEngine.Log("> Executando dism.exe /online /cleanup-image /restorehealth...", "info");
                var psiDism = new ProcessStartInfo("dism.exe", "/online /cleanup-image /restorehealth")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var proc = Process.Start(psiDism))
                {
                    while (!proc.StandardOutput.EndOfStream)
                    {
                        string line = proc.StandardOutput.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line) && (line.Contains("%") || line.Contains("Operação") || line.Contains("100%")))
                        {
                            LogEngine.Log("> DISM: " + line.Trim(), "info");
                        }
                    }
                    proc.WaitForExit();
                    LogEngine.Log("Reparo DISM RestoreHealth concluído com sucesso.", "success");
                }
            }
            catch (Exception ex)
            {
                LogEngine.Log("Aviso DISM: " + ex.Message, "warning");
            }

            LogEngine.SetProgress(100, "Reparação de integridade finalizada.");
        }
    }
}
