using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;
using Microsoft.Win32;

namespace DebloaterPC
{
    public class ServiceDefinition
    {
        public string ServiceName { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string DefaultStart { get; set; } // "auto", "demand", "disabled"
        public bool DisableInSafeProfile { get; set; }
    }

    public static class ServicesOptimizer
    {
        public static readonly List<ServiceDefinition> Catalog = new List<ServiceDefinition>
        {
            new ServiceDefinition {
                ServiceName = "DiagTrack",
                DisplayName = "Experiências do Usuário e Telemetria",
                Description = "Coleta e envia dados diagnósticos e telemetria invasiva para a Microsoft.",
                Category = "Telemetria",
                DefaultStart = "auto",
                DisableInSafeProfile = true
            },
            new ServiceDefinition {
                ServiceName = "diagsvc",
                DisplayName = "Execução de Diagnóstico",
                Description = "Executa ações de diagnóstico e suporte sob demanda da Microsoft.",
                Category = "Telemetria",
                DefaultStart = "demand",
                DisableInSafeProfile = true
            },
            new ServiceDefinition {
                ServiceName = "WerSvc",
                DisplayName = "Relatórios de Erros do Windows",
                Description = "Envia despejos e relatórios de falhas de aplicativos à Microsoft.",
                Category = "Telemetria",
                DefaultStart = "demand",
                DisableInSafeProfile = true
            },
            new ServiceDefinition {
                ServiceName = "lfsvc",
                DisplayName = "Serviço de Geolocalização",
                Description = "Rastreia a localização física do computador continuamente.",
                Category = "Telemetria",
                DefaultStart = "demand",
                DisableInSafeProfile = true
            },
            new ServiceDefinition {
                ServiceName = "SysMain",
                DisplayName = "SysMain (Superfetch)",
                Description = "Pré-carrega aplicativos na memória; em SSDs gera escritas contínuas desnecessárias.",
                Category = "Sistema",
                DefaultStart = "auto",
                DisableInSafeProfile = true
            },
            new ServiceDefinition {
                ServiceName = "AssignedAccessManager",
                DisplayName = "Assigned Access Manager",
                Description = "Gerencia perfis de quiosque/totem público para execução de aplicativo único.",
                Category = "Sistema",
                DefaultStart = "demand",
                DisableInSafeProfile = true
            },
            new ServiceDefinition {
                ServiceName = "WpcMonSvc",
                DisplayName = "Controle dos Pais (Família)",
                Description = "Supervisão e restrições de tempo de tela de contas familiares.",
                Category = "Sistema",
                DefaultStart = "demand",
                DisableInSafeProfile = true
            },
            new ServiceDefinition {
                ServiceName = "WalletService",
                DisplayName = "Carteira Digital do Windows",
                Description = "Hospeda dados de cartões e pagamentos em apps UWP da Microsoft Store.",
                Category = "Sistema",
                DefaultStart = "demand",
                DisableInSafeProfile = true
            },
            new ServiceDefinition {
                ServiceName = "SensorService",
                DisplayName = "Serviço de Sensores",
                Description = "Gerencia sensores de rotação e luminosidade em notebooks e tablets.",
                Category = "Hardware",
                DefaultStart = "demand",
                DisableInSafeProfile = true
            },
            new ServiceDefinition {
                ServiceName = "WbioSrvc",
                DisplayName = "Biometria do Windows (Hello)",
                Description = "Serviço para leitor de digitais e reconhecimento facial da câmera.",
                Category = "Hardware",
                DefaultStart = "demand",
                DisableInSafeProfile = true
            },
            new ServiceDefinition {
                ServiceName = "ScDeviceEnum",
                DisplayName = "Enumeração de Dispositivo Smart Card",
                Description = "Detecta leitores de cartões inteligentes físicos conectados ao PC.",
                Category = "Hardware",
                DefaultStart = "demand",
                DisableInSafeProfile = true
            },
            new ServiceDefinition {
                ServiceName = "SCardSvr",
                DisplayName = "Cartão Inteligente (Smart Card)",
                Description = "Gerencia acesso a cartões criptográficos (OAB, certificados em token).",
                Category = "Hardware",
                DefaultStart = "demand",
                DisableInSafeProfile = true
            },
            new ServiceDefinition {
                ServiceName = "BDESVC",
                DisplayName = "Criptografia de Unidade BitLocker",
                Description = "Criptografa unidades de disco. Inútil se você não usa BitLocker.",
                Category = "Segurança",
                DefaultStart = "demand",
                DisableInSafeProfile = true
            },
            new ServiceDefinition {
                ServiceName = "Netlogon",
                DisplayName = "Logon de Rede (Netlogon)",
                Description = "Mantém canal seguro com controladores de domínio corporativo Active Directory.",
                Category = "Rede",
                DefaultStart = "demand",
                DisableInSafeProfile = true
            },
            new ServiceDefinition {
                ServiceName = "workfolderssvc",
                DisplayName = "Pastas de Trabalho (Work Folders)",
                Description = "Sincroniza arquivos de servidores de arquivos empresariais corporativos.",
                Category = "Rede",
                DefaultStart = "demand",
                DisableInSafeProfile = true
            },
            new ServiceDefinition {
                ServiceName = "TapiSrv",
                DisplayName = "Telefonia (TAPI)",
                Description = "Suporte a controle de modems de discagem e aparelhos telefônicos legados.",
                Category = "Rede",
                DefaultStart = "demand",
                DisableInSafeProfile = true
            },
            new ServiceDefinition {
                ServiceName = "SessionEnv",
                DisplayName = "Configuração de Área de Trabalho Remota",
                Description = "Prepara ambientes de sessão RDP em redes locais ou internet.",
                Category = "Rede",
                DefaultStart = "demand",
                DisableInSafeProfile = true
            },
            new ServiceDefinition {
                ServiceName = "TermService",
                DisplayName = "Serviços de Área de Trabalho Remota",
                Description = "Permite que outros computadores se conectem remotamente a este PC.",
                Category = "Rede",
                DefaultStart = "demand",
                DisableInSafeProfile = true
            },
            new ServiceDefinition {
                ServiceName = "icssvc",
                DisplayName = "Hotspot Móvel do Windows",
                Description = "Permite compartilhar sua conexão de internet com outros aparelhos via Wi-Fi.",
                Category = "Rede",
                DefaultStart = "demand",
                DisableInSafeProfile = true
            },
            new ServiceDefinition {
                ServiceName = "XboxGipSvc",
                DisplayName = "Acessórios de Jogos Xbox",
                Description = "Gerencia acessórios físicos e adaptadores de controle do Xbox.",
                Category = "Xbox / Jogos",
                DefaultStart = "demand",
                DisableInSafeProfile = true
            },
            new ServiceDefinition {
                ServiceName = "BcastDVRUserService",
                DisplayName = "Game DVR e Broadcast",
                Description = "Gravação contínua em segundo plano de vídeos e streaming de jogos.",
                Category = "Xbox / Jogos",
                DefaultStart = "demand",
                DisableInSafeProfile = true
            },
            new ServiceDefinition {
                ServiceName = "Spooler",
                DisplayName = "Spooler de Impressão",
                Description = "Envia documentos para a fila de impressoras físicas ou virtuais.",
                Category = "Hardware",
                DefaultStart = "auto",
                DisableInSafeProfile = false // SEGURO: Mantido ativo no perfil equilibrado!
            },
            new ServiceDefinition {
                ServiceName = "bthserv",
                DisplayName = "Suporte a Bluetooth",
                Description = "Descobre e emparelha dispositivos Bluetooth como fones e teclados.",
                Category = "Hardware",
                DefaultStart = "demand",
                DisableInSafeProfile = false // SEGURO: Mantido ativo no perfil equilibrado!
            },
            new ServiceDefinition {
                ServiceName = "BTAGService",
                DisplayName = "Gateway de Áudio Bluetooth",
                Description = "Encaminha áudio de chamadas e microfones para headsets Bluetooth.",
                Category = "Hardware",
                DefaultStart = "demand",
                DisableInSafeProfile = false // SEGURO: Mantido ativo no perfil equilibrado!
            }
        };

        public static bool IsServiceDisabled(string serviceName)
        {
            try
            {
                // Verifica no registro HKLM\SYSTEM\CurrentControlSet\Services\<name>\Start (4 = Disabled)
                using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + serviceName))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("Start");
                        if (val != null)
                        {
                            int start = Convert.ToInt32(val);
                            return start == 4;
                        }
                    }
                }
            }
            catch {}
            return false;
        }

        public static string GetServiceStartupType(string serviceName)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + serviceName))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("Start");
                        if (val != null)
                        {
                            int start = Convert.ToInt32(val);
                            if (start == 2) return "Automático";
                            if (start == 3) return "Manual";
                            if (start == 4) return "Desativado";
                        }
                    }
                }
            }
            catch {}
            return "Desconhecido";
        }

        public static bool ToggleService(string serviceName, bool disable)
        {
            try
            {
                string startArg = disable ? "disabled" : "demand";
                LogEngine.Log(string.Format("Serviço '{0}' -> Definindo inicialização como {1}...", serviceName, disable ? "DESATIVADO" : "MANUAL"), "info");

                // sc config <name> start=disabled / demand
                RunCommand("sc.exe", string.Format("config {0} start={1}", serviceName, startArg));

                if (disable)
                {
                    RunCommand("sc.exe", string.Format("stop {0}", serviceName));
                    LogEngine.Log(string.Format("Serviço '{0}' interrompido e desativado com sucesso.", serviceName), "success");
                }
                else
                {
                    LogEngine.Log(string.Format("Serviço '{0}' reativado para inicialização manual/demanda.", serviceName), "success");
                }
                return true;
            }
            catch (Exception ex)
            {
                LogEngine.Log(string.Format("Erro ao alterar serviço {0}: {1}", serviceName, ex.Message), "danger");
                return false;
            }
        }

        public static void ApplyPreset(string preset)
        {
            LogEngine.Log(string.Format("Aplicando perfil de serviços: '{0}'...", preset.ToUpperInvariant()), "info");
            int altered = 0;

            if (preset == "balanced" || preset == "safe")
            {
                foreach (var s in Catalog)
                {
                    if (s.DisableInSafeProfile)
                    {
                        ToggleService(s.ServiceName, true);
                        altered++;
                    }
                    else
                    {
                        // Garante que Spooler e Bluetooth permaneçam ativos
                        ToggleService(s.ServiceName, false);
                    }
                }
                LogEngine.Log(string.Format("Perfil Seguro/Equilibrado aplicado: {0} serviços desativados (Impressora e Bluetooth preservados).", altered), "success");
            }
            else if (preset == "gamer" || preset == "aggressive")
            {
                foreach (var s in Catalog)
                {
                    ToggleService(s.ServiceName, true);
                    altered++;
                }
                LogEngine.Log(string.Format("Perfil Máximo Desempenho aplicado: Todos os {0} serviços foram desativados para alívio total de CPU/RAM.", altered), "success");
            }
            else if (preset == "restore")
            {
                foreach (var s in Catalog)
                {
                    string startMode = (s.DefaultStart == "auto") ? "auto" : "demand";
                    RunCommand("sc.exe", string.Format("config {0} start={1}", s.ServiceName, startMode));
                    if (s.DefaultStart == "auto")
                    {
                        RunCommand("sc.exe", string.Format("start {0}", s.ServiceName));
                    }
                    altered++;
                }
                LogEngine.Log(string.Format("Restaurados padrões de fábrica para todos os {0} serviços do Windows.", altered), "success");
            }
        }

        private static void RunCommand(string file, string args)
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
