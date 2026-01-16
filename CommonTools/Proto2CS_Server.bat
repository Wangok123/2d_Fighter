@echo off
chcp 65001 >nul
setlocal

echo ========================================
echo Proto2CS Server Tool
echo ========================================

set ROOT_DIR=%~dp0..
set PROTO_DIR=%ROOT_DIR%\Protocal
set PROTOC=%PROTO_DIR%\protoc.exe
set PROTOCOL_DIR=%PROTO_DIR%\Proto
set OUTPUT_DIR=%ROOT_DIR%\Server\LatServer\LatProtocol\Protocol

if not exist "%PROTOC%" (
    echo Error: protoc.exe not found at %PROTOC%
    pause
    exit /b 1
)

if not exist "%OUTPUT_DIR%" (
    mkdir "%OUTPUT_DIR%"
) else (
    del /q "%OUTPUT_DIR%\*.*" 2>nul
)

echo Compiling proto files...
for /r "%PROTOCOL_DIR%" %%f in (*.proto) do (
    echo Compiling %%~nxf...
    "%PROTOC%" --csharp_out="%OUTPUT_DIR%" --proto_path="%PROTOCOL_DIR%" "%%~nxf"
    if errorlevel 1 (
        echo Error compiling %%~nxf
        pause
        exit /b 1
    )
)

echo.
echo Generating ProtocolID and ProtocolMapping...
dotnet run --project "%~dp0ProtoGenerator\ProtoGenerator.csproj" -- "%PROTOCOL_DIR%" "%ROOT_DIR%\Server\LatServer\LatProtocol"

if errorlevel 1 (
    echo Error generating protocol files
    pause
    exit /b 1
)

echo.
echo ========================================
echo Proto2CS completed successfully!
echo ========================================
pause
