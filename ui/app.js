// ====================================================================
// DEBLOATER MASTER - CLIENT-SIDE ENGINE (VANILLA JS ULTRA-LEVE)
// Comunicação assíncrona pura sem bibliotecas externas
// ====================================================================

let logOffset = 0;
let isPollingLogs = true;

// Inicialização imediata
document.addEventListener('DOMContentLoaded', () => {
  fetchMetrics();
  setInterval(fetchMetrics, 2500);

  fetchLogs();
  setInterval(fetchLogs, 1500);

  // Heartbeat contínuo para manter o servidor ativo enquanto a janela estiver aberta
  setInterval(() => {
    fetch('/api/ping').catch(() => {});
  }, 2000);

  loadAuditData();
  loadServicesList();
});

// Navegação por Abas
function switchTab(tabId) {
  document.querySelectorAll('.tab-btn').forEach(btn => btn.classList.remove('active'));
  document.querySelectorAll('.tab-content').forEach(tab => tab.classList.remove('active'));

  const activeContent = document.getElementById(tabId);
  if (activeContent) activeContent.classList.add('active');

  const activeBtn = Array.from(document.querySelectorAll('.tab-btn')).find(b => 
    b.getAttribute('onclick') && b.getAttribute('onclick').includes(tabId)
  );
  if (activeBtn) activeBtn.classList.add('active');
}

// Métricas de Hardware e Sistema em Tempo Real
async function fetchMetrics() {
  try {
    const res = await fetch('/api/diagnostics');
    if (!res.ok) return;
    const data = await res.json();

    const cpuEl = document.getElementById('metric-cpu');
    if (cpuEl) cpuEl.innerText = `${data.cpuUsage}%`;

    const ramEl = document.getElementById('metric-ram');
    if (ramEl) ramEl.innerText = `${data.ramUsedGB.toFixed(1)} / ${data.ramTotalGB.toFixed(1)} GB (${data.ramUsagePercent}%)`;

    const diskEl = document.getElementById('metric-disk');
    if (diskEl) diskEl.innerText = `${data.diskFreeGB.toFixed(0)} GB Livres`;

    const osEl = document.getElementById('metric-os');
    if (osEl) osEl.innerText = `${data.os || 'Windows'}`;

    const adminEl = document.getElementById('admin-badge');
    if (adminEl) {
      if (data.isAdmin) {
        adminEl.innerHTML = '<span class="badge-dot" style="background-color: #10b981;"></span> ADMINISTRADOR ATIVO';
        adminEl.style.cursor = 'default';
        adminEl.onclick = null;
      } else {
        adminEl.innerHTML = '<span class="badge-dot" style="background-color: #ef4444;"></span> CLIQUE PARA ELEVAR (ADMIN)';
        adminEl.style.cursor = 'pointer';
        adminEl.onclick = async () => {
          await postApi('/api/restart-elevated');
        };
      }
    }
  } catch (err) {
    // Silencioso se o servidor estiver temporariamente ocupado
  }
}

// Feed de Logs do Console em Tempo Real
async function fetchLogs() {
  if (!isPollingLogs) return;
  try {
    const res = await fetch(`/api/logs?offset=${logOffset}`);
    if (!res.ok) return;
    const data = await res.json();

    if (data.lines && data.lines.length > 0) {
      const consoleBox = document.getElementById('console-output');
      data.lines.forEach(entry => {
        const div = document.createElement('div');
        div.className = `log-line ${entry.type || 'info'}`;
        div.innerText = entry.text;
        consoleBox.appendChild(div);
      });
      consoleBox.scrollTop = consoleBox.scrollHeight;
    }
    logOffset = data.nextOffset || logOffset;
  } catch (err) {}
}

function clearConsole() {
  const consoleBox = document.getElementById('console-output');
  if (consoleBox) consoleBox.innerHTML = '';
}

// Helper genérico para chamadas POST
async function postApi(endpoint, bodyObj = {}) {
  try {
    const res = await fetch(endpoint, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(bodyObj)
    });
    return await res.json();
  } catch (err) {
    console.error('Erro na chamada API:', endpoint, err);
    return { success: false, error: err.message };
  }
}

// 🛡️ Segurança: Ponto de Restauração & Rollback
async function createRestorePoint() {
  appendLocalLog('[AÇÃO] Solicitando criação de Ponto de Restauração...', 'info');
  const data = await postApi('/api/restore-point');
  if (data && data.success) {
    appendLocalLog('[SUCESSO] Ponto de Restauração criado com sucesso no Windows!', 'success');
  } else {
    appendLocalLog(`[AVISO] Falha ao criar ponto: ${data.error || 'Verifique permissões'}`, 'warning');
  }
}

// 🛑 Supervisão e Monitoramento de Segundo Plano
async function disableSupervisionAll() {
  appendLocalLog('[AÇÃO] Desativando todos os programas de supervisão, censo e tarefas de fundo...', 'warning');
  const res = await postApi('/api/supervision-disable');
  if (res && res.success) {
    appendLocalLog('[SUCESSO] Supervisão de digitação, hardware e processos em 2º plano bloqueados!', 'success');
  }
}

async function killSupervisionNow() {
  appendLocalLog('[AÇÃO] Encerrando processos de supervisão ativos em memória...', 'info');
  const res = await postApi('/api/supervision-kill');
  if (res && res.success) {
    appendLocalLog('[SUCESSO] Processos de supervisão (CompatTel, DeviceCensus, etc.) finalizados!', 'success');
  }
}

async function toggleSupervision(isChecked) {
  updateStatusBadge('tweak-supervision', isChecked);
  if (isChecked) {
    await disableSupervisionAll();
  } else {
    appendLocalLog('[AÇÃO] Bloqueio de supervisão desligado.', 'info');
  }
}

async function toggleCensus(isChecked) {
  updateStatusBadge('tweak-census', isChecked);
  if (isChecked) {
    await killSupervisionNow();
  } else {
    appendLocalLog('[AÇÃO] Censo de dispositivos desligado.', 'info');
  }
}

async function togglePowerPlan(isChecked) {
  updateStatusBadge('gamer-powerplan', isChecked);
  if (isChecked) {
    await applyPowerPlan();
  } else {
    appendLocalLog('[AÇÃO] Revertendo plano de energia para padrão...', 'info');
  }
}

async function rollbackDefaults() {
  if (!confirm('Deseja realmente reverter as principais configurações do Windows para os padrões de fábrica?')) return;
  appendLocalLog('[AÇÃO] Iniciando reversão completa de configurações...', 'warning');
  await postApi('/api/rollback');
}

// ⚡ Presets de Otimização Geral
async function applyPreset(type) {
  appendLocalLog(`[AÇÃO] Aplicando Preset Master (${type})...`, 'info');
  await postApi('/api/optimize', { preset: type });
}

// Tweaks Individuais de Privacidade e Essenciais
async function toggleTweak(tweakKey, isChecked) {
  updateStatusBadge('tweak-' + tweakKey, isChecked);
  const mapping = {
    'telemetry': 'tweak_block_telemetry',
    'bing': 'tweak_optimize_search',
    'bgapps': 'tweak_disable_background_apps',
    'ads': 'tweak_disable_ads_tracking',
    'activity': 'tweak_disable_activity_feed',
    'consumer': 'tweak_disable_ads_tracking'
  };

  const id = mapping[tweakKey] || tweakKey;
  await postApi('/api/tweak-toggle', { id: id, enable: isChecked });
}

// 🎮 Modo Gamer e Latência
async function applyGamerPreset() {
  appendLocalLog('[AÇÃO] Aplicando modo Gamer Completo (Baixa Latência, Prioridade GPU e Mouse 1:1)...', 'info');
  await postApi('/api/gamer-optimize', { enable: true });
  setSwitch('gamer-net', true);
  setSwitch('gamer-resp', true);
  setSwitch('gamer-mouse', true);
  setSwitch('gamer-gamedvr', true);
  setSwitch('gamer-powerplan', true);
}

async function toggleGamerTweak(type, isChecked) {
  const badgeId = (type === 'network') ? 'gamer-net' : ((type === 'responsiveness') ? 'gamer-resp' : ('gamer-' + type));
  updateStatusBadge(badgeId, isChecked);
  if (type === 'gamedvr') {
    await postApi('/api/tweak-toggle', { id: 'tweak_disable_gamedvr', enable: isChecked });
  } else {
    await postApi('/api/gamer-optimize', { action: type, enable: isChecked });
  }
}

async function applyPowerPlan() {
  appendLocalLog('[AÇÃO] Ativando Plano de Energia de Desempenho Máximo...', 'info');
  await postApi('/api/tweak-toggle', { id: 'tweak_high_performance', enable: true });
}

async function flushDns() {
  appendLocalLog('[AÇÃO] Limpando cache do resolvedor DNS (ipconfig /flushdns)...', 'info');
  await postApi('/api/tweak-toggle', { id: 'tweak_clean_temps', enable: true });
}

// 🛠️ Serviços do Windows
async function loadServicesList() {
  try {
    const res = await fetch('/api/services-catalog');
    if (!res.ok) return;
    const catalog = await res.json();
    const container = document.getElementById('services-list');
    if (!container) return;

    container.innerHTML = '';
    catalog.forEach(svc => {
      const item = document.createElement('div');
      item.className = 'service-item';

      const isOff = !!svc.disabled; // true = desativado (O), false = ativo (I)
      const serviceLabel = svc.displayName || svc.serviceName;
      const safeName = svc.serviceName.replace(/[^a-zA-Z0-9]/g, '_');

      item.innerHTML = `
        <div class="service-meta">
          <span class="service-name">${escapeHtml(serviceLabel)}</span>
          <span class="service-desc">${escapeHtml(svc.description || svc.serviceName)}</span>
        </div>
        <div class="tweak-controls">
          <span class="badge-status ${isOff ? 'is-inactive' : 'is-active'}" id="status-svc-${safeName}">${isOff ? 'Inativo' : 'Ativo'}</span>
          <button class="badge-rec" type="button" title="Clique para aplicar a melhor configuração de desempenho (Desativar)" onclick="applyRec('svc-toggle-${safeName}', false)">REC</button>
          <label class="rocker-switch-box" title="${isOff ? 'Clique para Ativar este serviço' : 'Clique para Desativar este serviço'}">
            <input type="checkbox" id="svc-toggle-${safeName}" ${isOff ? '' : 'checked'} onchange="toggleServiceItem('${svc.serviceName}', !this.checked, 'svc-${safeName}')">
            <div class="rocker-bezel">
              <div class="rocker-switch">
                <span class="rocker-half sym-i">|</span>
                <span class="rocker-half sym-o">O</span>
              </div>
            </div>
          </label>
        </div>
      `;
      container.appendChild(item);
    });
  } catch (err) {}
}

async function toggleServiceItem(serviceName, shouldDisable, badgeKey) {
  if (badgeKey) updateStatusBadge(badgeKey, !shouldDisable);
  appendLocalLog(`[SERVIÇOS] ${shouldDisable ? 'Desativando' : 'Ativando'} serviço '${serviceName}'...`, 'info');
  await postApi('/api/services-toggle', { serviceName: serviceName, disable: shouldDisable });
  await loadServicesList();
}

async function applyServicesPreset(presetType) {
  appendLocalLog(`[AÇÃO] Aplicando perfil de serviços: ${presetType.toUpperCase()}...`, 'info');
  await postApi('/api/services-preset', { preset: presetType });
  setTimeout(loadServicesList, 1500);
}

// 🧹 Limpeza de Disco & RAM
async function cleanDiskDeep() {
  appendLocalLog('[AÇÃO] Iniciando limpeza profunda de temporários e caches...', 'info');
  await postApi('/api/tweak-toggle', { id: 'tweak_clean_temps', enable: true });
}

async function cleanRamNow() {
  appendLocalLog('[AÇÃO] Esvaziando WorkingSets de memória RAM...', 'info');
  await postApi('/api/tweak-toggle', { id: 'tweak_clean_ram', enable: true });
}

async function ensureTrim() {
  appendLocalLog('[AÇÃO] Ativando e verificando suporte TRIM no sistema de arquivos NTFS...', 'info');
  await postApi('/api/tweak-toggle', { id: 'tweak_enable_trim', enable: true });
}

async function disableHibernation() {
  appendLocalLog('[AÇÃO] Desativando hibernação do Windows (powercfg -h off)...', 'info');
  await postApi('/api/tweak-toggle', { id: 'tweak_disable_hibernation', enable: true });
}

// 🗑️ Desbloat UWP & Apps Inúteis
async function removeSponsoredApps() {
  if (!confirm('Deseja desinstalar aplicativos patrocinados de fábrica (Candy Crush, Disney, TikTok, Spotify pré-instalado)?')) return;
  appendLocalLog('[AÇÃO] Removendo aplicativos patrocinados...', 'info');
  await postApi('/api/apps-debloat-batch', { category: 'sponsored' });
}

async function removeAssistants() {
  if (!confirm('Deseja remover Cortana, Hub de Feedback e Dicas do Windows?')) return;
  appendLocalLog('[AÇÃO] Removendo assistentes de telemetria...', 'info');
  await postApi('/api/apps-debloat-batch', { category: 'assistant' });
}

async function removeEdge() {
  if (!confirm('ATENÇÃO: Deseja desinstalar o Microsoft Edge completamente do sistema? Certifique-se de possuir outro navegador instalado.')) return;
  appendLocalLog('[AÇÃO] Executando desinstalação profunda do Microsoft Edge...', 'warning');
  await postApi('/api/uninstall-stubborn', { removeEdge: true });
}

async function removeOneDrive() {
  if (!confirm('Deseja remover o Microsoft OneDrive do Windows?')) return;
  appendLocalLog('[AÇÃO] Desinstalando Microsoft OneDrive...', 'warning');
  await postApi('/api/uninstall-stubborn', { removeOneDrive: true });
}

async function resurrectStore() {
  appendLocalLog('[AÇÃO] Iniciando ressurreição e reparação da Microsoft Store...', 'info');
  await postApi('/api/apps-resurrect-store');
}

async function scanApps() {
  const container = document.getElementById('apps-list');
  if (container) container.innerHTML = '<div class="loading-state">Escanando pacotes UWP instalados no computador...</div>';

  try {
    const res = await fetch('/api/apps-scan');
    if (!res.ok) return;
    const apps = await res.json();

    if (!container) return;
    container.innerHTML = '';

    if (apps.length === 0) {
      container.innerHTML = '<div class="loading-state">Nenhum aplicativo dispensável encontrado.</div>';
      return;
    }

    apps.forEach(app => {
      const item = document.createElement('div');
      item.className = 'app-item';
      item.innerHTML = `
        <div class="app-meta">
          <span class="app-name">${escapeHtml(app.name)}</span>
          <span class="app-desc">Versão: ${escapeHtml(app.version)} ${app.sizeMb > 0 ? `| ${app.sizeMb} MB` : ''}</span>
        </div>
        <button class="btn btn-danger btn-small" onclick="removeSingleApp('${escapeHtml(app.packageFullName)}')">Remover</button>
      `;
      container.appendChild(item);
    });
  } catch (err) {
    if (container) container.innerHTML = '<div class="loading-state">Erro ao escanear aplicativos.</div>';
  }
}

async function removeSingleApp(pkgFullName) {
  appendLocalLog(`[AÇÃO] Removendo pacote: ${pkgFullName}...`, 'info');
  await postApi('/api/apps-remove-single', { packageName: pkgFullName, deprovision: true });
  setTimeout(scanApps, 2000);
}

// ✨ Explorer & Windows 11
async function toggleExplorer(actionType, isChecked) {
  updateStatusBadge('w11-' + actionType, isChecked);
  const mapping = {
    'classic-menu': 'classicMenu',
    'show-ext': 'showExt',
    'show-hidden': 'showHidden',
    'this-pc': 'thisPC'
  };

  const act = mapping[actionType] || actionType;
  await postApi('/api/explorer-tweak', { action: act, enable: isChecked });
}

async function restartExplorer() {
  appendLocalLog('[AÇÃO] Reiniciando processo explorer.exe para recarregar a interface...', 'info');
  await postApi('/api/explorer-tweak', { action: 'restart' });
}

async function toggleW11Recall(isChecked) {
  updateStatusBadge('w11-recall', isChecked);
  appendLocalLog(`[AÇÃO] Configurando Windows Recall AI: ${isChecked ? 'DESATIVAR' : 'ATIVAR'}...`, 'info');
  await postApi('/api/w11-modern-recall', { disable: isChecked });
}

async function toggleW11Bypass(isChecked) {
  updateStatusBadge('w11-bypass', isChecked);
  appendLocalLog(`[AÇÃO] Bypass de Hardware Windows 11 (LabConfig): ${isChecked ? 'ATIVAR' : 'DESATIVAR'}...`, 'info');
  await postApi('/api/w11-labconfig', { enable: isChecked });
}

// Carregar status real da auditoria do Windows (Leitura cirúrgica do Registro)
async function loadAuditData() {
  try {
    const res = await fetch('/api/audit');
    if (!res.ok) return;
    const a = await res.json();

    // 1. Essencial & Chris Titus
    setSwitch('tweak-telemetry', a.tweak_block_telemetry ?? a.telemetry_blocked);
    setSwitch('tweak-bing', a.tweak_optimize_search ?? a.search_optimized);
    setSwitch('tweak-bgapps', a.tweak_disable_background_apps ?? a.background_apps_disabled);
    setSwitch('tweak-ads', a.tweak_disable_ads_tracking ?? a.ads_disabled);
    setSwitch('tweak-activity', a.tweak_disable_activity_feed ?? a.activity_history_disabled);
    setSwitch('tweak-consumer', a.tweak_disable_ads_tracking ?? a.consumer_features_disabled);
    setSwitch('tweak-supervision', a.tweak_tasks_appraiser);
    setSwitch('tweak-census', a.tweak_tasks_appraiser);

    // 2. Gamer & Latência
    setSwitch('gamer-net', a.tweak_gamer_network_throttling);
    setSwitch('gamer-resp', a.tweak_gamer_network_throttling);
    setSwitch('gamer-mouse', a.tweak_gamer_mouse_1to1);
    setSwitch('gamer-gamedvr', a.tweak_disable_gamedvr);
    setSwitch('gamer-powerplan', a.tweak_high_performance);

    // 3. Explorer & Windows 11
    setSwitch('w11-classic-menu', a.tweak_classic_context_menu ?? a.classic_context_menu);
    setSwitch('w11-show-ext', a.tweak_show_file_extensions ?? a.show_file_extensions);
    setSwitch('w11-show-hidden', a.tweak_show_hidden_files ?? a.show_hidden_files);
    setSwitch('w11-this-pc', a.tweak_open_this_pc);
    setSwitch('w11-recall', a.tweak_w11_recall_disable);
    setSwitch('w11-bypass', a.tweak_w11_labconfig_bypass);
  } catch (err) {}
}

function updateStatusBadge(id, isChecked) {
  const el = document.getElementById('status-' + id);
  if (!el) return;
  if (isChecked) {
    el.innerText = 'Ativo';
    el.className = 'badge-status is-active';
  } else {
    el.innerText = 'Inativo';
    el.className = 'badge-status is-inactive';
  }
}

function applyRec(targetId, optimalValue = true) {
  const el = document.getElementById(targetId);
  if (!el) return;
  // O botão REC seleciona exclusivamente a melhor configuração recomendada.
  // Se já estiver na configuração recomendada, mantém inalterado (só desativa clicando no interruptor).
  if (el.checked === optimalValue) {
    appendLocalLog('[REC] A melhor configuração recomendada já está ativa para este item!', 'info');
    return;
  }
  el.checked = optimalValue;
  updateStatusBadge(targetId, optimalValue);
  el.dispatchEvent(new Event('change'));
}

function setSwitch(id, val) {
  const el = document.getElementById(id);
  if (el && val !== undefined) {
    el.checked = !!val;
    updateStatusBadge(id, !!val);
  }
}

function appendLocalLog(text, type = 'info') {
  const consoleBox = document.getElementById('console-output');
  if (!consoleBox) return;
  const div = document.createElement('div');
  div.className = `log-line ${type}`;
  div.innerText = text;
  consoleBox.appendChild(div);
  consoleBox.scrollTop = consoleBox.scrollHeight;
}

async function closeApp() {
  if (confirm('Deseja fechar o Debloater Master?')) {
    try { await postApi('/api/exit'); } catch {}
    window.close();
  }
}

function escapeHtml(str) {
  if (!str) return '';
  return str.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}
