@echo off
chcp 65001 >nul
setlocal

echo ========================================
echo Override Protocol DLL Tool
echo ========================================

set ROOT_DIR=%~dp0..
set DLL_NAME=LatProtocol.dll
set SOURCE=%ROOT_DIR%\Server\LatServer\LatProtocol\bin\Release\netstandard2.1\%DLL_NAME%
set TARGET=%ROOT_DIR%\Client\Assets\Plugins\Tools\KCPNet\%DLL_NAME%

if not exist "%SOURCE%" (
    echo Error: Source DLL not found at %SOURCE%
    echo Please build the LatProtocol project first.
    pause
    exit /b 1
)

if not exist "%TARGET%" (
    echo Warning: Target path does not exist: %TARGET%
    set /p CREATE_DIR="Create target directory? (Y/N): "
    if /i "%CREATE_DIR%"=="Y" (
        mkdir "%ROOT_DIR%\Client\Assets\Plugins\Tools\KCPNet" 2>nul
    ) else (
        echo Operation cancelled.
        pause
        exit /b 1
    )
)

echo.
echo Copying DLL...
echo From: %SOURCE%
echo To: %TARGET%
copy /y "%SOURCE%" "%TARGET%"

if errorlevel 1 (
    echo Error: Failed to copy DLL file
    pause
    exit /b 1
)

echo.
echo ========================================
echo DLL override completed successfully!
echo ========================================
pause
