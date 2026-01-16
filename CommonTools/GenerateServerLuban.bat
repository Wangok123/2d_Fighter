@echo off
chcp 65001 >nul
setlocal

echo ========================================
echo Generate Server Luban Configuration
echo ========================================

set ROOT_DIR=%~dp0..
set PUBLIC_DIR=%ROOT_DIR%\Public

if not exist "%PUBLIC_DIR%\gen_server.bat" (
    echo Error: gen_server.bat not found at %PUBLIC_DIR%
    pause
    exit /b 1
)

echo Running gen_server.bat in %PUBLIC_DIR%...
cd /d "%PUBLIC_DIR%"
call gen_server.bat

if errorlevel 1 (
    echo Error: Failed to generate Server Luban configuration
    pause
    exit /b 1
)

echo.
echo ========================================
echo Server Luban configuration generated successfully!
echo ========================================
pause
