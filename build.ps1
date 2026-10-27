# Compilação Nativa Ultra-Rápida do DebloaterMaster (Sem Janela Preta - Target WinExe)
$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$cscPath = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
$outExe = Join-Path $scriptDir "DebloaterMaster.exe"
$manifestPath = Join-Path $scriptDir "app.manifest"
$srcPattern = Join-Path $scriptDir "src\*.cs"

Write-Host "Compilando DebloaterMaster.exe com Roslyn/CSC nativo do Windows..." -ForegroundColor Cyan

& $cscPath `
    /out:"$outExe" `
    /win32manifest:"$manifestPath" `
    /r:System.dll,System.Core.dll,System.Drawing.dll,System.Windows.Forms.dll,System.ServiceProcess.dll `
    /optimize+ `
    /target:winexe `
    "$srcPattern"

if ($LASTEXITCODE -eq 0) {
    $size = (Get-Item $outExe).Length / 1KB
    Write-Host "SUCESSO! Executável gerado: $outExe ($([Math]::Round($size, 1)) KB)" -ForegroundColor Green
} else {
    Write-Host "Falha na compilação. Código: $LASTEXITCODE" -ForegroundColor Red
}
