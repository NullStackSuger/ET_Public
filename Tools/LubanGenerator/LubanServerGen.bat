@echo off
set ROOT=..\..\
set LUBAN_DLL=Tools\Luban\Luban.dll
set LUBAN_CONF=luban.conf
set CUSTOM=Custom

set SERVER_CODE=%ROOT%DotNet\Model\Server\Config
set SERVER_BIN=%ROOT%Bin\Config\Luban\Binary\Server
set SERVER_JSON=%ROOT%Bin\Config\Luban\Json\Server

:: 服务端

dotnet %LUBAN_DLL% ^
--customTemplateDir %CUSTOM% ^
-t s ^
-c cs-bin ^
-d bin ^
-d json ^
--conf %LUBAN_CONF% ^
-x outputCodeDir=%SERVER_CODE% ^
-x bin.outputDataDir=%SERVER_BIN% ^
-x json.outputDataDir=%SERVER_JSON%

echo ==================== 服务端 完成 ====================