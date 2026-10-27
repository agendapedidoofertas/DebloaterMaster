@echo off
title Criar Atalho na Area de Trabalho
cd /d "%~dp0"
echo Criando atalho na sua Area de Trabalho...

powershell -NoProfile -Command "$ws = New-Object -ComObject WScript.Shell; $desktop = [Environment]::GetFolderPath('Desktop'); $shortcut = $ws.CreateShortcut($desktop + '\Debloater Master.lnk'); $shortcut.TargetPath = Join-Path (Get-Location) 'DebloaterMaster.exe'; $shortcut.WorkingDirectory = (Get-Location).Path; $shortcut.Description = 'Debloater Master & Otimizador de PC Ultra-Leve'; $shortcut.Save()"

if %errorlevel% equ 0 (
    echo.
    echo ======================================================================
    echo  Sucesso! O atalho 'Debloater Master' foi criado na Area de Trabalho!
    echo ======================================================================
) else (
    echo.
    echo Nao foi possivel criar o atalho automaticamente.
)
echo.
pause
