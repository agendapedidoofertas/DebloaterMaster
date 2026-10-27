using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace DebloaterPC
{
    public static class SupervisionManager
    {
        private static readonly string[] SupervisionProcesses = new string[]
        {
            "CompatTelRunner",
            "DeviceCensus",
            "GameBarPresenceWriter",
            "wermgr",
            "WerFault",
            "WerFaultSecure",
            "bcastdvr",
            "MicrosoftEdgeUpdate",
            "GoogleUpdate"
        };

        private static readonly string[] SupervisionTasks = new string[]
        {
            @"\Microsoft\Windows\Application Experience\Microsoft Compatibility Appraiser",
            @"\Microsoft\Windows\Application Experience\ProgramDataUpdater",
            @"\Microsoft\Windows\Application Experience\AitAgent",
            @"\Microsoft\Windows\Customer Experience Improvement Program\Consolidator",
            @"\Microsoft\Windows\Customer Experience Improvement Program\UsbCeip",
            @"\Microsoft\Windows\Customer Experience Improvement Program\BthSQM",
            @"\Microsoft\Windows\Device Information\Device",
            @"\Microsoft\Windows\DiskDiagnostic\Microsoft-Windows-DiskDiagnosticDataCollector",
            @"\Microsoft\Windows\Feedback\Siuf\DmClient",
            @"\Microsoft\Windows\Autochk\Proxy",
            @"\Microsoft\Windows\DiskFootprint\Diagnostics",
            @"\Microsoft\Windows\Maps\MapsUpdateTask"
        };

        public static void DisableAllSupervisionAndBackground()
        {
            LogEngine.Log("======================================================================", "info");
            LogEngine.Log("INICIANDO DESATIVAÇÃO DE PROGRAMAS DE SUPERVISÃO E SEGUNDO PLANO...", "info");
            LogEngine.Log("======================================================================", "info");

            // 1. Encerra processos de supervisão ativos imediatamente
            KillRunningSupervisionProcesses();

            // 2. Desativa tarefas agendadas de supervisão e censo
            DisableSupervisionTasks();

            // 3. Desativa serviços de supervisão e telemetria (WpcMonSvc, DiagTrack, WerSvc)
            DisableSupervisionServices();

            // 4. Bloqueia supervisão de digitação e escrita (Inking & Typing Keylogger)
            DisableTypingSupervision();

            // 5. Desativa censo de hardware e telemetria de inventário
            DisableDeviceCensusAndInventory();

            // 6. Bloqueia execução de aplicativos em segundo plano (Economiza CPU/RAM)
            DisableBackgroundAppsPolicy();

            // 7. Impede navegadores (Chrome/Edge) de rodarem em segundo plano
            DisableBrowserBackgroundSupervision();

            // 8. Desativa supervisão de Histórico de Atividades e Linha do Tempo
            DisableActivityTracking();

            // 9. Desativa telemetria de PowerShell e .NET
            DisableDevTelemetryVariables();

            LogEngine.Log("Todos os programas e serviços de supervisão de segundo plano foram neutralizados com sucesso!", "success");
        }

        public static void KillRunningSupervisionProcesses()
        {
            int killed = 0;
            foreach (var procName in SupervisionProcesses)
            {
                try
                {
                    var procs = Process.GetProcessesByName(procName);
                    foreach (var p in procs)
                    {
                        try
                        {
                            p.Kill();
                            p.WaitForExit(1000);
                            killed++;
                        }
                        catch {}
                    }
                }
                catch {}
            }
            if (killed > 0)
            {
                LogEngine.Log(string.Format("Encerrados {0} processos de supervisão/telemetria que rodavam em memória.", killed), "success");
            }
        }

        private static void DisableSupervisionTasks()
        {
            LogEngine.Log("Desativando tarefas agendadas de supervisão, censo e telemetria...", "info");
            foreach (var taskPath in SupervisionTasks)
            {
                try
                {
                    var psi = new ProcessStartInfo("schtasks.exe", string.Format("/change /tn \"{0}\" /disable", taskPath))
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    using (var p = Process.Start(psi))
                    {
                        p.WaitForExit(2000);
                    }
                }
                catch {}
            }
        }

        private static void DisableSupervisionServices()
        {
            LogEngine.Log("Parando e desativando serviços de supervisão do Windows...", "info");

            // Serviços: WpcMonSvc (Controle Parental), DiagTrack, diagsvc, WerSvc, dmwappushservice
            string[] services = new string[] { "WpcMonSvc", "DiagTrack", "diagsvc", "WerSvc", "dmwappushservice" };
            foreach (var s in services)
            {
                try
                {
                    var psiStop = new ProcessStartInfo("sc.exe", "stop " + s) { CreateNoWindow = true, UseShellExecute = false };
                    using (var p = Process.Start(psiStop)) p.WaitForExit(2000);

                    var psiConfig = new ProcessStartInfo("sc.exe", "config " + s + " start=disabled") { CreateNoWindow = true, UseShellExecute = false };
                    using (var p = Process.Start(psiConfig)) p.WaitForExit(2000);
                }
                catch {}
            }
        }

        private static void DisableTypingSupervision()
        {
            try
            {
                LogEngine.Log("Bloqueando supervisão de digitação e coleta de escrita (Inking/Typing)...", "info");

                // HKCU
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\InputPersonalization"))
                {
                    if (key != null)
                    {
                        key.SetValue("RestrictImplicitInkCollection", 1, RegistryValueKind.DWord);
                        key.SetValue("RestrictImplicitTextCollection", 1, RegistryValueKind.DWord);
                    }
                }

                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\InputPersonalization\TrainedDataStore"))
                {
                    if (key != null)
                    {
                        key.SetValue("HarvestContacts", 0, RegistryValueKind.DWord);
                    }
                }

                // HKLM GPO
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\InputPersonalization"))
                {
                    if (key != null)
                    {
                        key.SetValue("AllowInputPersonalization", 0, RegistryValueKind.DWord);
                        key.SetValue("RestrictImplicitInkCollection", 1, RegistryValueKind.DWord);
                        key.SetValue("RestrictImplicitTextCollection", 1, RegistryValueKind.DWord);
                    }
                }
            }
            catch (Exception ex)
            {
                LogEngine.Log("Aviso ao desativar supervisão de digitação: " + ex.Message, "warning");
            }
        }

        private static void DisableDeviceCensusAndInventory()
        {
            try
            {
                LogEngine.Log("Desativando Censo de Dispositivos (DeviceCensus) e inventário de aplicativos...", "info");

                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\DeviceCensus"))
                {
                    if (key != null) key.SetValue("DisableDeviceCensus", 1, RegistryValueKind.DWord);
                }

                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\AppCompat"))
                {
                    if (key != null)
                    {
                        key.SetValue("DisableInventory", 1, RegistryValueKind.DWord);
                        key.SetValue("AITEnable", 0, RegistryValueKind.DWord);
                    }
                }

                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\SQMClient\Windows"))
                {
                    if (key != null) key.SetValue("CEIPEnable", 0, RegistryValueKind.DWord);
                }

                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\Windows Error Reporting"))
                {
                    if (key != null)
                    {
                        key.SetValue("Disabled", 1, RegistryValueKind.DWord);
                        key.SetValue("DontSendAdditionalData", 1, RegistryValueKind.DWord);
                    }
                }
            }
            catch (Exception ex)
            {
                LogEngine.Log("Aviso ao desativar censo de dispositivos: " + ex.Message, "warning");
            }
        }

        private static void DisableBackgroundAppsPolicy()
        {
            try
            {
                LogEngine.Log("Bloqueando execução de aplicativos em segundo plano (LetAppsRunInBackground = Force Deny)...", "info");

                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy"))
                {
                    if (key != null) key.SetValue("LetAppsRunInBackground", 2, RegistryValueKind.DWord);
                }

                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications"))
                {
                    if (key != null)
                    {
                        key.SetValue("GlobalUserDisabled", 1, RegistryValueKind.DWord);
                        key.SetValue("Migrated", 4, RegistryValueKind.DWord);
                    }
                }
            }
            catch (Exception ex)
            {
                LogEngine.Log("Aviso ao desativar apps de segundo plano: " + ex.Message, "warning");
            }
        }

        private static void DisableBrowserBackgroundSupervision()
        {
            try
            {
                LogEngine.Log("Impedindo Google Chrome e Microsoft Edge de rodarem em segundo plano...", "info");

                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Google\Chrome"))
                {
                    if (key != null) key.SetValue("BackgroundModeEnabled", 0, RegistryValueKind.DWord);
                }

                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Edge"))
                {
                    if (key != null) key.SetValue("BackgroundModeEnabled", 0, RegistryValueKind.DWord);
                }
            }
            catch {}
        }

        private static void DisableActivityTracking()
        {
            try
            {
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\System"))
                {
                    if (key != null)
                    {
                        key.SetValue("EnableActivityFeed", 0, RegistryValueKind.DWord);
                        key.SetValue("PublishUserActivities", 0, RegistryValueKind.DWord);
                        key.SetValue("UploadUserActivities", 0, RegistryValueKind.DWord);
                    }
                }
            }
            catch {}
        }

        private static void DisableDevTelemetryVariables()
        {
            try
            {
                Environment.SetEnvironmentVariable("POWERSHELL_TELEMETRY_OPTOUT", "1", EnvironmentVariableTarget.Machine);
                Environment.SetEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1", EnvironmentVariableTarget.Machine);
                Environment.SetEnvironmentVariable("AZURE_CORE_COLLECT_TELEMETRY", "0", EnvironmentVariableTarget.Machine);
            }
            catch {}
        }
    }
}
