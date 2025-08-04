@echo off
set ROOT=..\..\
set LUBAN_DLL=Tools\Luban\Luban.dll
set LUBAN_CONF=luban.conf
set CUSTOM=Custom

set CLIENT_CODE=%ROOT%Unity\Model\Client\Config
set CLIENT_BIN=%ROOT%Bin\Config\Luban\Binary\Client
set CLIENT_JSON=%ROOT%Bin\Config\Luban\Json\Client

:: 客户端

dotnet %LUBAN_DLL% ^
--customTemplateDir %CUSTOM% ^
-t c ^
-c cs-bin ^
-d bin ^
-d json ^
--conf %LUBAN_CONF% ^
-x outputCodeDir=%CLIENT_CODE% ^
-x bin.outputDataDir=%CLIENT_BIN% ^
-x json.outputDataDir=%CLIENT_JSON%

echo ==================== 客户端 完成 ====================