using System;
using System.IO;
using Microsoft.Win32;

namespace DebloaterPC
{
    public static class ChromeOptimizer
    {
        public static void ApplyBrowserPolicies(bool noBackground, bool memorySaver, bool noPrediction)
        {
            try
            {
                LogEngine.Log("Aplicando políticas corporativas de economia de memória no Chrome e Edge...", "info");

                // Chrome Policies
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Google\Chrome"))
                {
                    if (key != null)
                    {
                        if (noBackground)
                        {
                            key.SetValue("BackgroundModeEnabled", 0, RegistryValueKind.DWord);
                        }
                        if (memorySaver)
                        {
                            // Chrome Memory Saver (descarte de abas inativas)
                            key.SetValue("HighEfficiencyModeEnabled", 1, RegistryValueKind.DWord);
                        }
                        if (noPrediction)
                        {
                            // Bloqueio de pré-carregamento de abas
                            key.SetValue("NetworkPredictionOptions", 2, RegistryValueKind.DWord);
                        }
                    }
                }

                // Edge Policies
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Edge"))
                {
                    if (key != null)
                    {
                        if (noBackground)
                        {
                            key.SetValue("BackgroundModeEnabled", 0, RegistryValueKind.DWord);
                        }
                        if (memorySaver)
                        {
                            key.SetValue("EfficiencyModeEnabled", 1, RegistryValueKind.DWord);
                        }
                    }
                }

                LogEngine.Log("Políticas aplicadas: Segundo plano desativado ao fechar e Economizador de Memória ativo.", "success");
            }
            catch (Exception ex)
            {
                LogEngine.Log("Aviso ao aplicar políticas de navegador: " + ex.Message, "warning");
            }
        }

        public static void RestoreBrowserPolicies()
        {
            try
            {
                LogEngine.Log("Restaurando políticas do Chrome e Edge...", "info");
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Google\Chrome", true))
                {
                    if (key != null)
                    {
                        key.DeleteValue("BackgroundModeEnabled", false);
                        key.DeleteValue("HighEfficiencyModeEnabled", false);
                        key.DeleteValue("NetworkPredictionOptions", false);
                    }
                }
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Edge", true))
                {
                    if (key != null)
                    {
                        key.DeleteValue("BackgroundModeEnabled", false);
                        key.DeleteValue("EfficiencyModeEnabled", false);
                    }
                }
                LogEngine.Log("Políticas de navegador restauradas.", "success");
            }
            catch (Exception ex)
            {
                LogEngine.Log("Aviso ao restaurar políticas: " + ex.Message, "warning");
            }
        }

        public static void CleanBrowserCaches()
        {
            LogEngine.Log("Limpando caches de renderização e GPU do Chrome e Edge...", "info");
            long bytesFreed = 0;

            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            // Chrome targets
            string chromeUser = Path.Combine(localAppData, @"Google\Chrome\User Data\Default");
            bytesFreed += CleanDir(Path.Combine(chromeUser, "Cache"));
            bytesFreed += CleanDir(Path.Combine(chromeUser, "Code Cache"));
            bytesFreed += CleanDir(Path.Combine(chromeUser, "GPUCache"));

            // Edge targets
            string edgeUser = Path.Combine(localAppData, @"Microsoft\Edge\User Data\Default");
            bytesFreed += CleanDir(Path.Combine(edgeUser, "Cache"));
            bytesFreed += CleanDir(Path.Combine(edgeUser, "Code Cache"));
            bytesFreed += CleanDir(Path.Combine(edgeUser, "GPUCache"));

            double mbFreed = Math.Round((double)bytesFreed / (1024 * 1024), 1);
            LogEngine.Log(string.Format("Caches de navegadores limpos! {0} MB liberados.", mbFreed), "success");
        }

        private static long CleanDir(string path)
        {
            long freed = 0;
            if (!Directory.Exists(path)) return 0;
            try
            {
                var dir = new DirectoryInfo(path);
                foreach (var f in dir.GetFiles())
                {
                    try
                    {
                        long s = f.Length;
                        f.Delete();
                        freed += s;
                    }
                    catch {}
                }
            }
            catch {}
            return freed;
        }
    }
}
