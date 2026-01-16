@echo off
chcp 65001 >nul
setlocal

echo ========================================
echo Generate Luban Configuration
echo ========================================

set ROOT_DIR=%~dp0..
set PUBLIC_DIR=%ROOT_DIR%\Public

if not exist "%PUBLIC_DIR%\gen.bat" (
    echo Error: gen.bat not found at %PUBLIC_DIR%
    pause
    exit /b 1
)

echo Running gen.bat in %PUBLIC_DIR%...
cd /d "%PUBLIC_DIR%"
call gen.bat

if errorlevel 1 (
    echo Error: Failed to generate Luban configuration
    pause
    exit /b 1
)

echo.
echo ========================================
echo Luban configuration generated successfully!
echo ========================================
pause
