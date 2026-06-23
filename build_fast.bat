@echo off
setlocal

:: Determine directories
set BASE_DIR=%~dp0
set OUTPUT_DIR=%BASE_DIR%output

echo [INFO] Cleaning old fast output directory...
if exist "%OUTPUT_DIR%" rmdir /s /q "%OUTPUT_DIR%"
mkdir "%OUTPUT_DIR%"

echo.
echo =======================================================
echo [INFO] Building Panel (FAST - No AOT)...
echo =======================================================
:: Build Panel without AOT
dotnet publish "%BASE_DIR%Panel\Panel.csproj" -c Release -o "%OUTPUT_DIR%" -p:PublishAot=false -p:DebugType=portable -p:DebugSymbols=true -v q -clp:ErrorsOnly -p:WarningLevel=0
if %ERRORLEVEL% neq 0 (
    echo [ERROR] Failed to build Panel!
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo =======================================================
echo [INFO] Building Client - NRO247Native (FAST - No AOT)...
echo =======================================================
:: Build Client without AOT
dotnet publish "%BASE_DIR%NRO247Native\NRO_v247.csproj" -c Release -o "%OUTPUT_DIR%" -p:PublishAot=false -p:DebugType=portable -p:DebugSymbols=true -v q -clp:ErrorsOnly -p:WarningLevel=0
if %ERRORLEVEL% neq 0 (
    echo [ERROR] Failed to build Client!
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo =======================================================
echo [INFO] Copying additional assets to output...
echo =======================================================
mkdir "%OUTPUT_DIR%\Data" 2>nul
if exist "%BASE_DIR%DataDownloader\Data\JSON\ItemTemplates.json" copy /Y "%BASE_DIR%DataDownloader\Data\JSON\ItemTemplates.json" "%OUTPUT_DIR%\Data\"
if exist "%BASE_DIR%DataDownloader\Data\JSON\Maps.json" copy /Y "%BASE_DIR%DataDownloader\Data\JSON\Maps.json" "%OUTPUT_DIR%\Data\"
if exist "%BASE_DIR%DataDownloader\Data\JSON\NClasses.json" copy /Y "%BASE_DIR%DataDownloader\Data\JSON\NClasses.json" "%OUTPUT_DIR%\Data\"
if exist "%BASE_DIR%NRO247Native\LoadAssets\assets.dat" copy /Y "%BASE_DIR%NRO247Native\LoadAssets\assets.dat" "%OUTPUT_DIR%\"

echo.
echo =======================================================
echo [INFO] Build completed successfully!
echo [INFO] Fast Executables (No AOT) are at: "%OUTPUT_DIR%"
echo [INFO] Note: Use this for quick testing ONLY.
echo =======================================================
echo.
pause
