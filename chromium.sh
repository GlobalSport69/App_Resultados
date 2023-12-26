#!/bin/sh

# Descargar la clave pública de Google
wget -q -O - https://dl-ssl.google.com/linux/linux_signing_key.pub | gpg --dearmor -o /usr/share/keyrings/chromium-archive-keyring.gpg

# Agregar el repositorio de Chromium a la lista de fuentes de APT
echo "deb [arch=arm64 signed-by=/usr/share/keyrings/chromium-archive-keyring.gpg] http://dl.google.com/linux/chrome/deb/ stable main" | tee /etc/apt/sources.list.d/chromium.list

# Actualizar la lista de paquetes e instalar Chromium
apt-get update && apt-get install -y chromium

# Establecer la ruta del ejecutable de Chromium para Puppeteer
export PUPPETEER_EXECUTABLE_PATH="/usr/bin/chromium"
