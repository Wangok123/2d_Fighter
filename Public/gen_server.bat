set WORKSPACE=.
set SERVERROOT=..\Server\LatServer\LatServer

echo ON

set LUBAN_DLL=%WORKSPACE%\Luban\Luban.dll
set CONF_ROOT=%WORKSPACE%

echo ON

dotnet %LUBAN_DLL% ^
    -t all ^
    -c cs-bin ^
    -d bin  ^
    --conf %CONF_ROOT%\luban.conf ^
    -x outputCodeDir=%SERVERROOT%\Core\Gen ^
    -x outputDataDir=%SERVERROOT%\bin\Release\net8.0\GenerateDatas\bytes ^
	> output.log 2>&1

if %errorlevel% neq 0 (
    echo An error occurred. Check output.log for details.
    pause
)