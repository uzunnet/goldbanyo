#!/bin/bash
# DB dosyasi imaja gomulmez; uygulama kendi migration+seed mekanizmasiyla
# (Program.cs -> MigrateAsync + TohumVerisi.TohumlaAsync) volume'deki
# VeriTabani__Yol hedefinde DB'yi ilk acilista otomatik olusturur.
# Bu script sadece volume dizininin var oldugunu garanti eder.

mkdir -p /app/Veri

# Coolify kalici depolamasi /app/wwwroot/medya uzerine baglandiginda
# Docker imajindaki katalog dosyalari gorunmez olur. Imaj olusturulurken
# /app/medya-init altina alinan dosyalardan yalnizca eksik olanlari tamamla;
# panelden yuklenen mevcut dosyalarin uzerine yazma.
mkdir -p /app/wwwroot/medya
# Runtime crash'i onlemek icin gerekli alt medya klasorlerini garanti et
mkdir -p /app/wwwroot/medya/urunler
mkdir -p /app/wwwroot/medya/gold-katalog
mkdir -p /app/wwwroot/medya/projeler
mkdir -p /app/wwwroot/medya/slaytlar
mkdir -p /app/wwwroot/medya/anasayfa
if [ -d /app/medya-init ]; then
    cp -rn /app/medya-init/. /app/wwwroot/medya/
fi

exec dotnet VizitLink3D.Api.dll
