using System;
using Microsoft.Win32;

namespace DebloaterPC
{
    public static class GamerOptimizer
    {
        private const string MultimediaKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile";
        private const string GamesTaskKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games";
        private const string MouseKey = @"Control Panel\Mouse";

        public static bool IsNetworkThrottlingDisabled()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(MultimediaKey, false))
                {
                    if (key != null)
                    {
                        var val = key.GetValue("NetworkThrottlingIndex");
                        if (val is int) return (int)val == unchecked((int)0xffffffff);
                    }
                }
            }
            catch {}
            return false;
        }

        public static bool SetNetworkThrottlingDisabled(bool disable)
        {
            try
            {
                using (var key = Registry.LocalMachine.CreateSubKey(MultimediaKey))
                {
                    if (key == null) return false;

                    if (disable)
                    {
                        LogEngine.Log("Desativando Network Throttling do Windows e ajustando Responsividade do Sistema para 0% de reserva para processos de fundo...", "info");
                        // 0xffffffff = Desativa limitação de pacotes de rede (máximo throughput e menor jitter/ping)
                        key.SetValue("NetworkThrottlingIndex", unchecked((int)0xffffffff), RegistryValueKind.DWord);
                        // 0 = Dedica 100% de CPU para jogos e aplicativos em primeiro plano
                        key.SetValue("SystemResponsiveness", 0, RegistryValueKind.DWord);

                        // Ajusta prioridade da GPU e agendador para tarefas de Jogos
                        using (var taskKey = Registry.LocalMachine.CreateSubKey(GamesTaskKey))
                        {
                            if (taskKey != null)
                            {
                                taskKey.SetValue("GPU Priority", 8, RegistryValueKind.DWord);
                                taskKey.SetValue("Priority", 6, RegistryValueKind.DWord);
                                taskKey.SetValue("Scheduling Category", "High", RegistryValueKind.String);
                                taskKey.SetValue("SFIO Priority", "High", RegistryValueKind.String);
                            }
                        }

                        LogEngine.Log("Otimizações de rede e prioridade de GPU para jogos aplicadas com sucesso.", "success");
                    }
                    else
                    {
                        LogEngine.Log("Restaurando valores padrão de rede e multimídia...", "info");
                        key.SetValue("NetworkThrottlingIndex", 10, RegistryValueKind.DWord);
                        key.SetValue("SystemResponsiveness", 20, RegistryValueKind.DWord);
                        using (var taskKey = Registry.LocalMachine.CreateSubKey(GamesTaskKey))
                        {
                            if (taskKey != null)
                            {
                                taskKey.SetValue("GPU Priority", 8, RegistryValueKind.DWord);
                                taskKey.SetValue("Priority", 2, RegistryValueKind.DWord);
                                taskKey.SetValue("Scheduling Category", "Medium", RegistryValueKind.String);
                                taskKey.SetValue("SFIO Priority", "Normal", RegistryValueKind.String);
                            }
                        }
                        LogEngine.Log("Valores padrão multimídia restaurados.", "info");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LogEngine.Log("Erro ao ajustar Network Throttling: " + ex.Message, "danger");
                return false;
            }
        }

        public static bool IsMouseAccelerationDisabled()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(MouseKey, false))
                {
                    if (key != null)
                    {
                        var speed = key.GetValue("MouseSpeed") as string;
                        var t1 = key.GetValue("MouseThreshold1") as string;
                        return speed == "0" && t1 == "0";
                    }
                }
            }
            catch {}
            return false;
        }

        public static bool SetMouseAccelerationDisabled(bool disable)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(MouseKey))
                {
                    if (key == null) return false;

                    if (disable)
                    {
                        LogEngine.Log("Desativando aceleração de mouse do Windows (Input 1:1 Direto sem curva de suavização)...", "info");
                        key.SetValue("MouseSpeed", "0", RegistryValueKind.String);
                        key.SetValue("MouseThreshold1", "0", RegistryValueKind.String);
                        key.SetValue("MouseThreshold2", "0", RegistryValueKind.String);

                        // Curva linear neutra
                        byte[] linearCurve = new byte[] {
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x15, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x45, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x60, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                        };
                        key.SetValue("SmoothMouseXCurve", linearCurve, RegistryValueKind.Binary);
                        key.SetValue("SmoothMouseYCurve", linearCurve, RegistryValueKind.Binary);

                        LogEngine.Log("Aceleração de mouse desativada (Sensibilidade direta 1:1 em jogos).", "success");
                    }
                    else
                    {
                        LogEngine.Log("Restaurando curvas padrão de aceleração de mouse...", "info");
                        key.SetValue("MouseSpeed", "1", RegistryValueKind.String);
                        key.SetValue("MouseThreshold1", "6", RegistryValueKind.String);
                        key.SetValue("MouseThreshold2", "10", RegistryValueKind.String);
                        LogEngine.Log("Curvas de mouse restauradas.", "info");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LogEngine.Log("Erro ao ajustar aceleração de mouse: " + ex.Message, "danger");
                return false;
            }
        }

        public static void ApplyGamerPreset(bool enable)
        {
            LogEngine.Log("Aplicando conjunto completo de otimizações para Jogos e Baixa Latência...", "info");
            SetNetworkThrottlingDisabled(enable);
            SetMouseAccelerationDisabled(enable);
        }
    }
}
