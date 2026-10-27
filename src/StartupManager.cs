using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace DebloaterPC
{
    public class StartupItem
    {
        public string Name { get; set; }
        public string Command { get; set; }
        public string Location { get; set; }
        public bool Enabled { get; set; }
    }

    public static class StartupManager
    {
        public static List<StartupItem> GetStartupItems()
        {
            var list = new List<StartupItem>();

            // 1. HKCU Run
            ScanRegistryKey(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Run", "HKCU: Run", list);

            // 2. HKLM Run
            ScanRegistryKey(Registry.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\Run", "HKLM: Run", list);

            // 3. User Startup Folder
            try
            {
                string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                if (Directory.Exists(startupFolder))
                {
                    foreach (var file in Directory.GetFiles(startupFolder))
                    {
                        list.Add(new StartupItem
                        {
                            Name = Path.GetFileNameWithoutExtension(file),
                            Command = file,
                            Location = "Pasta Inicializar (Usuário)",
                            Enabled = !file.EndsWith(".disabled", StringComparison.OrdinalIgnoreCase)
                        });
                    }
                }
            }
            catch {}

            return list;
        }

        private static void ScanRegistryKey(RegistryKey root, string subKeyPath, string locationName, List<StartupItem> list)
        {
            try
            {
                using (var key = root.OpenSubKey(subKeyPath, false))
                {
                    if (key != null)
                    {
                        foreach (var valueName in key.GetValueNames())
                        {
                            var cmd = key.GetValue(valueName) as string ?? "";
                            list.Add(new StartupItem
                            {
                                Name = valueName,
                                Command = cmd,
                                Location = locationName,
                                Enabled = true
                            });
                        }
                    }
                }
            }
            catch {}
        }

        public static bool ToggleItem(string name, string location)
        {
            try
            {
                if (location.StartsWith("HKCU"))
                {
                    using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                    {
                        if (key != null)
                        {
                            key.DeleteValue(name, false);
                            LogEngine.Log(string.Format("Entrada de inicialização '{0}' removida do HKCU.", name), "success");
                            return true;
                        }
                    }
                }
                else if (location.StartsWith("HKLM"))
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                    {
                        if (key != null)
                        {
                            key.DeleteValue(name, false);
                            LogEngine.Log(string.Format("Entrada de inicialização '{0}' removida do HKLM.", name), "success");
                            return true;
                        }
                    }
                }
                else if (location.Contains("Pasta Inicializar"))
                {
                    string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                    string target = Path.Combine(startupFolder, name + ".lnk");
                    if (File.Exists(target))
                    {
                        File.Move(target, target + ".disabled");
                        LogEngine.Log(string.Format("Atalho de inicialização '{0}' desativado.", name), "success");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogEngine.Log(string.Format("Erro ao alterar item de inicialização {0}: {1}", name, ex.Message), "danger");
            }
            return false;
        }
    }
}
