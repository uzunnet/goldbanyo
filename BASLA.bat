@echo off
echo === VIZITLINK3D Baslatiliyor ===

echo [1/4] Temizlik...
if exist "VIZITLINK3D.UI\wwwroot\_framework" rmdir /s /q "VIZITLINK3D.UI\wwwroot\_framework"

echo [2/4] Derleniyor...
dotnet build --no-restore
if %ERRORLEVEL% neq 0 (
    echo HATA: Derleme basarisiz!
    pause
    exit /b 1
)

echo [3/4] _framework kopyalaniyor...
xcopy /E /Y /I "VIZITLINK3D.UI\bin\Debug\net10.0\wwwroot\_framework" "VIZITLINK3D.UI\wwwroot\_framework" >nul

echo [4/4] Sunucular baslatiliyor...
start "VIZITLINK3D API" dotnet run --no-build --project VIZITLINK3D.Api\VIZITLINK3D.Api.csproj
start "VIZITLINK3D UI" dotnet run --no-build --project VIZITLINK3D.UI\VIZITLINK3D.UI.csproj

echo.
echo === Hazir! ===
echo   API : http://localhost:5115
echo   UI  : http://localhost:5113
echo.
pause
