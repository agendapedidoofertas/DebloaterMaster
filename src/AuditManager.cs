using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceProcess;
using Microsoft.Win32;

namespace DebloaterPC
{
    public static class AuditManager
    {
        public static Dictionary<string, bool> GetAuditState()
        {
            var state = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

            // 1. Memória & CPU
            state["tweak_disable_mmagent"] = !Diagnostics.CheckMemoryCompressionStatus();
            state["tweak_disable_diagtrack"] = IsServiceDisabled("DiagTrack");
            state["tweak_disable_sysmain"] = IsServiceDisabled("SysMain");
            state["tweak_high_performance"] = CheckRegValue(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes", "ActivePowerScheme", "8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");

            // 2. Armazenamento & SSD
            state["tweak_disable_hibernation"] = !File.Exists(@"C:\hiberfil.sys");
            state["tweak_enable_trim"] = Diagnostics.CheckTrimStatus();
            state["tweak_disable_8dot3"] = Check8Dot3Disabled();

            // 3. Privacidade & Telemetria
            state["tweak_block_telemetry"] = CheckRegDword(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0);
            state["tweak_disable_background_apps"] = CheckRegDword(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsRunInBackground", 2);
            state["tweak_optimize_search"] = CheckRegDword(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "DisableWebSearch", 1);
            state["tweak_disable_ads_tracking"] = CheckRegDword(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo", "DisabledByGroupPolicy", 1);
            state["tweak_disable_activity_feed"] = CheckRegDword(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "EnableActivityFeed", 0);
            state["tweak_disable_gamedvr"] = CheckRegDword(Registry.CurrentUser, @"System\GameConfigStore", "GameDVR_Enabled", 0);

            // 4. Interface & Explorer
            state["tweak_classic_context_menu"] = KeyExists(Registry.CurrentUser, @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32");
            state["tweak_hide_copilot_widgets"] = CheckRegDword(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarDa", 0) &&
                                                  CheckRegDword(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowCopilotButton", 0);
            state["tweak_disable_transparency"] = CheckRegDword(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", 0);
            state["tweak_taskbar_end_task"] = CheckRegDword(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarDeveloperSettings", "TaskbarEndTask", 1);
            state["tweak_taskbar_align_left"] = CheckRegDword(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAl", 0);
            state["tweak_numlock_boot"] = CheckRegValue(Registry.Users, @".DEFAULT\Control Panel\Keyboard", "InitialKeyboardIndicators", "2");
            state["tweak_bsod_details"] = CheckRegDword(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\CrashControl", "DisplayParameters", 1);
            state["tweak_verbose_logon"] = CheckRegDword(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "VerboseStatus", 1);
            state["tweak_hide_gallery_home"] = !KeyExists(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{e88865ea-0e1c-4e20-9aa6-ed353b740269}");

            // 5. Navegadores
            state["tweak_browser_no_background"] = CheckRegDword(Registry.LocalMachine, @"SOFTWARE\Policies\Google\Chrome", "BackgroundModeEnabled", 0);
            state["tweak_browser_memory_saver"] = CheckRegDword(Registry.LocalMachine, @"SOFTWARE\Policies\Google\Chrome", "HighEfficiencyModeEnabled", 1);

            // 6. Rede, Hosts & Windows Update
            state["tweak_network_hosts_block"] = NetworkBlockManager.IsHostsBlocked();
            state["tweak_windows_update_security"] = WindowsUpdateManager.IsSecurityModeActive();
            state["tweak_delivery_optimization_p2p"] = CheckRegDword(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization", "DODownloadMode", 0);

            // 7. Gamer & Latência
            state["tweak_gamer_network_throttling"] = GamerOptimizer.IsNetworkThrottlingDisabled();
            state["tweak_gamer_mouse_1to1"] = GamerOptimizer.IsMouseAccelerationDisabled();

            // 8. Windows 11 Moderno & Tarefas
            state["tweak_w11_labconfig_bypass"] = Windows11ModernManager.IsLabConfigActive();
            state["tweak_w11_recall_disable"] = Windows11ModernManager.IsRecallDisabled();
            state["tweak_tasks_appraiser"] = ScheduledTasksManager.IsAppraiserDisabled();

            return state;
        }

        public static bool ToggleSingleTweak(string tweakId, bool enable)
        {
            try
            {
                LogEngine.Log(string.Format("Alterando ajuste individual '{0}' para: {1}", tweakId, enable ? "ATIVADO" : "DESATIVADO"), "info");

                switch (tweakId.ToLowerInvariant())
                {
                    case "tweak_disable_mmagent":
                        MemoryManager.ToggleMemoryCompression(enable);
                        break;
                    case "tweak_disable_diagtrack":
                        if (enable) MemoryManager.ConfigureServices(true, false);
                        break;
                    case "tweak_disable_sysmain":
                        if (enable) MemoryManager.ConfigureServices(false, true);
                        break;
                    case "tweak_high_performance":
                        if (enable) MemoryManager.SetHighPerformancePlan();
                        break;
                    case "tweak_clean_ram":
                        MemoryManager.FlushWorkingSets();
                        break;
                    case "tweak_clean_temps":
                        DiskManager.DeepCleanTemps();
                        break;
                    case "tweak_disable_supervision":
                        SupervisionManager.DisableAllSupervisionAndBackground();
                        break;
                    case "tweak_kill_supervision":
                        SupervisionManager.KillRunningSupervisionProcesses();
                        break;
                    case "tweak_disable_hibernation":
                        DiskManager.DisableHibernation(enable);
                        break;
                    case "tweak_enable_trim":
                        if (enable) DiskManager.EnsureTrimEnabled();
                        break;
                    case "tweak_disable_8dot3":
                        DiskManager.Disable8Dot3Names(enable);
                        break;
                    case "tweak_block_telemetry":
                        RegistryManager.ApplyPrivacyTweaks(enable);
                        break;
                    case "tweak_disable_background_apps":
                        RegistryManager.ApplyBackgroundAppsPolicy(enable);
                        break;
                    case "tweak_optimize_search":
                        RegistryManager.OptimizeSearchBar(enable);
                        break;
                    case "tweak_disable_ads_tracking":
                        RegistryManager.DisableAdsAndTracking(enable);
                        break;
                    case "tweak_disable_activity_feed":
                        try
                        {
                            using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\System"))
                            {
                                if (key != null) key.SetValue("EnableActivityFeed", enable ? 0 : 1, RegistryValueKind.DWord);
                            }
                        }
                        catch (UnauthorizedAccessException)
                        {
                            LogEngine.Log("Aviso: Alterar Linha do Tempo requer privilégios de Administrador.", "warning");
                        }
                        break;
                    case "tweak_disable_gamedvr":
                        try
                        {
                            using (var key = Registry.CurrentUser.CreateSubKey(@"System\GameConfigStore"))
                            {
                                if (key != null) key.SetValue("GameDVR_Enabled", enable ? 0 : 1, RegistryValueKind.DWord);
                            }
                        }
                        catch {}
                        try
                        {
                            using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\GameDVR"))
                            {
                                if (key != null) key.SetValue("AllowGameDVR", enable ? 0 : 1, RegistryValueKind.DWord);
                            }
                        }
                        catch (UnauthorizedAccessException)
                        {
                            LogEngine.Log("Aviso: Política AllowGameDVR requer privilégios de Administrador.", "warning");
                        }
                        break;
                    case "tweak_classic_context_menu":
                        RegistryManager.SetClassicContextMenu(enable);
                        break;
                    case "tweak_hide_copilot_widgets":
                        using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"))
                        {
                            if (key != null)
                            {
                                key.SetValue("TaskbarDa", enable ? 0 : 1, RegistryValueKind.DWord);
                                key.SetValue("ShowCopilotButton", enable ? 0 : 1, RegistryValueKind.DWord);
                            }
                        }
                        break;
                    case "tweak_disable_transparency":
                        using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                        {
                            if (key != null) key.SetValue("EnableTransparency", enable ? 0 : 1, RegistryValueKind.DWord);
                        }
                        break;
                    case "tweak_taskbar_end_task":
                        ExplorerVisualManager.SetTaskbarEndTask(enable);
                        break;
                    case "tweak_taskbar_align_left":
                        ExplorerVisualManager.SetTaskbarAlignment(enable ? 0 : 1);
                        break;
                    case "tweak_numlock_boot":
                        ExplorerVisualManager.SetNumLockOnBoot(enable);
                        break;
                    case "tweak_bsod_details":
                        ExplorerVisualManager.SetDetailedBsod(enable);
                        break;
                    case "tweak_verbose_logon":
                        ExplorerVisualManager.SetVerboseLogon(enable);
                        break;
                    case "tweak_hide_gallery_home":
                        ExplorerVisualManager.ToggleGalleryAndHome(!enable);
                        break;
                    case "tweak_browser_no_background":
                    case "tweak_browser_memory_saver":
                        ChromeOptimizer.ApplyBrowserPolicies(enable, enable, enable);
                        break;
                    case "tweak_network_hosts_block":
                        NetworkBlockManager.ToggleHostsBlock(enable);
                        break;
                    case "tweak_windows_update_security":
                        WindowsUpdateManager.SetSecurityMode(enable);
                        break;
                    case "tweak_delivery_optimization_p2p":
                        WindowsUpdateManager.SetDeliveryOptimizationP2P(!enable);
                        break;
                    case "tweak_gamer_network_throttling":
                        GamerOptimizer.SetNetworkThrottlingDisabled(enable);
                        break;
                    case "tweak_gamer_mouse_1to1":
                        GamerOptimizer.SetMouseAccelerationDisabled(enable);
                        break;
                    case "tweak_w11_labconfig_bypass":
                        Windows11ModernManager.SetLabConfigBypass(enable);
                        break;
                    case "tweak_w11_recall_disable":
                        Windows11ModernManager.SetRecallDisabled(enable);
                        break;
                    case "tweak_tasks_appraiser":
                        ScheduledTasksManager.ToggleCompatibilityAppraiser(!enable);
                        break;
                    default:
                        LogEngine.Log("Ajuste desconhecido: " + tweakId, "warning");
                        return false;
                }

                LogEngine.Log(string.Format("Ajuste '{0}' atualizado com sucesso.", tweakId), "success");
                return true;
            }
            catch (Exception ex)
            {
                LogEngine.Log(string.Format("Erro ao alterar {0}: {1}", tweakId, ex.Message), "danger");
                return false;
            }
        }

        private static bool CheckRegDword(RegistryKey root, string subKey, string valueName, int expected)
        {
            try
            {
                using (var key = root.OpenSubKey(subKey, false))
                {
                    if (key != null)
                    {
                        var val = key.GetValue(valueName);
                        if (val is int) return (int)val == expected;
                    }
                }
            }
            catch {}
            return false;
        }

        private static bool CheckRegValue(RegistryKey root, string subKey, string valueName, string expected)
        {
            try
            {
                using (var key = root.OpenSubKey(subKey, false))
                {
                    if (key != null)
                    {
                        var val = key.GetValue(valueName) as string;
                        if (val != null) return val.IndexOf(expected, StringComparison.OrdinalIgnoreCase) >= 0;
                    }
                }
            }
            catch {}
            return false;
        }

        private static bool KeyExists(RegistryKey root, string subKey)
        {
            try
            {
                using (var key = root.OpenSubKey(subKey, false))
                {
                    return key != null;
                }
            }
            catch {}
            return false;
        }

        private static bool IsServiceDisabled(string serviceName)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(string.Format(@"SYSTEM\CurrentControlSet\Services\{0}", serviceName), false))
                {
                    if (key != null)
                    {
                        var startVal = key.GetValue("Start");
                        if (startVal is int) return (int)startVal == 4; // 4 = Disabled
                    }
                }
            }
            catch {}
            return false;
        }

        private static bool Check8Dot3Disabled()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\FileSystem", false))
                {
                    if (key != null)
                    {
                        var val = key.GetValue("NtfsDisable8dot3NameCreation");
                        if (val is int) return (int)val == 1;
                    }
                }
            }
            catch {}
            return false;
        }
    }
}
