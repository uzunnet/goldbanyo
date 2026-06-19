#!/bin/bash
# DB her deploy'da guncelle — yedek al, image'daki DB'yi volume'a kopyala

DB_MARKER="/app/Veri/.db_initialized"
DB_HEDEF="/app/Veri/desadoor_v2.db"
DB_KAYNAK="/app/desadoor.db"

mkdir -p /app/Veri

if [ -f "$DB_MARKER" ]; then
  # Eski DB'yi yedekle
  if [ -f "$DB_HEDEF" ]; then
    BACKUP="/app/Veri/desadoor_v2_backup_$(date +%Y%m%d_%H%M%S).db"
    cp "$DB_HEDEF" "$BACKUP"
    echo "[entrypoint] Eski DB yedeklendi: $BACKUP"
  fi
fi

# Her zaman image'daki DB'yi kopyala
if [ -f "$DB_KAYNAK" ]; then
  cp "$DB_KAYNAK" "$DB_HEDEF"
  touch "$DB_MARKER"
  echo "[entrypoint] DB guncellendi: $DB_KAYNAK -> $DB_HEDEF"
else
  echo "[entrypoint] HATA: Kaynak DB bulunamadi: $DB_KAYNAK"
fi

exec dotnet Desadoor.Api.dll
