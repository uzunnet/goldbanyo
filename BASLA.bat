@echo off
echo === Desadoor Baslatiliyor ===

echo [1/4] Temizlik...
if exist "Desadoor.UI\wwwroot\_framework" rmdir /s /q "Desadoor.UI\wwwroot\_framework"

echo [2/4] Derleniyor...
dotnet build --no-restore
if %ERRORLEVEL% neq 0 (
    echo HATA: Derleme basarisiz!
    pause
    exit /b 1
)

echo [3/4] _framework kopyalaniyor...
xcopy /E /Y /I "Desadoor.UI\bin\Debug\net10.0\wwwroot\_framework" "Desadoor.UI\wwwroot\_framework" >nul

echo [4/4] Sunucular baslatiliyor...
start "Desadoor API" dotnet run --no-build --project Desadoor.Api\Desadoor.Api.csproj
start "Desadoor UI" dotnet run --no-build --project Desadoor.UI\Desadoor.UI.csproj

echo.
echo === Hazir! ===
echo   API : http://localhost:5015
echo   UI  : http://localhost:5013
echo.
pause
