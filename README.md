# TP2 - Introducción a Docker
**Stack:** .NET 8 API Minima + MySQL 8.4
**Autor**: @YagoGandara

## Objetivo
Levantar una app web minima, contenerizada, con MySQL (por elección propia) y dos entornos (QA y PROD) utilizando **la misma imagen, con **persistencia** y **variables de entorno**

----------------------------------------------------------------------------------------------------------------------

## Requisitos  
 - Docker Desktop 4.x (o Docker Engine 24+)
 - (Opcional) .NET 8 SDK si quuisieras compilar local (Dockerfile ya lo publica).

----------------------------------------------------------------------------------------------------------------------
## Build y publicación de la imagen
```bash
# en la raiz del proyecto
docker build -t yagogandara/tp2-docker:dev .
docker tag yagogandara/tp2-docker:dev yagogandara/tp2-docker:v1.0

docker login
docker push yagogandara/tp2-docker:dev
docker push yagogandara/tp2-docker:v1.0
```

docker-compose.yml levanta los 3 servicios (mysql, app-qa y app-prod, ambas usando la misma imagen, pero con variables de entorno distintas)

1) Creamos el .env
    - DOCKERHUB_USER=yagogandara
    - APP_TAG=v1.0   # o 'dev' si querés la versión de desarrollo

2) Levantar
    - docker compose up -d
 - docker compose ps

3) Apagar
    - docker compose down #si queremos mantener los datos
    - docker compose down -v #si queremos borrar el volumen

----------------------------------------------------------------------------------------------------------------------
## Acceso a la publicación (URL'S, puertos)
 - QA: http://localhost:8081
 - PROD: http://localhost:8082

----------------------------------------------------------------------------------------------------------------------
## Endpoints
 - GET /health -> ok
 - POST /seed -> crea tabla "todos" y crea 2 filas
 - GET /todos -> lista items
 - POST /todos -> inserta un ítem
Ejemplo: 
```bash
curl -X POST http://localhost:8081/todos
curl -H "Content-Type: application/json"
curl -d "{\"title\":\"nuevo item\"}"
```
Aclaración: si tuviste que cambiar algun puerto en el docker-compose.yml, tenes que cambiarlo por el que reemplazaste

----------------------------------------------------------------------------------------------------------------------
## Conexión a la base de datos

MySQL en contenedor (por defecto, es publicado en el 3306):
 - HOST: localhost
 - Puerto: 3307 
 - Usuario: appuser
 - Password: apppass
 - Bases: appdb_qa y appdb_prod

----------------------------------------------------------------------------------------------------------------------
# #Verificación del funcionamiento
Salud de los Servicios
```bash
    docker compose ps
    curl http://localhost:8081/health
    curl http://localhost:8082/health
```
Semilla y lectura de QA
```bash
    curl.exe -X POST http://localhost:8081/seed
    curl http://localhost:8081/todos
```

Semilla y lectura de PROD
```bash
    curl.exe -X POST http://localhost:8082/seed
    curl http://localhost:8082/todos
```

Aislamiento QA/PROD
Para esto insertaremos algo en QA y controlaremos que no aparezca en PROD; a modo de ejemplo:
```bash
    curl.exe -X POST http://localhost:8081/todos -H "Content-Type: application/json" -d "{\"title\":\"qa-only\"}"
    curl http://localhost:8081/todos
    curl http://localhost:8082/todos
```

Persistencia
```bash
    # agregar un registro
    curl.exe -X POST http://localhost:8081/todos -H "Content-Type: application/json" -d '{"title":"persisto"}'

    # reiniciar servicios
    docker compose restart

    # el registro debe seguir
    curl http://localhost:8081/todos

```

----------------------------------------------------------------------------------------------------------------------
## Para evidenciar el funcionamiento
Tenemos varias opciones o puntos a revisar, principalmente con logs:
1) docker compose ps, mostrando mysql, app-qa y app-prod Up (healthy)
2) Pagina raíz de QA y PROD (/) mostrando enviroments distintos
3) POST /seed y GET /todos en ambos entornos (deberian dar distintos)
4) Persistencia tras docker compose restart
