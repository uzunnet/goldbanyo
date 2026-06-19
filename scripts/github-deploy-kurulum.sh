#!/bin/bash
# DesaDoor GitHub Actions Deploy Kurulum Scripti
# Bu scripti SUNUCUDA (SSH ile baglandiktan sonra) calistirin.

set -e

echo "=== GitHub Actions Deploy Anahtari Olusturuluyor ==="

# 1. SSH anahtar cifti olustur (parolasiz, GitHub Actions icin)
ssh-keygen -t ed25519 -C "github-actions-desadoor" -f ~/.ssh/github_actions_desadoor -N ""

echo ""
echo "=== SUNUCUYA YETKILI ANAHTAR EKLENIYOR ==="
cat ~/.ssh/github_actions_desadoor.pub >> ~/.ssh/authorized_keys
chmod 600 ~/.ssh/authorized_keys

echo ""
echo "================================================================"
echo "GitHub'a eklenecek OZEL ANAHTAR (SSH_PRIVATE_KEY secret'i):"
echo "----------------------------------------------------------------"
cat ~/.ssh/github_actions_desadoor
echo "----------------------------------------------------------------"
echo ""
echo "Simdi GitHub reponuzda:"
echo "  Settings → Secrets and variables → Actions → New repository secret"
echo ""
echo "Su 4 secret'i ekleyin:"
echo ""
echo "  SSH_HOST       = sunucu IP adresiniz veya alan adi (orn: 123.45.67.89)"
echo "  SSH_USER       = $(whoami)"
echo "  SSH_PORT       = 22"
echo "  DEPLOY_PATH    = $(pwd)"
echo "  SSH_PRIVATE_KEY = (yukaridaki ozel anahtarin tamami, BEGIN satirindan END satirina kadar)"
echo "================================================================"
