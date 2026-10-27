using System;
using Microsoft.Win32;

namespace DebloaterPC
{
    public static class WindowsUpdateManager
    {
        private const string WuPolicyKey = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate";
        private const string DoPolicyKey = @"SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization";

        public static bool IsSecurityModeActive()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(WuPolicyKey, false))
                {
                    if (key != null)
                    {
                        var branch = key.GetValue("BranchReadinessLevel");
                        var defer = key.GetValue("DeferFeatureUpdates");
                        if (branch is int && defer is int)
                        {
                            return ((int)branch == 16 || (int)branch == 32) && (int)defer == 1;
                        }
                    }
                }
            }
            catch {}
            return false;
        }

        public static bool SetSecurityMode(bool enable)
        {
            try
            {
                using (var key = Registry.LocalMachine.CreateSubKey(WuPolicyKey))
                {
                    if (key == null) return false;

                    if (enable)
                    {
                        LogEngine.Log("Configurando Windows Update para o Modo Segurança / Equilibrado (Pausa atualizações instáveis de recursos por até 365 dias, mantendo patches críticos de segurança)...", "info");
                        // 16 = CurrentBranchForBusiness / Semi-Annual Channel
                        key.SetValue("BranchReadinessLevel", 16, RegistryValueKind.DWord);
                        key.SetValue("DeferFeatureUpdates", 1, RegistryValueKind.DWord);
                        key.SetValue("DeferFeatureUpdatesPeriodInDays", 365, RegistryValueKind.DWord);
                        key.SetValue("DeferQualityUpdates", 0, RegistryValueKind.DWord);
                        LogEngine.Log("Windows Update ajustado com sucesso para modo Segurança / Estabilidade.", "success");
                    }
                    else
                    {
                        LogEngine.Log("Restaurando Windows Update para canal padrão do consumidor...", "info");
                        try { key.DeleteValue("BranchReadinessLevel", false); } catch {}
                        try { key.DeleteValue("DeferFeatureUpdates", false); } catch {}
                        try { key.DeleteValue("DeferFeatureUpdatesPeriodInDays", false); } catch {}
                        LogEngine.Log("Configurações de adiamento de recursos do Windows Update removidas.", "info");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LogEngine.Log("Erro ao configurar Windows Update: " + ex.Message, "danger");
                return false;
            }
        }

        public static bool IsDeliveryOptimizationP2PDisabled()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(DoPolicyKey, false))
                {
                    if (key != null)
                    {
                        var mode = key.GetValue("DODownloadMode");
                        if (mode is int) return (int)mode == 0;
                    }
                }
            }
            catch {}
            return false;
        }

        public static bool SetDeliveryOptimizationP2P(bool disableP2P)
        {
            try
            {
                using (var key = Registry.LocalMachine.CreateSubKey(DoPolicyKey))
                {
                    if (key == null) return false;

                    if (disableP2P)
                    {
                        LogEngine.Log("Desativando compartilhamento P2P (Otimização de Entrega) para não consumir largura de banda de upload...", "info");
                        // 0 = Bypass / Simple HTTP only (sem compartilhamento P2P de atualizações para terceiros)
                        key.SetValue("DODownloadMode", 0, RegistryValueKind.DWord);
                        LogEngine.Log("Otimização de Entrega configurada em modo HTTP simples (P2P desativado).", "success");
                    }
                    else
                    {
                        LogEngine.Log("Restaurando Otimização de Entrega P2P para o padrão do Windows...", "info");
                        try { key.DeleteValue("DODownloadMode", false); } catch {}
                        LogEngine.Log("Otimização de Entrega restaurada.", "info");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LogEngine.Log("Erro ao configurar Otimização de Entrega: " + ex.Message, "danger");
                return false;
            }
        }
    }
}
