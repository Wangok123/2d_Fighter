set WORKSPACE=.
set UNITYROOT=..\Client\Assets

echo ON

set LUBAN_DLL=%WORKSPACE%\Luban\Luban.dll
set CONF_ROOT=%WORKSPACE%

echo ON

dotnet %LUBAN_DLL% ^
    -t all ^
    -c cs-bin ^
    -d bin  ^
    --conf %CONF_ROOT%\luban.conf ^
    -x outputCodeDir=%UNITYROOT%\Scripts\Gen ^
    -x outputDataDir=%UNITYROOT%\GenerateDatas\bytes ^
	> output.log 2>&1

if %errorlevel% neq 0 (
    echo An error occurred. Check output.log for details.
    pause
)