using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace DebloaterPC
{
    public static class Windows11ModernManager
    {
        private const string WindowsAiKey = @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI";
        private const string LabConfigKey = @"SYSTEM\Setup\LabConfig";
        private const string MoSetupKey = @"SYSTEM\Setup\MoSetup";

        public static bool IsRecallDisabled()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(WindowsAiKey, false))
                {
                    if (key != null)
                    {
                        var val = key.GetValue("DisableAIDataAnalysis");
                        if (val is int) return (int)val == 1;
                    }
                }
            }
            catch {}
            return false;
        }

        public static bool SetRecallDisabled(bool disable)
        {
            try
            {
                if (disable)
                {
                    LogEngine.Log("Desativando e bloqueando o Windows Recall AI (Windows 11 24H2+)...", "info");

                    // 1. Políticas de Grupo HKLM e HKCU
                    using (var key = Registry.LocalMachine.CreateSubKey(WindowsAiKey))
                    {
                        if (key != null)
                        {
                            key.SetValue("DisableAIDataAnalysis", 1, RegistryValueKind.DWord);
                            key.SetValue("TurnOffRecall", 1, RegistryValueKind.DWord);
                        }
                    }

                    using (var key = Registry.CurrentUser.CreateSubKey(WindowsAiKey))
                    {
                        if (key != null)
                        {
                            key.SetValue("DisableAIDataAnalysis", 1, RegistryValueKind.DWord);
                            key.SetValue("TurnOffRecall", 1, RegistryValueKind.DWord);
                        }
                    }

                    // 2. DISM /Online /Disable-Feature /FeatureName:Recall (se aplicável ao 24H2)
                    RunDismDisableFeature("Recall");

                    LogEngine.Log("Windows Recall AI desativado e gravação contínua de tela bloqueada com sucesso.", "success");
                }
                else
                {
                    LogEngine.Log("Restaurando configurações do Windows Recall AI...", "info");
                    using (var key = Registry.LocalMachine.OpenSubKey(WindowsAiKey, true))
                    {
                        if (key != null)
                        {
                            try { key.DeleteValue("DisableAIDataAnalysis", false); } catch {}
                            try { key.DeleteValue("TurnOffRecall", false); } catch {}
                        }
                    }
                    using (var key = Registry.CurrentUser.OpenSubKey(WindowsAiKey, true))
                    {
                        if (key != null)
                        {
                            try { key.DeleteValue("DisableAIDataAnalysis", false); } catch {}
                            try { key.DeleteValue("TurnOffRecall", false); } catch {}
                        }
                    }
                    LogEngine.Log("Políticas restritivas do Recall AI removidas.", "info");
                }
                return true;
            }
            catch (Exception ex)
            {
                LogEngine.Log("Erro ao alterar configurações do Recall: " + ex.Message, "danger");
                return false;
            }
        }

        public static bool IsLabConfigActive()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(LabConfigKey, false))
                {
                    if (key != null)
                    {
                        var tpm = key.GetValue("BypassTPMCheck");
                        if (tpm is int) return (int)tpm == 1;
                    }
                }
            }
            catch {}
            return false;
        }

        public static bool SetLabConfigBypass(bool enable)
        {
            try
            {
                if (enable)
                {
                    LogEngine.Log("Aplicando bypass de requisitos de hardware do Windows 11 (TPM 2.0, SecureBoot, CPU e RAM)...", "info");

                    using (var key = Registry.LocalMachine.CreateSubKey(LabConfigKey))
                    {
                        if (key != null)
                        {
                            key.SetValue("BypassTPMCheck", 1, RegistryValueKind.DWord);
                            key.SetValue("BypassSecureBootCheck", 1, RegistryValueKind.DWord);
                            key.SetValue("BypassCPUCheck", 1, RegistryValueKind.DWord);
                            key.SetValue("BypassRAMCheck", 1, RegistryValueKind.DWord);
                            key.SetValue("BypassStorageCheck", 1, RegistryValueKind.DWord);
                        }
                    }

                    using (var key = Registry.LocalMachine.CreateSubKey(MoSetupKey))
                    {
                        if (key != null)
                        {
                            key.SetValue("AllowUpgradesWithUnsupportedTPMOrCPU", 1, RegistryValueKind.DWord);
                        }
                    }

                    LogEngine.Log("Bypass de hardware do Windows 11 gravado no registro com sucesso (pronto para atualizações e instalações).", "success");
                }
                else
                {
                    LogEngine.Log("Removendo chaves de bypass de hardware (LabConfig)...", "info");
                    try { Registry.LocalMachine.DeleteSubKeyTree(LabConfigKey, false); } catch {}
                    using (var key = Registry.LocalMachine.OpenSubKey(MoSetupKey, true))
                    {
                        if (key != null)
                        {
                            try { key.DeleteValue("AllowUpgradesWithUnsupportedTPMOrCPU", false); } catch {}
                        }
                    }
                    LogEngine.Log("Bypass de hardware desativado.", "info");
                }
                return true;
            }
            catch (Exception ex)
            {
                LogEngine.Log("Erro ao configurar LabConfig: " + ex.Message, "danger");
                return false;
            }
        }

        private static void RunDismDisableFeature(string featureName)
        {
            try
            {
                var psi = new ProcessStartInfo("dism.exe", string.Format("/Online /Disable-Feature /FeatureName:{0} /NoRestart /Quiet", featureName))
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using (var p = Process.Start(psi))
                {
                    p.WaitForExit(6000);
                }
            }
            catch {}
        }
    }
}
