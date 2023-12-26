# Generador de Resultados

Sistema generador de resultados de loterías mediante scraping con administrativo y monitor de tareas.

## Instrucciones de Despliegue

Para desplegar la aplicación, ejecuta el siguiente comando en la terminal:

```bash 
docker-compose -f app-dev-compose.yml up --build -d 
```

**Nota: debido a que google chrome no esta disponible para arquitecturas aarch64 se instala en su lugar chromium sin embargo para esto es necesario especificar la arquitectura del host**

Ejecute este comando para generar un archivo .env con la arquitectura de su host o realice una copia de **.env.example**, remobrelo a **.env** y realice la modificacion pertinente

```bash
echo "ARG=$(uname -m)" > .env
```