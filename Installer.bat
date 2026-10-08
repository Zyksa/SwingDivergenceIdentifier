@echo off
setlocal
title Swing ^& Divergence Identifier - Installation
if not exist "%~dp0scripts\Install-Indicator.ps1" (
    echo Dossier incomplet. Telechargez puis extrayez tout le ZIP GitHub.
    pause
    exit /b 1
)
powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\Install-Indicator.ps1" %*
exit /b %errorlevel%
