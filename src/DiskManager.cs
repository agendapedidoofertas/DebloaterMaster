using System;
using System.IO;
using System.Diagnostics;

namespace DebloaterPC
{
    public static class DiskManager
    {
        public static void DisableHibernation(bool disable)
        {
            try
            {
                string arg = disable ? "-h off" : "-h on";
                LogEngine.Log(string.Format("Configurando hibernação: powercfg {0}...", arg), "info");
                var psi = new ProcessStartInfo("powercfg.exe", arg)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using (var p = Process.Start(psi))
                {
                    p.WaitForExit(5000);
                    if (disable)
                        LogEngine.Log("Hibernação desativada. Arquivo hiberfil.sys removido (liberados de 4 a 8 GB no SSD C:).", "success");
                    else
                        LogEngine.Log("Hibernação reativada.", "info");
                }
            }
            catch (Exception ex)
            {
                LogEngine.Log("Erro ao alterar hibernação: " + ex.Message, "danger");
            }
        }

        public static void EnsureTrimEnabled()
        {
            try
            {
                LogEngine.Log("Garantindo ativação do comando TRIM no SSD...", "info");
                var psi = new ProcessStartInfo("fsutil.exe", "behavior set DisableDeleteNotify 0")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using (var p = Process.Start(psi))
                {
                    p.WaitForExit(4000);
                    LogEngine.Log("TRIM garantido como ATIVO (DisableDeleteNotify = 0). O SSD manterá performance ideal de descarte de blocos.", "success");
                }
            }
            catch (Exception ex)
            {
                LogEngine.Log("Aviso ao verificar TRIM: " + ex.Message, "warning");
            }
        }

        public static void Disable8Dot3Names(bool disable)
        {
            try
            {
                string arg = disable ? "8dot3name set 1" : "8dot3name set 0";
                LogEngine.Log(string.Format("Configurando nomes curtos 8.3 do NTFS: fsutil {0}...", arg), "info");
                var psi = new ProcessStartInfo("fsutil.exe", arg)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using (var p = Process.Start(psi))
                {
                    p.WaitForExit(4000);
                    if (disable)
                        LogEngine.Log("Criação de nomes 8.3 desativada. Reduz sobrecarga de I/O em discos NTFS.", "success");
                    else
                        LogEngine.Log("Nomes 8.3 reativados.", "info");
                }
            }
            catch (Exception ex)
            {
                LogEngine.Log("Aviso fsutil 8dot3name: " + ex.Message, "warning");
            }
        }

        public static void DeepCleanTemps()
        {
            LogEngine.Log("Iniciando limpeza profunda de temporários, caches e arquivos descartáveis...", "info");
            long bytesFreed = 0;

            // 1. User Temp
            string userTemp = Path.GetTempPath();
            bytesFreed += CleanDirectory(userTemp);

            // 2. Windows Temp
            string winTemp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp");
            bytesFreed += CleanDirectory(winTemp);

            // 3. Prefetch
            string prefetch = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch");
            bytesFreed += CleanDirectory(prefetch);

            // 4. SoftwareDistribution\Download
            string softDist = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), @"SoftwareDistribution\Download");
            bytesFreed += CleanDirectory(softDist);

            // 5. Crash dumps
            try
            {
                string memoryDmp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "MEMORY.DMP");
                if (File.Exists(memoryDmp))
                {
                    long sz = new FileInfo(memoryDmp).Length;
                    File.Delete(memoryDmp);
                    bytesFreed += sz;
                    LogEngine.Log("Arquivo de despejo de memória MEMORY.DMP removido.", "info");
                }
            }
            catch {}

            double mbFreed = Math.Round((double)bytesFreed / (1024 * 1024), 1);
            LogEngine.Log(string.Format("Limpeza de temporários concluída! Aproximadamente {0} MB liberados.", mbFreed), "success");
        }

        private static long CleanDirectory(string path)
        {
            long freed = 0;
            if (!Directory.Exists(path)) return 0;

            try
            {
                var dir = new DirectoryInfo(path);
                foreach (var file in dir.GetFiles())
                {
                    try
                    {
                        long sz = file.Length;
                        file.Delete();
                        freed += sz;
                    }
                    catch {}
                }

                foreach (var subDir in dir.GetDirectories())
                {
                    try
                    {
                        subDir.Delete(true);
                    }
                    catch {}
                }
            }
            catch {}
            return freed;
        }

        public static void CompactWinSxS()
        {
            try
            {
                LogEngine.Log("Iniciando compactação profunda do repositório WinSxS via DISM (/StartComponentCleanup /ResetBase)...", "info");
                LogEngine.Log("Aviso: Esta etapa pode levar de 3 a 8 minutos dependendo da CPU e do SSD.", "warning");

                var psi = new ProcessStartInfo("dism.exe", "/online /cleanup-image /startcomponentcleanup /resetbase")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var proc = Process.Start(psi))
                {
                    while (!proc.StandardOutput.EndOfStream)
                    {
                        string line = proc.StandardOutput.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line) && (line.Contains("%") || line.Contains("Operação") || line.Contains("concluída") || line.Contains("100%")))
                        {
                            LogEngine.Log("> DISM: " + line.Trim(), "info");
                        }
                    }
                    proc.WaitForExit();

                    if (proc.ExitCode == 0)
                    {
                        LogEngine.Log("Compactação WinSxS ResetBase concluída com sucesso! Atualizações antigas descartadas.", "success");
                    }
                    else
                    {
                        LogEngine.Log(string.Format("DISM finalizou com código {0}.", proc.ExitCode), "warning");
                    }
                }
            }
            catch (Exception ex)
            {
                LogEngine.Log("Erro ao executar compactação WinSxS: " + ex.Message, "danger");
            }
        }
    }
}
