#!/bin/bash
# DB dosyasi imaja gomulmez; uygulama kendi migration+seed mekanizmasiyla
# (Program.cs -> MigrateAsync + TohumVerisi.TohumlaAsync) volume'deki
# VeriTabani__Yol hedefinde DB'yi ilk acilista otomatik olusturur.
# Bu script sadece volume dizininin var oldugunu garanti eder.

mkdir -p /app/Veri

exec dotnet VizitLink3D.Api.dll
