#!/bin/sh

# Actualizar la lista de paquetes e instalar wget, gnupg y ca-certificates
apt-get update && apt-get install -y wget gnupg ca-certificates

# Descargar Google Chrome
wget https://dl.google.com/linux/direct/google-chrome-stable_current_amd64.deb

# Instalar Google Chrome
apt-get install -y ./google-chrome-stable_current_amd64.deb

# Establecer la ruta del ejecutable de Google Chrome para Puppeteer
export PUPPETEER_EXECUTABLE_PATH="/usr/bin/google-chrome-stable"
