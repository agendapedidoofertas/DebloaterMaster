# ⚡ Debloater Master & Otimizador de PC Pro (Windows 10 & 11)

> **Debloater e otimizador de sistema ultra-leve (~160 KB), com interface webapp nativa e controle total de privacidade, serviços e desempenho.**

Inspirado nas soluções consagradas de otimização de mercado (**Chris Titus Tech / WinUtil**, **Decodificando TI** e **Coisa de Nerd**), o **Debloater Master** foi desenhado com arquitetura híbrida ultra-eficiente: backend nativo em C# (.NET 4.8 / Win32) e micro-servidor HTTP embutido que renderiza a interface diretamente em modo App no Microsoft Edge / WebView2, consumindo o mínimo de memória RAM e sem janelas de terminal abertas.

---

## 🚀 Principais Recursos

### 1. 🛡️ Essencial & Chris Titus Debloat
- **Telemetria & Diagnósticos**: Desativação completa do DiagTrack, relatórios de erro e envio de dados em segundo plano.
- **Pesquisa Limpa**: Remoção de anúncios da web, destaques do Bing e IA da caixa de pesquisa do Iniciar.
- **Supervisão & Keylogger**: Bloqueio de coleta de dados de digitação/escrita, contatos e finalização do `CompatTelRunner`.
- **Censo de Dispositivos**: Encerramento e bloqueio das tarefas agendadas do `DeviceCensus`.
- **Apps em Segundo Plano**: Suspensão de processos UWP ociosos consumindo CPU e memória.

### 2. 🎮 Modo Gamer & Latência Ultrabaixa
- **Network Throttling Index (`0xFFFFFFFF`)**: Eliminação de gargalos artificiais de pacotes de rede do Windows.
- **System Responsiveness (0%)**: Alocação de 100% de prioridade de CPU para aplicativos em primeiro plano.
- **Mira 1:1 Perfeita**: Desativação total da aceleração de ponteiro do mouse para precisão competitiva.
- **Xbox GameDVR**: Desativação de gravação em segundo plano liberando VRAM.
- **Plano de Desempenho Máximo**: Ativação nativa de perfil de alto rendimento.

### 3. ⚙️ Controle de Serviços Cirúrgico (24 Serviços Mapeados)
- **Interruptores Físicos Basculantes (`|` Ativo / `O` Desativado)** com relevo visual de precisão.
- Perfis rápidos de configuração: **Perfil Seguro**, **Perfil Gamer Máximo** e **Restaurar Padrões**.
- Gestão individual de Telemetria, Experiência Conectada, Biometria, Roteamento e Serviços de Diagnóstico.

### 4. 🧹 Limpeza de Disco & Otimização de RAM
- **Limpeza Profunda de Cache**: Remoção segura de temporários (`%TEMP%`, `Windows\Temp`, `Prefetch`).
- **WorkingSet Trimmer**: Esvaziamento instantâneo de páginas de RAM retidas por processos inativos.
- **TRIM para SSDs**: Verificação e ativação forçada do comando TRIM em unidades NTFS.
- **Desativação de Hibernação**: Liberação instantânea de 4 GB a 16 GB no disco C: (`powercfg -h off`).

### 5. 📦 Desbloat de Aplicativos UWP & Microsoft Edge/OneDrive
- Remoção seletiva ou em lote de bloatwares pré-instalados (mantendo intactas Loja, Calculadora e Bloco de Notas).
- Script seguro de desinstalação profunda do **Microsoft Edge** e **OneDrive**.
- Recurso de emergência para **Ressuscitar a Microsoft Store** via manifesto oficial.

### 6. 🖥️ Explorer & Windows 11 Moderno
- Menu de Contexto Clássico (estilo Windows 10 no Windows 11 sem 'Mostrar mais opções').
- Exibição padrão de extensões de arquivo e arquivos ocultos.
- Atalho do Iniciar direcionado para **'Este Computador'**.
- **Desativação do Windows Recall AI** (gravação periódica de telas no Win 11 24H2+).
- **Bypass de Requisitos LabConfig** (TPM 2.0, SecureBoot e CPU não suportadas).

---

## 🛠️ Tecnologias Utilizadas

- **Linguagem Backend**: C# (.NET Framework 4.8 / Roslyn CSC nativo do Windows).
- **Frontend**: HTML5, CSS3 moderno (paleta corporativa escura `#24272C`), JavaScript Vanilla puro (zero dependências ou frameworks pesados).
- **Modo de Exibição**: Microsoft Edge App Mode (`msedge.exe --app=http://127.0.0.1:13580`), reaproveitando o motor web do próprio sistema.
- **Peso do Executável**: Apenas ~160 KB.

---

## 💻 Como Compilar e Executar

### Pré-requisitos
Windows 10 ou Windows 11 (funciona nativamente sem instalar nenhum SDK ou compilador externo).

### 1. Compilar com 1 Clique
Execute o script PowerShell nativo:
```powershell
powershell -ExecutionPolicy Bypass -File build.ps1
```
O executável compilado `DebloaterMaster.exe` será gerado imediatamente na pasta raiz.

### 2. Abrir o Aplicativo
- Dê dois cliques em `1-CLIQUE_ABRIR_AQUI.bat` ou em `DebloaterMaster.exe`.
- Conceda permissão de Administrador quando solicitado pelo Windows UAC para permitir a aplicação dos ajustes de registro e serviços.
