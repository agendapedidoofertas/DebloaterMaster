using System;
using Microsoft.Win32;
using System.Diagnostics;

namespace DebloaterPC
{
    public static class RegistryManager
    {
        public static void ApplyPrivacyTweaks(bool apply)
        {
            try
            {
                LogEngine.Log(string.Format("Aplicando políticas de privacidade e bloqueio de telemetria ({0})...", apply ? "BLOQUEAR" : "REVERTER"), "info");

                // 1. Telemetria Diagnóstica (AllowTelemetry = 0)
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\DataCollection"))
                {
                    if (key != null)
                    {
                        key.SetValue("AllowTelemetry", apply ? 0 : 1, RegistryValueKind.DWord);
                        key.SetValue("DoNotShowFeedbackNotifications", apply ? 1 : 0, RegistryValueKind.DWord);
                    }
                }

                // 2. Histórico de Atividades (ActivityFeed)
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\System"))
                {
                    if (key != null)
                    {
                        key.SetValue("EnableActivityFeed", apply ? 0 : 1, RegistryValueKind.DWord);
                    }
                }

                // 3. Xbox GameDVR
                using (var key = Registry.CurrentUser.CreateSubKey(@"System\GameConfigStore"))
                {
                    if (key != null)
                    {
                        key.SetValue("GameDVR_Enabled", apply ? 0 : 1, RegistryValueKind.DWord);
                    }
                }

                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\GameDVR"))
                {
                    if (key != null)
                    {
                        key.SetValue("AllowGameDVR", apply ? 0 : 1, RegistryValueKind.DWord);
                    }
                }

                // Aplica Otimizações da Barra de Pesquisa, Apps de Segundo Plano e Bloqueio de Anúncios/Rastreio
                OptimizeSearchBar(apply);
                DisableAdsAndTracking(apply);
                ApplyBackgroundAppsPolicy(apply);

                LogEngine.Log("Políticas de privacidade, barra de pesquisa, segundo plano e anúncios configuradas com sucesso.", "success");
            }
            catch (Exception ex)
            {
                LogEngine.Log("Erro ao aplicar chaves de privacidade: " + ex.Message, "danger");
            }
        }

        public static void ApplyBackgroundAppsPolicy(bool disable)
        {
            try
            {
                LogEngine.Log(string.Format("Configurando execução de aplicativos em segundo plano: {0}...", disable ? "BLOQUEAR (Poupar RAM)" : "PADRÃO"), "info");

                // LetAppsRunInBackground: 2 = Force Deny (não permite apps rodarem consumindo RAM em 2º plano)
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy"))
                {
                    if (key != null)
                    {
                        key.SetValue("LetAppsRunInBackground", disable ? 2 : 0, RegistryValueKind.DWord);
                    }
                }

                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications"))
                {
                    if (key != null)
                    {
                        key.SetValue("GlobalUserDisabled", disable ? 1 : 0, RegistryValueKind.DWord);
                        key.SetValue("Migrated", 4, RegistryValueKind.DWord);
                    }
                }

                LogEngine.Log("Apps em segundo plano configurados para poupar memória RAM e ciclos de CPU.", "success");
            }
            catch (Exception ex)
            {
                LogEngine.Log("Aviso segundo plano: " + ex.Message, "warning");
            }
        }

        public static void OptimizeSearchBar(bool optimize)
        {
            try
            {
                LogEngine.Log("Otimizando Barra de Pesquisa do Windows (desativando Bing, IA e destaques na web)...", "info");

                // 1. Pesquisa Web e Bing no Menu Iniciar / Barra de Tarefas
                try
                {
                    using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Policies\Microsoft\Windows\Explorer"))
                    {
                        if (key != null)
                        {
                            key.SetValue("DisableSearchBoxSuggestions", optimize ? 1 : 0, RegistryValueKind.DWord);
                        }
                    }
                }
                catch {}

                try
                {
                    using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Search"))
                    {
                        if (key != null)
                        {
                            key.SetValue("BingSearchEnabled", optimize ? 0 : 1, RegistryValueKind.DWord);
                            key.SetValue("CortanaConsent", optimize ? 0 : 1, RegistryValueKind.DWord);
                        }
                    }
                }
                catch {}

                // 2. Windows Search Group Policy
                try
                {
                    using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Windows Search"))
                    {
                        if (key != null)
                        {
                            key.SetValue("DisableWebSearch", optimize ? 1 : 0, RegistryValueKind.DWord);
                            key.SetValue("ConnectedSearchUseWeb", optimize ? 0 : 1, RegistryValueKind.DWord);
                            key.SetValue("AllowCloudSearch", optimize ? 0 : 1, RegistryValueKind.DWord);
                            key.SetValue("EnableDynamicContentInWSB", optimize ? 0 : 1, RegistryValueKind.DWord); // Search highlights off
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    LogEngine.Log("Aviso: Configurações de pesquisa de máquina (HKLM) requerem permissões de Administrador.", "warning");
                }

                // 3. SearchSettings (Destaques e histórico local)
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\SearchSettings"))
                {
                    if (key != null)
                    {
                        key.SetValue("IsDynamicSearchBoxEnabled", optimize ? 0 : 1, RegistryValueKind.DWord);
                        key.SetValue("IsDeviceSearchHistoryEnabled", optimize ? 0 : 1, RegistryValueKind.DWord);
                    }
                }

                LogEngine.Log("Barra de pesquisa otimizada: busca local ultrarrápida, sem consumo de rede ou anúncios.", "success");
            }
            catch (Exception ex)
            {
                LogEngine.Log("Aviso barra de pesquisa: " + ex.Message, "warning");
            }
        }

        public static void DisableAdsAndTracking(bool disable)
        {
            try
            {
                LogEngine.Log("Desativando ID de Anúncio, rastreamento de apps e sugestões patrocinadas...", "info");

                // 1. Advertising ID (ID de Anúncios)
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo"))
                {
                    if (key != null) key.SetValue("DisabledByGroupPolicy", disable ? 1 : 0, RegistryValueKind.DWord);
                }

                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo"))
                {
                    if (key != null) key.SetValue("Enabled", disable ? 0 : 1, RegistryValueKind.DWord);
                }

                // 2. Rastreamento de inicialização de aplicativos
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"))
                {
                    if (key != null) key.SetValue("Start_TrackProgs", disable ? 0 : 1, RegistryValueKind.DWord);
                }

                // 3. Sugestões e Anúncios do ContentDeliveryManager
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"))
                {
                    if (key != null)
                    {
                        key.SetValue("SubscribedContent-338388Enabled", disable ? 0 : 1, RegistryValueKind.DWord);
                        key.SetValue("SubscribedContent-338389Enabled", disable ? 0 : 1, RegistryValueKind.DWord);
                        key.SetValue("SubscribedContent-310093Enabled", disable ? 0 : 1, RegistryValueKind.DWord); // Anúncios no Iniciar
                        key.SetValue("SubscribedContent-353694Enabled", disable ? 0 : 1, RegistryValueKind.DWord);
                        key.SetValue("SubscribedContent-353696Enabled", disable ? 0 : 1, RegistryValueKind.DWord);
                        key.SetValue("SystemPaneSuggestionsEnabled", disable ? 0 : 1, RegistryValueKind.DWord);
                        key.SetValue("SilentInstalledAppsEnabled", disable ? 0 : 1, RegistryValueKind.DWord);
                        key.SetValue("SoftLandingEnabled", disable ? 0 : 1, RegistryValueKind.DWord);
                    }
                }

                // 4. Consumer Features & Experiências personalizadas
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\CloudContent"))
                {
                    if (key != null) key.SetValue("DisableWindowsConsumerFeatures", disable ? 1 : 0, RegistryValueKind.DWord);
                }

                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Policies\Microsoft\Windows\CloudContent"))
                {
                    if (key != null) key.SetValue("DisableTailoredExperiencesWithDiagnosticData", disable ? 1 : 0, RegistryValueKind.DWord);
                }

                // 5. Rastreamento de digitação e escrita (Typing Insights)
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Input\Settings"))
                {
                    if (key != null) key.SetValue("InsightsEnabled", disable ? 0 : 1, RegistryValueKind.DWord);
                }

                // 6. Relatórios de erro em segundo plano (Windows Error Reporting)
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting"))
                {
                    if (key != null) key.SetValue("Disabled", disable ? 1 : 0, RegistryValueKind.DWord);
                }

                LogEngine.Log("Rastreamento de anúncios, sugestões e telemetria de digitação desativados com sucesso.", "success");
            }
            catch (Exception ex)
            {
                LogEngine.Log("Aviso anúncios e rastreio: " + ex.Message, "warning");
            }
        }

        public static void SetClassicContextMenu(bool enable)
        {
            try
            {
                string clsidKey = @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}";
                if (enable)
                {
                    LogEngine.Log("Restaurando Menu de Contexto Clássico do Windows 10...", "info");
                    using (var key = Registry.CurrentUser.CreateSubKey(clsidKey + @"\InprocServer32"))
                    {
                        if (key != null)
                        {
                            key.SetValue("", ""); // Default empty string activates classic menu
                        }
                    }
                    LogEngine.Log("Menu de contexto clássico ativado (sem atraso de animação).", "success");
                }
                else
                {
                    LogEngine.Log("Revertendo para o menu de contexto moderno do Windows 11...", "info");
                    try
                    {
                        Registry.CurrentUser.DeleteSubKeyTree(clsidKey, false);
                        LogEngine.Log("Menu do Windows 11 restaurado.", "info");
                    }
                    catch {}
                }
            }
            catch (Exception ex)
            {
                LogEngine.Log("Aviso menu clássico: " + ex.Message, "warning");
            }
        }

        public static void OptimizeExplorerVisuals(bool disableTransparency, bool hideWidgetsAndCopilot, bool speedMenu)
        {
            try
            {
                LogEngine.Log("Ajustando configurações visuais do Windows Explorer...", "info");

                // Transparência
                if (disableTransparency)
                {
                    using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                    {
                        if (key != null) key.SetValue("EnableTransparency", 0, RegistryValueKind.DWord);
                    }
                }

                // Widgets (TaskbarDa) e Copilot (ShowCopilotButton)
                if (hideWidgetsAndCopilot)
                {
                    using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"))
                    {
                        if (key != null)
                        {
                            key.SetValue("TaskbarDa", 0, RegistryValueKind.DWord);
                            key.SetValue("ShowCopilotButton", 0, RegistryValueKind.DWord);
                        }
                    }
                }

                // Menu delay
                if (speedMenu)
                {
                    using (var key = Registry.CurrentUser.CreateSubKey(@"Control Panel\Desktop"))
                    {
                        if (key != null) key.SetValue("MenuShowDelay", "50", RegistryValueKind.String);
                    }

                    using (var key = Registry.CurrentUser.CreateSubKey(@"Control Panel\Desktop\WindowMetrics"))
                    {
                        if (key != null) key.SetValue("MinAnimate", "0", RegistryValueKind.String);
                    }
                }

                LogEngine.Log("Otimizações do Windows Explorer aplicadas com sucesso.", "success");
            }
            catch (Exception ex)
            {
                LogEngine.Log("Aviso ao configurar Explorer: " + ex.Message, "warning");
            }
        }

        public static void FlushDns()
        {
            try
            {
                LogEngine.Log("Limpando cache do resolvedor DNS (ipconfig /flushdns)...", "info");
                var psi = new ProcessStartInfo("ipconfig.exe", "/flushdns")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using (var p = Process.Start(psi))
                {
                    p.WaitForExit(3000);
                    LogEngine.Log("Cache DNS esvaziado com sucesso.", "success");
                }
            }
            catch (Exception ex)
            {
                LogEngine.Log("Aviso ipconfig: " + ex.Message, "warning");
            }
        }

        public static void RestartExplorer()
        {
            try
            {
                LogEngine.Log("Reiniciando o Windows Explorer para aplicar as novas configurações visuais...", "info");
                foreach (var p in Process.GetProcessesByName("explorer"))
                {
                    try { p.Kill(); p.WaitForExit(2000); } catch {}
                }
                Process.Start("explorer.exe");
                LogEngine.Log("Windows Explorer reiniciado com sucesso.", "success");
            }
            catch (Exception ex)
            {
                LogEngine.Log("Aviso ao reiniciar explorer: " + ex.Message, "warning");
            }
        }
    }
}
