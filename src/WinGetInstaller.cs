using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DebloaterPC
{
    public class WinGetAppInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
    }

    public static class WinGetInstaller
    {
        public static readonly List<WinGetAppInfo> Catalog = new List<WinGetAppInfo>
        {
            // Navegadores
            new WinGetAppInfo { Id = "Google.Chrome", Name = "Google Chrome", Category = "Navegadores", Description = "Navegador web rápido e seguro da Google." },
            new WinGetAppInfo { Id = "Mozilla.Firefox", Name = "Mozilla Firefox", Category = "Navegadores", Description = "Navegador de código aberto focado em privacidade." },
            new WinGetAppInfo { Id = "Brave.Brave", Name = "Brave Browser", Category = "Navegadores", Description = "Navegador com bloqueador nativo de anúncios e rastreadores." },
            new WinGetAppInfo { Id = "Opera.OperaGX", Name = "Opera GX", Category = "Navegadores", Description = "Navegador voltado para gamers com limitadores de CPU e RAM." },
            new WinGetAppInfo { Id = "TorProject.TorBrowser", Name = "Tor Browser", Category = "Navegadores", Description = "Navegação totalmente anônima pela rede Tor com criptografia em camadas." },

            // Sistema & Otimização
            new WinGetAppInfo { Id = "7zip.7zip", Name = "7-Zip", Category = "Sistema & Compressão", Description = "Compactador e descompactador de arquivos ultrarrápido." },
            new WinGetAppInfo { Id = "Giorgiotani.PeaZip", Name = "PeaZip", Category = "Sistema & Compressão", Description = "Gerenciador de arquivos compactados moderno de código aberto." },
            new WinGetAppInfo { Id = "M2Team.NanaZip", Name = "NanaZip", Category = "Sistema & Compressão", Description = "Fork moderno do 7-Zip integrado ao menu de contexto do Windows 11." },
            new WinGetAppInfo { Id = "Microsoft.PowerToys", Name = "Microsoft PowerToys", Category = "Sistema & Otimização", Description = "Conjunto oficial da Microsoft de ferramentas e utilitários avançados." },
            new WinGetAppInfo { Id = "CrystalDewWorld.CrystalDiskInfo", Name = "CrystalDiskInfo", Category = "Sistema & Otimização", Description = "Monitoramento da saúde e temperatura de HDs e SSDs (S.M.A.R.T.)." },
            new WinGetAppInfo { Id = "CrystalDewWorld.CrystalDiskMark", Name = "CrystalDiskMark", Category = "Sistema & Otimização", Description = "Teste de velocidade de leitura e escrita para SSDs e unidades de armazenamento." },
            new WinGetAppInfo { Id = "CPUID.CPU-Z", Name = "CPU-Z", Category = "Sistema & Otimização", Description = "Diagnóstico detalhado de processador, placa-mãe e memória RAM." },
            new WinGetAppInfo { Id = "TechPowerUp.GPU-Z", Name = "GPU-Z", Category = "Sistema & Otimização", Description = "Informações completas e monitoramento de placas de vídeo NVIDIA, AMD e Intel." },
            new WinGetAppInfo { Id = "REALiX.HWiNFO", Name = "HWiNFO", Category = "Sistema & Otimização", Description = "Diagnóstico aprofundado de sensores, temperaturas e voltagens de hardware." },
            new WinGetAppInfo { Id = "WinsiderSeminars.SystemInformer", Name = "System Informer (Process Hacker)", Category = "Sistema & Otimização", Description = "Substituto avançado para o Gerenciador de Tarefas do Windows." },
            new WinGetAppInfo { Id = "RevoUninstaller.RevoUninstaller", Name = "Revo Uninstaller", Category = "Sistema & Otimização", Description = "Desinstalador completo que remove sobras de arquivos e registros órfãos." },

            // Desenvolvimento & Produtividade
            new WinGetAppInfo { Id = "Microsoft.VisualStudioCode", Name = "Visual Studio Code", Category = "Desenvolvimento", Description = "Editor de código leve, extensível e moderno." },
            new WinGetAppInfo { Id = "Notepad++.Notepad++", Name = "Notepad++", Category = "Desenvolvimento", Description = "Editor de texto e código extremamente leve com suporte a abas e macros." },
            new WinGetAppInfo { Id = "Git.Git", Name = "Git", Category = "Desenvolvimento", Description = "Sistema de controle de versão distribuído mais utilizado do mundo." },
            new WinGetAppInfo { Id = "Microsoft.WindowsTerminal", Name = "Windows Terminal", Category = "Desenvolvimento", Description = "Terminal moderno com suporte a abas, PowerShell e CMD." },
            new WinGetAppInfo { Id = "OpenJS.NodeJS.LTS", Name = "Node.js (LTS)", Category = "Desenvolvimento", Description = "Ambiente de execução JavaScript server-side estável." },
            new WinGetAppInfo { Id = "Python.Python.3.12", Name = "Python 3.12", Category = "Desenvolvimento", Description = "Linguagem de programação versátil para automações e desenvolvimento." },

            // Comunicação
            new WinGetAppInfo { Id = "Discord.Discord", Name = "Discord", Category = "Comunicação", Description = "Chat de voz, vídeo e texto para comunidades e jogos." },
            new WinGetAppInfo { Id = "Telegram.TelegramDesktop", Name = "Telegram Desktop", Category = "Comunicação", Description = "Aplicativo rápido e sincronizado de mensagens para desktop." },
            new WinGetAppInfo { Id = "WhatsApp.WhatsApp", Name = "WhatsApp Desktop", Category = "Comunicação", Description = "Mensageiro oficial do WhatsApp para computador." },
            new WinGetAppInfo { Id = "SlackTechnologies.Slack", Name = "Slack", Category = "Comunicação", Description = "Plataforma de colaboração em equipe e ambientes profissionais." },

            // Multimídia
            new WinGetAppInfo { Id = "VideoLAN.VLC", Name = "VLC Media Player", Category = "Multimídia", Description = "Reprodutor de áudio e vídeo universal compatível com todos os formatos." },
            new WinGetAppInfo { Id = "OBSProject.OBSStudio", Name = "OBS Studio", Category = "Multimídia", Description = "Software profissional de gravação de tela e transmissões ao vivo (live streaming)." },
            new WinGetAppInfo { Id = "Spotify.Spotify", Name = "Spotify", Category = "Multimídia", Description = "Serviço de streaming de músicas, álbuns e podcasts." },
            new WinGetAppInfo { Id = "Audacity.Audacity", Name = "Audacity", Category = "Multimídia", Description = "Editor e gravador de áudio multifaixas de código aberto." },
            new WinGetAppInfo { Id = "HandBrake.HandBrake", Name = "HandBrake", Category = "Multimídia", Description = "Conversor e compressor universal de vídeos em alta qualidade." },
            new WinGetAppInfo { Id = "GIMP.GIMP", Name = "GIMP", Category = "Multimídia", Description = "Editor de imagens avançado de código aberto (alternativa ao Photoshop)." },
            new WinGetAppInfo { Id = "BlenderFoundation.Blender", Name = "Blender", Category = "Multimídia", Description = "Suíte completa de modelagem 3D, animação, efeitos e renderização." },

            // Jogos & Plataformas
            new WinGetAppInfo { Id = "Valve.Steam", Name = "Steam", Category = "Jogos", Description = "Principal plataforma de distribuição digital de jogos para PC." },
            new WinGetAppInfo { Id = "EpicGames.EpicGamesLauncher", Name = "Epic Games Launcher", Category = "Jogos", Description = "Loja de jogos digitais da Epic Games com jogos gratuitos semanais." },
            new WinGetAppInfo { Id = "GOG.Galaxy", Name = "GOG Galaxy", Category = "Jogos", Description = "Cliente de jogos sem DRM com agregador de bibliotecas." }
        };

        public static void Install(string packageId)
        {
            try
            {
                LogEngine.Log(string.Format("Iniciando instalação oficial do WinGet para o pacote: {0}...", packageId), "info");

                string args = string.Format("install --id {0} --silent --accept-package-agreements --accept-source-agreements --exact", packageId);

                var psi = new ProcessStartInfo("winget.exe", args)
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
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            LogEngine.Log("> WinGet: " + line.Trim(), "info");
                        }
                    }
                    proc.WaitForExit();

                    if (proc.ExitCode == 0)
                    {
                        LogEngine.Log(string.Format("Software '{0}' instalado com sucesso pelo repositório oficial da Microsoft!", packageId), "success");
                    }
                    else
                    {
                        LogEngine.Log(string.Format("WinGet retornou código de saída: {0}", proc.ExitCode), "warning");
                    }
                }
            }
            catch (Exception ex)
            {
                LogEngine.Log("Erro ao executar WinGet: " + ex.Message, "danger");
            }
        }

        public static void UpgradeAll()
        {
            try
            {
                LogEngine.Log("Iniciando Atualização Geral de todos os aplicativos instalados no PC via WinGet...", "info");
                string args = "upgrade --all --include-unknown --accept-package-agreements --accept-source-agreements";

                var psi = new ProcessStartInfo("winget.exe", args)
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
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            LogEngine.Log("> " + line.Trim(), "info");
                        }
                    }
                    proc.WaitForExit();

                    if (proc.ExitCode == 0)
                    {
                        LogEngine.Log("Todos os aplicativos do sistema foram atualizados com sucesso para suas últimas versões!", "success");
                    }
                    else
                    {
                        LogEngine.Log(string.Format("WinGet finalizou com código {0}.", proc.ExitCode), "info");
                    }
                }
            }
            catch (Exception ex)
            {
                LogEngine.Log("Erro ao executar atualização global WinGet: " + ex.Message, "danger");
            }
        }

        public static string Search(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query)) return "[]";
                var psi = new ProcessStartInfo("winget.exe", string.Format("search \"{0}\" --accept-source-agreements", query))
                {
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                using (var proc = Process.Start(psi))
                {
                    string output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit(7000);
                    return output;
                }
            }
            catch (Exception ex)
            {
                return "Erro na busca: " + ex.Message;
            }
        }
    }
}
