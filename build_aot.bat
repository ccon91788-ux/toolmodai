@echo off
setlocal

:: Determine directories
set BASE_DIR=%~dp0
set OUTPUT_DIR=%BASE_DIR%output

echo [INFO] Cleaning old output directory...
if exist "%OUTPUT_DIR%" rmdir /s /q "%OUTPUT_DIR%"
mkdir "%OUTPUT_DIR%"

echo.
echo =======================================================
echo [INFO] Building Panel (Native AOT win-x86)...
echo =======================================================
:: Publish Panel as Native AOT
dotnet publish "%BASE_DIR%Panel\Panel.csproj" -c Release -r win-x86 -o "%OUTPUT_DIR%" -p:PublishAot=true -v q -clp:ErrorsOnly -p:WarningLevel=0
if %ERRORLEVEL% neq 0 (
    echo [ERROR] Failed to build Panel AOT!
    echo [TIP] Make sure Visual Studio with "Desktop development with C++" is installed.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo =======================================================
echo [INFO] Building Client - NRO247Native (Native AOT win-x86)...
echo =======================================================
:: Publish Client as Native AOT
dotnet publish "%BASE_DIR%NRO247Native\NRO_v247.csproj" -c Release -r win-x86 -o "%OUTPUT_DIR%" -p:PublishAot=true -v q -clp:ErrorsOnly -p:WarningLevel=0
if %ERRORLEVEL% neq 0 (
    echo [ERROR] Failed to build Client AOT!
    echo [TIP] Make sure Visual Studio with "Desktop development with C++" is installed.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo =======================================================
echo [INFO] Copying additional assets to output...
echo =======================================================
mkdir "%OUTPUT_DIR%\Data" 2>nul
copy /Y "%BASE_DIR%DataDownloader\Data\JSON\ItemTemplates.json" "%OUTPUT_DIR%\Data\"
copy /Y "%BASE_DIR%DataDownloader\Data\JSON\Maps.json" "%OUTPUT_DIR%\Data\"
copy /Y "%BASE_DIR%DataDownloader\Data\JSON\NClasses.json" "%OUTPUT_DIR%\Data\"
copy /Y "%BASE_DIR%NRO247Native\LoadAssets\assets.dat" "%OUTPUT_DIR%\"

echo.
echo =======================================================
echo [INFO] Computing integrity hash for NRO_v247.exe...
echo =======================================================
:: Tính SHA256 hash của NRO_v247.exe → hiển thị để admin paste lên web
for /f "delims=" %%i in ('powershell -command "(Get-FileHash -Path '%OUTPUT_DIR%\NRO_v247.exe' -Algorithm SHA256).Hash.ToLower()"') do (
    set "EXE_HASH=%%i"
)
:hash_done

echo.
echo =======================================================
echo [INFO] Build completed successfully!
echo [INFO] AOT Executables are at: "%OUTPUT_DIR%"
echo =======================================================
echo.
echo -------------------------------------------------------
echo [IMPORTANT] SHA256 Hash cua NRO_v247.exe:
echo.
echo   %EXE_HASH%
echo.
echo Hay copy hash nay va paste len trang Admin web:
echo   https://zefoxtools.io.vn/admin
echo   Muc "Bao Mat EXE (Server-Side Integrity)"
echo -------------------------------------------------------
pause
