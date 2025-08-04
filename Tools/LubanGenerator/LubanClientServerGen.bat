@echo off
set ROOT=..\..\
set LUBAN_DLL=Tools\Luban\Luban.dll
set LUBAN_CONF=luban.conf
set CUSTOM=Custom

set CLIENTSERVER_CODE=%ROOT%Unity\Model\Share\Config
set CLIENTSERVER_BIN=%ROOT%Bin\Config\Luban\Binary\ClientServer
set CLIENTSERVER_JSON=%ROOT%Bin\Config\Luban\Json\ClientServer

:: 所有

dotnet %LUBAN_DLL% ^
--customTemplateDir %CUSTOM% ^
-t cs ^
-c cs-bin ^
-d bin ^
-d json ^
--conf %LUBAN_CONF% ^
-x outputCodeDir=%CLIENTSERVER_CODE% ^
-x bin.outputDataDir=%CLIENTSERVER_BIN% ^
-x json.outputDataDir=%CLIENTSERVER_JSON%

echo ==================== 所有 完成 ====================