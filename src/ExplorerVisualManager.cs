using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace DebloaterPC
{
    public static class ExplorerVisualManager
    {
        private const string AdvKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        private const string TaskbarDevKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarDeveloperSettings";
        private const string CrashControlKey = @"SYSTEM\CurrentControlSet\Control\CrashControl";
        private const string PoliciesSystemKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System";
        private const string GalleryGuid = "{e88865ea-0e1c-4e20-9aa6-ed353b740269}";
        private const string HomeGuid = "{f874310e-b6b7-47dc-bc84-b9e6b38f5903}";

        public static bool SetTaskbarEndTask(bool enable)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(TaskbarDevKey))
                {
                    if (key != null)
                    {
                        key.SetValue("TaskbarEndTask", enable ? 1 : 0, RegistryValueKind.DWord);
                        LogEngine.Log(string.Format("Opção 'Finalizar Tarefa' no botão direito da barra de tarefas: {0}.", enable ? "HABILITADA" : "DESABILITADA"), "success");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogEngine.Log("Erro ao alterar TaskbarEndTask: " + ex.Message, "danger");
            }
            return false;
        }

        public static bool SetTaskbarAlignment(int align)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(AdvKey))
                {
                    if (key != null)
                    {
                        key.SetValue("TaskbarAl", align, RegistryValueKind.DWord);
                        LogEngine.Log(string.Format("Alinhamento da Barra de Tarefas definido para: {0}.", align == 0 ? "ESQUERDA" : "CENTRO"), "success");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogEngine.Log("Erro ao alterar alinhamento da barra de tarefas: " + ex.Message, "danger");
            }
            return false;
        }

        public static bool SetNumLockOnBoot(bool enable)
        {
            try
            {
                string val = enable ? "2" : "0";
                using (var key = Registry.Users.CreateSubKey(@".DEFAULT\Control Panel\Keyboard"))
                {
                    if (key != null) key.SetValue("InitialKeyboardIndicators", val, RegistryValueKind.String);
                }
                using (var key = Registry.CurrentUser.CreateSubKey(@"Control Panel\Keyboard"))
                {
                    if (key != null) key.SetValue("InitialKeyboardIndicators", val, RegistryValueKind.String);
                }
                LogEngine.Log(string.Format("NumLock automático ao ligar/reiniciar o computador: {0}.", enable ? "ATIVADO" : "DESATIVADO"), "success");
                return true;
            }
            catch (Exception ex)
            {
                LogEngine.Log("Erro ao alterar NumLock de boot: " + ex.Message, "danger");
                return false;
            }
        }

        public static bool SetDetailedBsod(bool enable)
        {
            try
            {
                using (var key = Registry.LocalMachine.CreateSubKey(CrashControlKey))
                {
                    if (key != null)
                    {
                        key.SetValue("DisplayParameters", enable ? 1 : 0, RegistryValueKind.DWord);
                        LogEngine.Log(string.Format("Tela Azul com Informações Técnicas Detalhadas: {0}.", enable ? "ATIVADA" : "PADRÃO"), "success");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogEngine.Log("Erro ao alterar CrashControl: " + ex.Message, "danger");
            }
            return false;
        }

        public static bool SetVerboseLogon(bool enable)
        {
            try
            {
                using (var key = Registry.LocalMachine.CreateSubKey(PoliciesSystemKey))
                {
                    if (key != null)
                    {
                        key.SetValue("VerboseStatus", enable ? 1 : 0, RegistryValueKind.DWord);
                        LogEngine.Log(string.Format("Mensagens de Inicialização Detalhadas (Verbose Logon): {0}.", enable ? "ATIVADAS" : "DESATIVADAS"), "success");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogEngine.Log("Erro ao alterar VerboseStatus: " + ex.Message, "danger");
            }
            return false;
        }

        public static bool ToggleGalleryAndHome(bool hide)
        {
            try
            {
                string nsPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\";
                if (hide)
                {
                    LogEngine.Log("Ocultando 'Galeria' e 'Início' da barra lateral do Explorador de Arquivos...", "info");
                    try { Registry.LocalMachine.DeleteSubKeyTree(nsPath + GalleryGuid, false); } catch {}
                    try { Registry.LocalMachine.DeleteSubKeyTree(nsPath + HomeGuid, false); } catch {}
                    LogEngine.Log("Galeria e Início ocultados da navegação do Explorer.", "success");
                }
                else
                {
                    LogEngine.Log("Restaurando 'Galeria' e 'Início' no Explorador de Arquivos...", "info");
                    try { Registry.LocalMachine.CreateSubKey(nsPath + GalleryGuid); } catch {}
                    try { Registry.LocalMachine.CreateSubKey(nsPath + HomeGuid); } catch {}
                    LogEngine.Log("Galeria e Início restaurados.", "info");
                }
                return true;
            }
            catch (Exception ex)
            {
                LogEngine.Log("Erro ao alterar Galeria/Início: " + ex.Message, "danger");
                return false;
            }
        }

        public static bool HideChatAndTaskView(bool hide)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(AdvKey))
                {
                    if (key != null)
                    {
                        int val = hide ? 0 : 1;
                        key.SetValue("TaskbarMn", val, RegistryValueKind.DWord); // Chat
                        key.SetValue("ShowTaskViewButton", val, RegistryValueKind.DWord); // Task View
                        LogEngine.Log(string.Format("Ícones de Chat e Visão de Tarefas: {0}.", hide ? "OCULTADOS" : "VISÍVEIS"), "success");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogEngine.Log("Erro ao alterar Chat e Visão de Tarefas: " + ex.Message, "danger");
            }
            return false;
        }

        public static bool DisableStartRecommendations(bool disable)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(AdvKey))
                {
                    if (key != null)
                    {
                        int val = disable ? 0 : 1;
                        key.SetValue("Start_TrackProgs", val, RegistryValueKind.DWord);
                    }
                }
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\Explorer"))
                {
                    if (key != null)
                    {
                        key.SetValue("HideRecentlyAddedApps", disable ? 1 : 0, RegistryValueKind.DWord);
                    }
                }
                LogEngine.Log(string.Format("Recomendações e Aplicativos Recentes no Menu Iniciar: {0}.", disable ? "DESATIVADOS" : "ATIVADOS"), "success");
                return true;
            }
            catch (Exception ex)
            {
                LogEngine.Log("Erro ao configurar recomendações do Menu Iniciar: " + ex.Message, "danger");
                return false;
            }
        }

        public static bool SetShowFileExtensions(bool show)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(AdvKey))
                {
                    if (key != null)
                    {
                        key.SetValue("HideFileExt", show ? 0 : 1, RegistryValueKind.DWord);
                        LogEngine.Log(string.Format("Exibição de extensões de arquivo: {0}.", show ? "ATIVADA" : "DESATIVADA"), "success");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogEngine.Log("Erro ao alterar HideFileExt: " + ex.Message, "danger");
            }
            return false;
        }

        public static bool SetShowHiddenFiles(bool show)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(AdvKey))
                {
                    if (key != null)
                    {
                        key.SetValue("Hidden", show ? 1 : 2, RegistryValueKind.DWord);
                        LogEngine.Log(string.Format("Exibição de arquivos ocultos: {0}.", show ? "ATIVADA" : "DESATIVADA"), "success");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogEngine.Log("Erro ao alterar Hidden: " + ex.Message, "danger");
            }
            return false;
        }

        public static bool SetLaunchToThisPC(bool enable)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(AdvKey))
                {
                    if (key != null)
                    {
                        // LaunchTo: 1 = Este Computador, 2 = Acesso Rápido
                        key.SetValue("LaunchTo", enable ? 1 : 2, RegistryValueKind.DWord);
                        LogEngine.Log(string.Format("Explorador de Arquivos configurado para abrir em: {0}.", enable ? "ESTE COMPUTADOR" : "ACESSO RÁPIDO"), "success");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogEngine.Log("Erro ao alterar LaunchTo: " + ex.Message, "danger");
            }
            return false;
        }

        public static void RemoveFaxAndXpsPrinters()
        {
            try
            {
                LogEngine.Log("Removendo impressoras legadas não utilizadas (Fax e Microsoft XPS Document Writer)...", "info");

                RunProcess("rundll32.exe", "printui.dll,PrintUIEntry /dl /n \"Fax\"");
                RunProcess("rundll32.exe", "printui.dll,PrintUIEntry /dl /n \"Microsoft XPS Document Writer\"");

                // Fallback via powershell Remove-Printer
                string psScript = "Get-Printer | Where-Object { $_.Name -in @('Fax', 'Microsoft XPS Document Writer') } | Remove-Printer -ErrorAction SilentlyContinue";
                var psi = new ProcessStartInfo("powershell.exe", "-NoProfile -NonInteractive -Command \"" + psScript + "\"")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using (var p = Process.Start(psi))
                {
                    p.WaitForExit(4000);
                }

                LogEngine.Log("Filas de impressão do Fax e XPS Writer removidas com sucesso.", "success");
            }
            catch (Exception ex)
            {
                LogEngine.Log("Erro ao remover impressoras legadas: " + ex.Message, "danger");
            }
        }

        public static void RestartExplorer()
        {
            try
            {
                LogEngine.Log("Reiniciando processo Windows Explorer para aplicar alterações visuais...", "info");
                foreach (var p in Process.GetProcessesByName("explorer"))
                {
                    try { p.Kill(); } catch {}
                }
                Process.Start("explorer.exe");
                LogEngine.Log("Windows Explorer reiniciado com sucesso.", "success");
            }
            catch (Exception ex)
            {
                LogEngine.Log("Erro ao reiniciar Explorer: " + ex.Message, "danger");
            }
        }

        private static void RunProcess(string file, string args)
        {
            try
            {
                var psi = new ProcessStartInfo(file, args)
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
