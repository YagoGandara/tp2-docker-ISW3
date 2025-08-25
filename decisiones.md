---

## `decisiones.md`

```md
# decisiones.md

## Elección de la aplicación y tecnología utilizada
Se implementó una **Minimal API en .NET 8** por:
- Simplicidad para crear endpoints REST.
- Buen soporte oficial, rendimiento y tooling.
- Facilidad para contenerizar (imágenes oficiales de Microsoft).
- Es el que vimos en clase

Endpoints:
- `/health` para mostrar si está vivo.
- `/seed` para preparar datos de demo.
- `/todos` para CRUD mínimo de verificación.

---

## Elección de imagen base

Se usa un **Dockerfile multi-stage**:
- **Build stage:** `mcr.microsoft.com/dotnet/sdk:8.0`  
  Compila y publica en Release (incluye SDK y herramientas).
- **Runtime stage:** `mcr.microsoft.com/dotnet/aspnet:8.0`  
  Ejecuta el binario con un entorno mínimo, más seguro y liviano.

Justificación:
- Reduce el tamaño de la imagen final
- Separa la compilación de la ejecución 
- Sigue mejores prácticas de Docker para .NET

---

## Elección de base de datos

Mi elección fue **MySQL 8.4**:
- Es una imagen oficial y muy utilizada
- No es dificil de integrar con .NET (usando `MySqlConnector` + `Dapper`)
- Conozco MySQL, y me siento mas familiarizado con este lenguaje

---

## Estructura del Dockerfile

- Copia el `.csproj` y hace un `dotnet restore` (agarra las dependencias).
- Copia el código y publica en `/app`
- Imagen final solo con lo necesario para ejecutar.
- `EXPOSE 8080` + `ASPNETCORE_URLS=http://+:8080` (no requiere puertos privilegiados).
- Variables `ASPNETCORE_ENVIRONMENT` y `CONNECTION_STRING` preparadas para **inyectarse desde compose**.
- `ENTRYPOINT ["dotnet","tp2-docker.dll"]` para iniciar la app.

Elegí esta estructura ya que:
1) Las capas atrapables me permiten un build más rápido
2) Es seguro y pequeño, por lo que el tiempo de ejecución es pequeño
3) Configura por variables de entorno, lo que me evita tener que hardcodear secretos (mala practica de seguridad)

---
## Configuración de QA y PROD (variables de entorno)
Se ejecuta **la misma imagen** en dos servicios: 
- `app-qa` (`ASPNETCORE_ENVIRONMENT=QA`, DB `appdb_qa`)
- `app-prod` (`ASPNETCORE_ENVIRONMENT=Production`, DB `appdb_prod`)

Ambos usan:
- `CONNECTION_STRING` con `Server=mysql;Port=3306...;User ID=appuser;Password=apppass`
- El hostname `mysql` (en la red de Docker Compose, ese es el nombre del servicio)

Se eligió esta estructura ya que las diferencias se **centralizan** en variables (12-factor), y se establece una **Paridad de binarios** en todos los entornos (lo que evita el desplazamiento de los datos de un entorno a otro)

---

## Estrategia de persistencia de datos
çSe definió el volumen `mysqldata` montado en `/var/lib/mysql` del contenedor MySQL:
- Los datos **persisten** entre reinicios de contenedor
- Simple reinicialización del entorno: `docker compose down -v`

Adicionalmente se incluyó `db/init.sql`, el cual es encargado de :
- Crear las bases de datos `appdb_qa` y `appdb_prod`
- Crear al usuario `appuser` con permisos

Esto fue diagramado así para asegurar los datos persistentes para las pruebas, y para que se pueda reproducir en cualquier máquina

---

## Estrategia de versionado y publicación
- `dev`: imagen que puede cambiar para iteraciones rapidas
- `v1.0`: **release estable** para la corrección

Se usa un `.env` con `APP_TAG` para elegir la versión que se quiera desplegar, sin tocar el compose.
Publicado en Docker Hub: `yagogandara/tp2-docker:<tag deseado>`

Esto nos permite un control mas claro entre **desarrollo** y **entrega**, además de permitir trazar que binario se ejecuta en cada entorno

---

## Para evidenciar el funcionamiento 
- **Aplicación corriendo en ambos entornos:**
    1) `docker compose ps` con :
            `mysql`
            `app-qa`
            `app-prod`
        en `Up (healthy)`
    2) `GET /` en QA y PROD mostrando dos enviroments distintos

- **Conexión exitosa a la base:**
    `POST /seed` y luego `GET / todos` en QA y PROD

- **Datos Persistiendo:**
    1) Insertamos datos
    2) `docker compose restart`
    3) Los datos siguen en `GET /todos` tanto en QA como en PROD

--- 
## Problemas y soluciones
- **“Port 3306 already in use”** (host con MySQL local):  
  *Solución:* cambiar mapeo a `3307:3306` o no publicar el puerto (si no se necesita acceso desde host).

- **Error al hacer push a Docker Hub (proxy/VPN):**  
  *Solución:* reintentar (Docker reanuda capas)