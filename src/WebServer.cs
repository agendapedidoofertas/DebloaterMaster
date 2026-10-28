using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace DebloaterPC
{
    public class WebServer
    {
        public static DateTime LastActivity = DateTime.UtcNow;
        public static bool HasReceivedRequests = false;
        private readonly HttpListener _listener = new HttpListener();
        private readonly string _uiPath;
        private readonly int _port;
        private bool _isRunning = false;

        public int Port { get { return _port; } }

        public WebServer(string uiPath, int port)
        {
            _uiPath = uiPath;
            _port = port;
            _listener.Prefixes.Add(string.Format("http://127.0.0.1:{0}/", port));
        }

        public void Start()
        {
            _listener.Start();
            _isRunning = true;
            ThreadPool.QueueUserWorkItem(ListenLoop);
        }

        public void Stop()
        {
            _isRunning = false;
            try { _listener.Stop(); } catch {}
        }

        private void ListenLoop(object state)
        {
            while (_isRunning && _listener.IsListening)
            {
                try
                {
                    var context = _listener.GetContext();
                    ThreadPool.QueueUserWorkItem((ctx) => HandleRequest((HttpListenerContext)ctx), context);
                }
                catch
                {
                    if (!_isRunning) break;
                }
            }
        }

        private void HandleRequest(HttpListenerContext context)
        {
            HasReceivedRequests = true;
            LastActivity = DateTime.UtcNow;
            var req = context.Request;
            var res = context.Response;

            // CORS headers for local webview
            res.Headers.Add("Access-Control-Allow-Origin", "*");
            res.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            res.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

            if (req.HttpMethod == "OPTIONS")
            {
                res.StatusCode = 200;
                res.Close();
                return;
            }

            string rawUrl = req.Url.AbsolutePath;

            try
            {
                if (rawUrl == "/api/ping")
                {
                    SendJson(res, "{\"pong\":true}");
                    return;
                }
                else if (rawUrl == "/api/exit" && req.HttpMethod == "POST")
                {
                    SendJson(res, "{\"success\":true}");
                    ThreadPool.QueueUserWorkItem((s) => {
                        Thread.Sleep(400);
                        Environment.Exit(0);
                    });
                    return;
                }
                // Core & Status Endpoints
                if (rawUrl == "/api/diagnostics")
                {
                    HandleDiagnostics(res);
                }
                else if (rawUrl == "/api/logs")
                {
                    HandleLogs(req, res);
                }
                else if (rawUrl == "/api/audit" && req.HttpMethod == "GET")
                {
                    HandleAudit(res);
                }
                else if (rawUrl == "/api/tweak-toggle" && req.HttpMethod == "POST")
                {
                    HandleTweakToggle(req, res);
                }
                else if (rawUrl == "/api/optimize" && req.HttpMethod == "POST")
                {
                    HandleOptimize(req, res);
                }
                else if (rawUrl == "/api/restore-point" && req.HttpMethod == "POST")
                {
                    HandleRestorePoint(res);
                }
                else if (rawUrl == "/api/rollback" && req.HttpMethod == "POST")
                {
                    HandleRollback(res);
                }
                else if (rawUrl == "/api/integrity-check" && req.HttpMethod == "POST")
                {
                    HandleIntegrityCheck(res);
                }
                else if (rawUrl == "/api/restart-elevated" && req.HttpMethod == "POST")
                {
                    HandleRestartElevated(res);
                }
                // Startup
                else if (rawUrl == "/api/startup" && req.HttpMethod == "GET")
                {
                    HandleStartupGet(res);
                }
                else if (rawUrl == "/api/startup-toggle" && req.HttpMethod == "POST")
                {
                    HandleStartupToggle(req, res);
                }
                // Services (Decodificando TI 24 services)
                else if (rawUrl == "/api/services-catalog" && req.HttpMethod == "GET")
                {
                    HandleServicesCatalog(res);
                }
                else if (rawUrl == "/api/services-toggle" && req.HttpMethod == "POST")
                {
                    HandleServicesToggle(req, res);
                }
                else if (rawUrl == "/api/services-preset" && req.HttpMethod == "POST")
                {
                    HandleServicesPreset(req, res);
                }
                // Scheduled Tasks (6 tasks)
                else if (rawUrl == "/api/tasks-list" && req.HttpMethod == "GET")
                {
                    HandleTasksList(res);
                }
                else if (rawUrl == "/api/tasks-toggle" && req.HttpMethod == "POST")
                {
                    HandleTasksToggle(req, res);
                }
                else if (rawUrl == "/api/tasks-optimize" && req.HttpMethod == "POST")
                {
                    HandleTasksOptimize(req, res);
                }
                // Network, Hosts & Firewall
                else if (rawUrl == "/api/network-status" && req.HttpMethod == "GET")
                {
                    HandleNetworkStatus(res);
                }
                else if (rawUrl == "/api/network-hosts-toggle" && req.HttpMethod == "POST")
                {
                    HandleNetworkHostsToggle(req, res);
                }
                else if (rawUrl == "/api/network-firewall-toggle" && req.HttpMethod == "POST")
                {
                    HandleNetworkFirewallToggle(req, res);
                }
                else if (rawUrl == "/api/windows-update-security" && req.HttpMethod == "POST")
                {
                    HandleWindowsUpdateSecurity(req, res);
                }
                else if (rawUrl == "/api/delivery-opt-toggle" && req.HttpMethod == "POST")
                {
                    HandleDeliveryOptToggle(req, res);
                }
                // Gamer & Latency
                else if (rawUrl == "/api/gamer-status" && req.HttpMethod == "GET")
                {
                    HandleGamerStatus(res);
                }
                else if (rawUrl == "/api/gamer-optimize" && req.HttpMethod == "POST")
                {
                    HandleGamerOptimize(req, res);
                }
                // Windows 11 Modern & Explorer
                else if (rawUrl == "/api/w11-modern-recall" && req.HttpMethod == "POST")
                {
                    HandleW11RecallToggle(req, res);
                }
                else if (rawUrl == "/api/w11-labconfig" && req.HttpMethod == "POST")
                {
                    HandleW11LabConfigToggle(req, res);
                }
                else if (rawUrl == "/api/explorer-tweak" && req.HttpMethod == "POST")
                {
                    HandleExplorerTweak(req, res);
                }
                // Apps & UWP Scanner
                else if (rawUrl == "/api/apps-scan" && req.HttpMethod == "GET")
                {
                    HandleAppsScan(res);
                }
                else if (rawUrl == "/api/apps-remove-single" && req.HttpMethod == "POST")
                {
                    HandleAppsRemoveSingle(req, res);
                }
                else if (rawUrl == "/api/apps-resurrect-store" && req.HttpMethod == "POST")
                {
                    HandleAppsResurrectStore(res);
                }
                else if (rawUrl == "/api/apps-debloat-batch" && req.HttpMethod == "POST")
                {
                    HandleAppsDebloatBatch(req, res);
                }
                else if (rawUrl == "/api/supervision-disable" && req.HttpMethod == "POST")
                {
                    ThreadPool.QueueUserWorkItem((s) => {
                        LogEngine.IsExecuting = true;
                        SupervisionManager.DisableAllSupervisionAndBackground();
                        LogEngine.IsExecuting = false;
                    });
                    SendJson(res, "{\"success\":true,\"message\":\"Desativação de supervisão iniciada.\"}");
                }
                else if (rawUrl == "/api/supervision-kill" && req.HttpMethod == "POST")
                {
                    SupervisionManager.KillRunningSupervisionProcesses();
                    SendJson(res, "{\"success\":true,\"message\":\"Processos de supervisão encerrados.\"}");
                }
                else if (rawUrl == "/api/uninstall-stubborn" && req.HttpMethod == "POST")
                {
                    HandleUninstallStubborn(req, res);
                }
                // WinGet Catalog & Upgrade All
                else if (rawUrl == "/api/winget-catalog" && req.HttpMethod == "GET")
                {
                    HandleWingetCatalog(res);
                }
                else if (rawUrl == "/api/winget-search" && req.HttpMethod == "GET")
                {
                    HandleWingetSearch(req, res);
                }
                else if (rawUrl == "/api/winget-install" && req.HttpMethod == "POST")
                {
                    HandleWingetInstall(req, res);
                }
                else if (rawUrl == "/api/winget-upgrade-all" && req.HttpMethod == "POST")
                {
                    HandleWingetUpgradeAll(res);
                }
                else
                {
                    // Static File Serving
                    ServeStaticFile(rawUrl, res);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    res.StatusCode = 500;
                    byte[] err = Encoding.UTF8.GetBytes("{\"error\":\"" + EscapeJson(ex.Message) + "\"}");
                    res.OutputStream.Write(err, 0, err.Length);
                    res.Close();
                }
                catch {}
            }
        }

        private void ServeStaticFile(string path, HttpListenerResponse res)
        {
            if (path == "/" || string.IsNullOrEmpty(path)) path = "/index.html";

            string localFile = Path.Combine(_uiPath, path.TrimStart('/').Replace('/', '\\'));

            if (!File.Exists(localFile))
            {
                res.StatusCode = 404;
                byte[] notFound = Encoding.UTF8.GetBytes("404 Not Found");
                res.OutputStream.Write(notFound, 0, notFound.Length);
                res.Close();
                return;
            }

            string ext = Path.GetExtension(localFile).ToLowerInvariant();
            string mime = "text/plain";
            if (ext == ".html") mime = "text/html; charset=utf-8";
            else if (ext == ".css") mime = "text/css; charset=utf-8";
            else if (ext == ".js") mime = "application/javascript; charset=utf-8";
            else if (ext == ".woff2") mime = "font/woff2";
            else if (ext == ".woff") mime = "font/woff";
            else if (ext == ".ttf") mime = "font/ttf";
            else if (ext == ".svg") mime = "image/svg+xml";
            else if (ext == ".png") mime = "image/png";
            else if (ext == ".jpg" || ext == ".jpeg") mime = "image/jpeg";
            else if (ext == ".ico") mime = "image/x-icon";

            res.ContentType = mime;
            byte[] fileBytes = File.ReadAllBytes(localFile);
            res.ContentLength64 = fileBytes.Length;
            res.OutputStream.Write(fileBytes, 0, fileBytes.Length);
            res.Close();
        }

        private void HandleDiagnostics(HttpListenerResponse res)
        {
            int cpu = Diagnostics.GetCpuUsage();
            string cpuModel = Diagnostics.GetCpuModel();

            double ramTotal, ramUsed;
            int ramPercent;
            Diagnostics.GetRamInfo(out ramTotal, out ramUsed, out ramPercent);

            double diskTotal, diskFree;
            int diskUsedPercent;
            Diagnostics.GetDiskInfo(out diskTotal, out diskFree, out diskUsedPercent);

            bool trim = Diagnostics.CheckTrimStatus();
            bool comp = Diagnostics.CheckMemoryCompressionStatus();

            string osName, osBuild, uptime;
            Diagnostics.GetOsInfo(out osName, out osBuild, out uptime);
            bool isAdmin = Diagnostics.IsAdministrator();

            string json = string.Format(
                "{{\"cpuUsage\":{0},\"cpuModel\":\"{1}\",\"ramUsagePercent\":{2},\"ramUsedGB\":{3},\"ramTotalGB\":{4}," +
                "\"diskFreeGB\":{5},\"diskTotalGB\":{6},\"diskUsedPercent\":{7},\"trimEnabled\":{8},\"memoryCompression\":{9}," +
                "\"osName\":\"{10}\",\"osBuild\":\"{11}\",\"uptime\":\"{12}\",\"isAdmin\":{13}}}",
                cpu, EscapeJson(cpuModel), ramPercent, ramUsed.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture),
                ramTotal.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture),
                diskFree.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture),
                diskTotal.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture),
                diskUsedPercent, trim ? "true" : "false", comp ? "true" : "false",
                EscapeJson(osName), EscapeJson(osBuild), EscapeJson(uptime), isAdmin ? "true" : "false"
            );

            SendJson(res, json);
        }

        private void HandleLogs(HttpListenerRequest req, HttpListenerResponse res)
        {
            int offset = 0;
            int.TryParse(req.QueryString["offset"], out offset);

            int nextOffset;
            var entries = LogEngine.GetLogsFrom(offset, out nextOffset);

            var sb = new StringBuilder();
            sb.Append("{\"lines\":[");
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                sb.Append(string.Format("{{\"text\":\"{0}\",\"type\":\"{1}\"}}", EscapeJson(entry.Text), EscapeJson(entry.Type)));
                if (i < entries.Count - 1) sb.Append(",");
            }
            sb.Append(string.Format("],\"nextOffset\":{0},\"progress\":{1},\"currentTask\":\"{2}\",\"completed\":{3}}}",
                nextOffset, LogEngine.CurrentProgress, EscapeJson(LogEngine.CurrentTask ?? ""),
                (!LogEngine.IsExecuting && LogEngine.CurrentProgress >= 100) ? "true" : "false"));

            SendJson(res, sb.ToString());
        }

        private void HandleAudit(HttpListenerResponse res)
        {
            var auditState = AuditManager.GetAuditState();
            var sb = new StringBuilder();
            sb.Append("{");
            int i = 0;
            foreach (var kvp in auditState)
            {
                sb.Append(string.Format("\"{0}\":{1}", kvp.Key, kvp.Value ? "true" : "false"));
                if (i < auditState.Count - 1) sb.Append(",");
                i++;
            }
            sb.Append("}");
            SendJson(res, sb.ToString());
        }

        private void HandleRestartElevated(HttpListenerResponse res)
        {
            try
            {
                SendJson(res, "{\"success\":true,\"message\":\"Solicitando elevação de Administrador (UAC)...\"}");
                ThreadPool.QueueUserWorkItem((s) => {
                    Thread.Sleep(800);
                    try
                    {
                        var psi = new System.Diagnostics.ProcessStartInfo();
                        psi.FileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                        psi.Arguments = "";
                        psi.Verb = "runas";
                        psi.UseShellExecute = true;
                        System.Diagnostics.Process.Start(psi);
                        Environment.Exit(0);
                    }
                    catch (Exception ex)
                    {
                        LogEngine.Log("Falha ao elevar processo: " + ex.Message, "danger");
                    }
                });
            }
            catch (Exception ex)
            {
                SendJson(res, string.Format("{{\"success\":false,\"error\":\"{0}\"}}", EscapeJson(ex.Message)));
            }
        }

        private void HandleTweakToggle(HttpListenerRequest req, HttpListenerResponse res)
        {
            string body = GetRequestBody(req);
            string id = ExtractJsonField(body, "id");
            bool enable = body.Contains("\"enable\":true");
            bool ok = AuditManager.ToggleSingleTweak(id, enable);
            SendJson(res, string.Format("{{\"success\":{0},\"id\":\"{1}\",\"enabled\":{2}}}", ok ? "true" : "false", EscapeJson(id), enable ? "true" : "false"));
        }

        private void HandleServicesCatalog(HttpListenerResponse res)
        {
            var sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < ServicesOptimizer.Catalog.Count; i++)
            {
                var s = ServicesOptimizer.Catalog[i];
                bool disabled = ServicesOptimizer.IsServiceDisabled(s.ServiceName);
                string startup = ServicesOptimizer.GetServiceStartupType(s.ServiceName);

                sb.Append(string.Format(
                    "{{\"serviceName\":\"{0}\",\"displayName\":\"{1}\",\"description\":\"{2}\",\"category\":\"{3}\",\"disabled\":{4},\"startupType\":\"{5}\",\"safe\":{6}}}",
                    EscapeJson(s.ServiceName), EscapeJson(s.DisplayName), EscapeJson(s.Description), EscapeJson(s.Category),
                    disabled ? "true" : "false", EscapeJson(startup), s.DisableInSafeProfile ? "true" : "false"
                ));
                if (i < ServicesOptimizer.Catalog.Count - 1) sb.Append(",");
            }
            sb.Append("]");
            SendJson(res, sb.ToString());
        }

        private void HandleServicesToggle(HttpListenerRequest req, HttpListenerResponse res)
        {
            string body = GetRequestBody(req);
            string sName = ExtractJsonField(body, "serviceName");
            bool disable = body.Contains("\"disable\":true");
            bool ok = ServicesOptimizer.ToggleService(sName, disable);
            SendJson(res, string.Format("{{\"success\":{0}}}", ok ? "true" : "false"));
        }

        private void HandleServicesPreset(HttpListenerRequest req, HttpListenerResponse res)
        {
            string body = GetRequestBody(req);
            string preset = ExtractJsonField(body, "preset");
            ThreadPool.QueueUserWorkItem((s) => {
                LogEngine.IsExecuting = true;
                ServicesOptimizer.ApplyPreset(preset);
                LogEngine.IsExecuting = false;
            });
            SendJson(res, "{\"success\":true,\"message\":\"Perfil de serviços iniciado em segundo plano.\"}");
        }

        private void HandleTasksList(HttpListenerResponse res)
        {
            var sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < ScheduledTasksManager.Tasks.Count; i++)
            {
                var t = ScheduledTasksManager.Tasks[i];
                bool disabled = ScheduledTasksManager.IsTaskDisabled(t.Path);
                sb.Append(string.Format(
                    "{{\"id\":\"{0}\",\"name\":\"{1}\",\"path\":\"{2}\",\"description\":\"{3}\",\"disabled\":{4}}}",
                    EscapeJson(t.Id), EscapeJson(t.Name), EscapeJson(t.Path), EscapeJson(t.Description), disabled ? "true" : "false"
                ));
                if (i < ScheduledTasksManager.Tasks.Count - 1) sb.Append(",");
            }
            sb.Append("]");
            SendJson(res, sb.ToString());
        }

        private void HandleTasksToggle(HttpListenerRequest req, HttpListenerResponse res)
        {
            string body = GetRequestBody(req);
            string path = ExtractJsonField(body, "path");
            bool disable = body.Contains("\"disable\":true");
            bool ok = ScheduledTasksManager.ToggleTask(path, disable);
            SendJson(res, string.Format("{{\"success\":{0}}}", ok ? "true" : "false"));
        }

        private void HandleTasksOptimize(HttpListenerRequest req, HttpListenerResponse res)
        {
            string body = GetRequestBody(req);
            bool disable = body.Contains("\"disable\":true");
            ThreadPool.QueueUserWorkItem((s) => {
                LogEngine.IsExecuting = true;
                ScheduledTasksManager.ApplyAll(disable);
                LogEngine.IsExecuting = false;
            });
            SendJson(res, "{\"success\":true,\"message\":\"Otimização das tarefas iniciada.\"}");
        }

        private void HandleNetworkStatus(HttpListenerResponse res)
        {
            bool hostsBlocked = NetworkBlockManager.IsHostsBlockActive();
            bool fwBlocked = NetworkBlockManager.IsFirewallBlockActive();
            bool wuSec = WindowsUpdateManager.IsSecurityModeActive();
            bool p2pOff = WindowsUpdateManager.IsDeliveryOptimizationP2PDisabled();

            string json = string.Format(
                "{{\"hostsBlocked\":{0},\"firewallBlocked\":{1},\"wuSecurity\":{2},\"deliveryOptDisabled\":{3}}}",
                hostsBlocked ? "true" : "false", fwBlocked ? "true" : "false",
                wuSec ? "true" : "false", p2pOff ? "true" : "false"
            );
            SendJson(res, json);
        }

        private void HandleNetworkHostsToggle(HttpListenerRequest req, HttpListenerResponse res)
        {
            string body = GetRequestBody(req);
            bool enable = body.Contains("\"enable\":true");
            bool ok = NetworkBlockManager.ToggleHostsBlock(enable);
            SendJson(res, string.Format("{{\"success\":{0}}}", ok ? "true" : "false"));
        }

        private void HandleNetworkFirewallToggle(HttpListenerRequest req, HttpListenerResponse res)
        {
            string body = GetRequestBody(req);
            bool enable = body.Contains("\"enable\":true");
            bool ok = NetworkBlockManager.ToggleFirewallBlock(enable);
            SendJson(res, string.Format("{{\"success\":{0}}}", ok ? "true" : "false"));
        }

        private void HandleWindowsUpdateSecurity(HttpListenerRequest req, HttpListenerResponse res)
        {
            string body = GetRequestBody(req);
            bool enable = body.Contains("\"enable\":true");
            bool ok = WindowsUpdateManager.SetSecurityMode(enable);
            SendJson(res, string.Format("{{\"success\":{0}}}", ok ? "true" : "false"));
        }

        private void HandleDeliveryOptToggle(HttpListenerRequest req, HttpListenerResponse res)
        {
            string body = GetRequestBody(req);
            bool disable = body.Contains("\"disable\":true");
            bool ok = WindowsUpdateManager.SetDeliveryOptimizationP2P(disable);
            SendJson(res, string.Format("{{\"success\":{0}}}", ok ? "true" : "false"));
        }

        private void HandleGamerStatus(HttpListenerResponse res)
        {
            bool throttlingOff = GamerOptimizer.IsNetworkThrottlingDisabled();
            bool mouse1to1 = GamerOptimizer.IsMouseAccelerationDisabled();
            string json = string.Format("{{\"networkThrottlingDisabled\":{0},\"mouseAccelerationDisabled\":{1}}}",
                throttlingOff ? "true" : "false", mouse1to1 ? "true" : "false");
            SendJson(res, json);
        }

        private void HandleGamerOptimize(HttpListenerRequest req, HttpListenerResponse res)
        {
            string body = GetRequestBody(req);
            bool enable = body.Contains("\"enable\":true");
            string action = ExtractJsonField(body, "action");

            if (action == "network")
            {
                GamerOptimizer.SetNetworkThrottlingDisabled(enable);
            }
            else if (action == "mouse")
            {
                GamerOptimizer.SetMouseAccelerationDisabled(enable);
            }
            else if (action == "responsiveness")
            {
                GamerOptimizer.SetNetworkThrottlingDisabled(enable);
            }
            else
            {
                GamerOptimizer.ApplyGamerPreset(enable);
            }
            SendJson(res, "{\"success\":true,\"message\":\"Ajustes gamer processados com sucesso.\"}");
        }

        private void HandleW11RecallToggle(HttpListenerRequest req, HttpListenerResponse res)
        {
            string body = GetRequestBody(req);
            bool disable = body.Contains("\"disable\":true");
            bool ok = Windows11ModernManager.SetRecallDisabled(disable);
            SendJson(res, string.Format("{{\"success\":{0}}}", ok ? "true" : "false"));
        }

        private void HandleW11LabConfigToggle(HttpListenerRequest req, HttpListenerResponse res)
        {
            string body = GetRequestBody(req);
            bool enable = body.Contains("\"enable\":true");
            bool ok = Windows11ModernManager.SetLabConfigBypass(enable);
            SendJson(res, string.Format("{{\"success\":{0}}}", ok ? "true" : "false"));
        }

        private void HandleExplorerTweak(HttpListenerRequest req, HttpListenerResponse res)
        {
            string body = GetRequestBody(req);
            string action = ExtractJsonField(body, "action");
            bool enable = body.Contains("\"enable\":true");

            if (action == "restart") ExplorerVisualManager.RestartExplorer();
            else if (action == "showExt") ExplorerVisualManager.SetShowFileExtensions(enable);
            else if (action == "showHidden") ExplorerVisualManager.SetShowHiddenFiles(enable);
            else if (action == "thisPC") ExplorerVisualManager.SetLaunchToThisPC(enable);
            else if (action == "classicMenu") RegistryManager.SetClassicContextMenu(enable);
            else if (action == "removePrinters") ExplorerVisualManager.RemoveFaxAndXpsPrinters();
            else if (action == "endTask") ExplorerVisualManager.SetTaskbarEndTask(enable);
            else if (action == "alignLeft") ExplorerVisualManager.SetTaskbarAlignment(0);
            else if (action == "alignCenter") ExplorerVisualManager.SetTaskbarAlignment(1);

            SendJson(res, "{\"success\":true}");
        }

        private void HandleAppsScan(HttpListenerResponse res)
        {
            var apps = UwpDebloater.ScanInstalledApps();
            var sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < apps.Count; i++)
            {
                var a = apps[i];
                sb.Append(string.Format(
                    "{{\"name\":\"{0}\",\"packageFullName\":\"{1}\",\"version\":\"{2}\",\"sizeMb\":{3},\"category\":\"{4}\",\"isWhitelisted\":{5}}}",
                    EscapeJson(a.Name), EscapeJson(a.PackageFullName), EscapeJson(a.Version),
                    a.SizeMb.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture),
                    EscapeJson(a.Category), a.IsWhitelisted ? "true" : "false"
                ));
                if (i < apps.Count - 1) sb.Append(",");
            }
            sb.Append("]");
            SendJson(res, sb.ToString());
        }

        private void HandleAppsRemoveSingle(HttpListenerRequest req, HttpListenerResponse res)
        {
            string body = GetRequestBody(req);
            string pkg = ExtractJsonField(body, "packageName");
            bool deprov = body.Contains("\"deprovision\":true");
            ThreadPool.QueueUserWorkItem((s) => {
                LogEngine.IsExecuting = true;
                UwpDebloater.RemovePackage(pkg, deprov);
                LogEngine.IsExecuting = false;
            });
            SendJson(res, "{\"success\":true,\"message\":\"Remoção do pacote iniciada.\"}");
        }

        private void HandleAppsResurrectStore(HttpListenerResponse res)
        {
            ThreadPool.QueueUserWorkItem((s) => {
                LogEngine.IsExecuting = true;
                UwpDebloater.ResurrectWindowsStore();
                LogEngine.IsExecuting = false;
            });
            SendJson(res, "{\"success\":true,\"message\":\"Ressurreição da Microsoft Store iniciada.\"}");
        }

        private void HandleAppsDebloatBatch(HttpListenerRequest req, HttpListenerResponse res)
        {
            string body = GetRequestBody(req);
            string cat = ExtractJsonField(body, "category");
            ThreadPool.QueueUserWorkItem((s) => {
                LogEngine.IsExecuting = true;
                if (cat == "sponsored") UwpDebloater.RunDebloat(true, false, false, true);
                else if (cat == "assistant") UwpDebloater.RunDebloat(false, true, false, true);
                else if (cat == "widgets") UwpDebloater.RunDebloat(false, false, true, true);
                else UwpDebloater.RunDebloat(true, true, true, true);
                LogEngine.IsExecuting = false;
            });
            SendJson(res, "{\"success\":true,\"message\":\"Remoção em lote iniciada.\"}");
        }

        private void HandleWingetCatalog(HttpListenerResponse res)
        {
            var sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < WinGetInstaller.Catalog.Count; i++)
            {
                var it = WinGetInstaller.Catalog[i];
                sb.Append(string.Format("{{\"id\":\"{0}\",\"name\":\"{1}\",\"category\":\"{2}\",\"description\":\"{3}\"}}",
                    EscapeJson(it.Id), EscapeJson(it.Name), EscapeJson(it.Category), EscapeJson(it.Description)));
                if (i < WinGetInstaller.Catalog.Count - 1) sb.Append(",");
            }
            sb.Append("]");
            SendJson(res, sb.ToString());
        }

        private void HandleWingetSearch(HttpListenerRequest req, HttpListenerResponse res)
        {
            string query = req.QueryString["query"] ?? "";
            string result = WinGetInstaller.Search(query);
            SendJson(res, string.Format("{{\"result\":\"{0}\"}}", EscapeJson(result)));
        }

        private void HandleWingetUpgradeAll(HttpListenerResponse res)
        {
            ThreadPool.QueueUserWorkItem((s) => {
                LogEngine.IsExecuting = true;
                WinGetInstaller.UpgradeAll();
                LogEngine.IsExecuting = false;
            });
            SendJson(res, "{\"success\":true,\"message\":\"Atualização geral de aplicativos via WinGet iniciada.\"}");
        }

        private void HandleOptimize(HttpListenerRequest req, HttpListenerResponse res)
        {
            string body = GetRequestBody(req);
            ThreadPool.QueueUserWorkItem((s) => ExecuteOptimization(body));
            SendJson(res, "{\"success\":true,\"message\":\"Otimização iniciada em segundo plano.\"}");
        }

        private void ExecuteOptimization(string body)
        {
            LogEngine.IsExecuting = true;
            LogEngine.SetProgress(5, "Iniciando processo de otimização...");

            // Snapshot de registro e ponto de restauração
            SafetyManager.BackupRegistrySnapshot();
            string err;
            SafetyManager.CreateRestorePoint("DebloaterPC_AutoBackup", out err);

            bool isAggressive = body.Contains("\"preset\":\"aggressive\"");
            bool isCustom = body.Contains("\"preset\":\"custom\"");

            if (isAggressive)
            {
                LogEngine.SetProgress(15, "Aplicando Preset Máximo Desempenho (PC Fraco)...");
                // Memória e CPU
                MemoryManager.ToggleMemoryCompression(true);
                MemoryManager.ConfigureServices(true, true);
                MemoryManager.SetHighPerformancePlan();

                LogEngine.SetProgress(30, "Otimizando armazenamento e SSD...");
                DiskManager.DisableHibernation(true);
                DiskManager.EnsureTrimEnabled();
                DiskManager.Disable8Dot3Names(true);
                DiskManager.DeepCleanTemps();

                LogEngine.SetProgress(45, "Desativando 24 Serviços Inúteis e 6 Tarefas de Telemetria...");
                ServicesOptimizer.ApplyPreset("gamer");
                ScheduledTasksManager.ApplyAll(true);

                LogEngine.SetProgress(60, "Bloqueando Telemetria por Rede (Hosts) e Políticas...");
                NetworkBlockManager.ToggleHostsBlock(true);
                NetworkBlockManager.ToggleFirewallBlock(true);
                SupervisionManager.DisableAllSupervisionAndBackground();
                WindowsUpdateManager.SetDeliveryOptimizationP2P(true);
                RegistryManager.ApplyPrivacyTweaks(true);
                RegistryManager.OptimizeSearchBar(true);
                RegistryManager.DisableAdsAndTracking(true);
                RegistryManager.ApplyBackgroundAppsPolicy(true);

                LogEngine.SetProgress(75, "Aplicando Otimizações Gamer (Zero Latência)...");
                GamerOptimizer.ApplyGamerPreset(true);

                LogEngine.SetProgress(85, "Otimizações do Chrome/Edge e Debloat UWP...");
                ChromeOptimizer.ApplyBrowserPolicies(true, true, true);
                ChromeOptimizer.CleanBrowserCaches();
                UwpDebloater.RunDebloat(true, true, true, true);

                LogEngine.SetProgress(95, "Reiniciando Windows Explorer...");
                ExplorerVisualManager.RestartExplorer();
            }
            else if (isCustom)
            {
                LogEngine.SetProgress(15, "Aplicando ajustes personalizados selecionados...");
                if (body.Contains("\"disableMMAgent\":true")) MemoryManager.ToggleMemoryCompression(true);
                if (body.Contains("\"disableDiagTrack\":true")) MemoryManager.ConfigureServices(true, false);
                if (body.Contains("\"disableSysMain\":true")) MemoryManager.ConfigureServices(false, true);
                if (body.Contains("\"highPerformance\":true")) MemoryManager.SetHighPerformancePlan();

                LogEngine.SetProgress(35, "Ajustes de disco...");
                if (body.Contains("\"disableHibernation\":true")) DiskManager.DisableHibernation(true);
                if (body.Contains("\"enableTrim\":true")) DiskManager.EnsureTrimEnabled();
                if (body.Contains("\"disable8dot3\":true")) DiskManager.Disable8Dot3Names(true);
                if (body.Contains("\"cleanTemps\":true")) DiskManager.DeepCleanTemps();
                if (body.Contains("\"winsxsResetBase\":true")) DiskManager.CompactWinSxS();

                LogEngine.SetProgress(55, "Ajustes de registro, privacidade e tarefas...");
                if (body.Contains("\"blockTelemetry\":true")) RegistryManager.ApplyPrivacyTweaks(true);
                if (body.Contains("\"optimizeSearch\":true")) RegistryManager.OptimizeSearchBar(true);
                if (body.Contains("\"disableBackgroundApps\":true")) RegistryManager.ApplyBackgroundAppsPolicy(true);
                if (body.Contains("\"disableAdsTracking\":true")) RegistryManager.DisableAdsAndTracking(true);
                if (body.Contains("\"disableTasks\":true")) ScheduledTasksManager.ApplyAll(true);
                if (body.Contains("\"hostsBlock\":true")) NetworkBlockManager.ToggleHostsBlock(true);
                if (body.Contains("\"gamerTweaks\":true")) GamerOptimizer.ApplyGamerPreset(true);

                LogEngine.SetProgress(75, "Navegadores e Visual...");
                if (body.Contains("\"browserNoBackground\":true") || body.Contains("\"browserMemorySaver\":true"))
                    ChromeOptimizer.ApplyBrowserPolicies(true, true, true);
                if (body.Contains("\"browserCleanCache\":true"))
                    ChromeOptimizer.CleanBrowserCaches();
                if (body.Contains("\"classicContextMenu\":true")) RegistryManager.SetClassicContextMenu(true);

                LogEngine.SetProgress(88, "Debloat UWP...");
                bool sp = body.Contains("\"uwpSponsored\":true");
                bool asst = body.Contains("\"uwpAssistants\":true");
                bool widg = body.Contains("\"uwpWidgets\":true");
                bool depr = body.Contains("\"uwpDeprovision\":true");
                if (sp || asst || widg) UwpDebloater.RunDebloat(sp, asst, widg, depr);

                ExplorerVisualManager.RestartExplorer();
            }
            else
            {
                // Preset Standard (Equilibrado / Seguro)
                LogEngine.SetProgress(15, "Aplicando Preset Standard Recomendado...");
                ServicesOptimizer.ApplyPreset("safe");
                SupervisionManager.DisableAllSupervisionAndBackground();
                ScheduledTasksManager.ApplyAll(true);
                DiskManager.EnsureTrimEnabled();
                DiskManager.DeepCleanTemps();
                RegistryManager.ApplyPrivacyTweaks(true);
                RegistryManager.OptimizeSearchBar(true);
                RegistryManager.DisableAdsAndTracking(true);
                RegistryManager.ApplyBackgroundAppsPolicy(true);
                RegistryManager.SetClassicContextMenu(true);
                WindowsUpdateManager.SetSecurityMode(true);
                WindowsUpdateManager.SetDeliveryOptimizationP2P(true);
                NetworkBlockManager.ToggleHostsBlock(true);
                ChromeOptimizer.ApplyBrowserPolicies(true, true, true);
                ChromeOptimizer.CleanBrowserCaches();
                UwpDebloater.RunDebloat(true, false, false, false);
                ExplorerVisualManager.RestartExplorer();
            }

            LogEngine.SetProgress(100, "Todas as otimizações foram concluídas com sucesso!");
            LogEngine.IsExecuting = false;
        }

        private void HandleRestorePoint(HttpListenerResponse res)
        {
            string err;
            bool ok = SafetyManager.CreateRestorePoint("DebloaterPC_ManualCheckpoint", out err);
            SendJson(res, string.Format("{{\"success\":{0},\"name\":\"DebloaterPC_ManualCheckpoint\",\"error\":\"{1}\"}}",
                ok ? "true" : "false", EscapeJson(err)));
        }

        private void HandleRollback(HttpListenerResponse res)
        {
            ThreadPool.QueueUserWorkItem((s) => {
                LogEngine.IsExecuting = true;
                SafetyManager.ApplyRollback();
                LogEngine.IsExecuting = false;
            });
            SendJson(res, "{\"success\":true,\"message\":\"Modo Reversão disparado.\"}");
        }

        private void HandleIntegrityCheck(HttpListenerResponse res)
        {
            ThreadPool.QueueUserWorkItem((s) => {
                LogEngine.IsExecuting = true;
                SafetyManager.RunIntegrityRepair();
                LogEngine.IsExecuting = false;
            });
            SendJson(res, "{\"success\":true,\"message\":\"Verificação de Integridade SFC e DISM iniciada.\"}");
        }

        private void HandleStartupGet(HttpListenerResponse res)
        {
            var items = StartupManager.GetStartupItems();
            var sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                sb.Append(string.Format("{{\"name\":\"{0}\",\"command\":\"{1}\",\"location\":\"{2}\",\"enabled\":{3}}}",
                    EscapeJson(it.Name), EscapeJson(it.Command), EscapeJson(it.Location), it.Enabled ? "true" : "false"));
                if (i < items.Count - 1) sb.Append(",");
            }
            sb.Append("]");
            SendJson(res, sb.ToString());
        }

        private void HandleStartupToggle(HttpListenerRequest req, HttpListenerResponse res)
        {
            string body = GetRequestBody(req);
            string name = ExtractJsonField(body, "name");
            string loc = ExtractJsonField(body, "location");
            bool ok = StartupManager.ToggleItem(name, loc);
            SendJson(res, string.Format("{{\"success\":{0}}}", ok ? "true" : "false"));
        }

        private void HandleWingetInstall(HttpListenerRequest req, HttpListenerResponse res)
        {
            string body = GetRequestBody(req);
            string pkg = ExtractJsonField(body, "packageId");
            ThreadPool.QueueUserWorkItem((s) => {
                LogEngine.IsExecuting = true;
                WinGetInstaller.Install(pkg);
                LogEngine.IsExecuting = false;
            });
            SendJson(res, "{\"success\":true,\"message\":\"Instalação iniciada via WinGet.\"}");
        }

        private void HandleUninstallStubborn(HttpListenerRequest req, HttpListenerResponse res)
        {
            string body = GetRequestBody(req);
            ThreadPool.QueueUserWorkItem((s) => {
                LogEngine.IsExecuting = true;
                LogEngine.SetProgress(5, "Iniciando remoção de aplicativos inamovíveis...");

                if (body.Contains("\"removeEdge\":true"))
                {
                    LogEngine.SetProgress(20, "Removendo Microsoft Edge...");
                    StubbornAppsManager.RemoveMicrosoftEdge();
                }

                if (body.Contains("\"removeOneDrive\":true"))
                {
                    LogEngine.SetProgress(40, "Removendo Microsoft OneDrive...");
                    StubbornAppsManager.RemoveOneDrive();
                }

                if (body.Contains("\"removeCopilot\":true"))
                {
                    LogEngine.SetProgress(60, "Removendo Copilot e IA...");
                    StubbornAppsManager.RemoveCopilotDeep();
                }

                if (body.Contains("\"removeYourPhone\":true"))
                {
                    LogEngine.SetProgress(75, "Removendo Vincular ao Celular...");
                    StubbornAppsManager.RemoveYourPhone();
                }

                if (body.Contains("\"removeXbox\":true"))
                {
                    LogEngine.SetProgress(85, "Removendo Xbox Game Bar...");
                    StubbornAppsManager.RemoveXboxOverlays();
                }

                if (body.Contains("\"removeQuickAssist\":true"))
                {
                    LogEngine.SetProgress(95, "Removendo Dicas e Assistência Rápida...");
                    StubbornAppsManager.RemoveQuickAssistAndTips();
                }

                ExplorerVisualManager.RestartExplorer();
                LogEngine.SetProgress(100, "Remoção de inamovíveis concluída com sucesso!");
                LogEngine.IsExecuting = false;
            });
            SendJson(res, "{\"success\":true,\"message\":\"Remoção disparada em segundo plano.\"}");
        }

        private string GetRequestBody(HttpListenerRequest req)
        {
            using (var reader = new StreamReader(req.InputStream, req.ContentEncoding))
            {
                return reader.ReadToEnd();
            }
        }

        private void SendJson(HttpListenerResponse res, string json)
        {
            res.ContentType = "application/json; charset=utf-8";
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            res.ContentLength64 = bytes.Length;
            res.OutputStream.Write(bytes, 0, bytes.Length);
            res.Close();
        }

        private string EscapeJson(string s)
        {
            if (s == null) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "\\n");
        }

        private string ExtractJsonField(string json, string field)
        {
            try
            {
                string search = "\"" + field + "\":\"";
                int idx = json.IndexOf(search);
                if (idx != -1)
                {
                    int start = idx + search.Length;
                    int end = json.IndexOf("\"", start);
                    if (end != -1) return json.Substring(start, end - start);
                }
            }
            catch {}
            return "";
        }
    }
}
