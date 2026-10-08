# Dsw2025Tpi

## Integrantes del grupo

-58114-Goane Bernardo Luis - Bernardo.Goane@alu.frt.utn.edu.ar
-57861-Barale Agustin Miqueas - Agustin.Barale@alu.frt.utn.edu.ar
-58282-Alonso Iglesias Fernando Paul - Fernando.AlonsoIglesias@alu.frt.utn.edu.ar
-57873-Campos Lucas Gonzalo - Lucas.Campos@alu.frt.utn.edu.ar
-58185-Rodriguez Hector Martin - HectorMartin.Rodriguez@alu.frt.utn.edu.ar

## Instrucciones para configurar y ejecutar el proyecto localmente

1. **Requisitos previos**
   - [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
   - Visual Studio 2022
   - SQL Server

2. **Clonar el repositorio**

   git clone https://github.com/Marchin0145/Dsw2025Tpi.git

3. **Configurar la base de datos**
   - Edita el archivo `appsettings.json` en el proyecto `Dsw2025Tpi.Api` para establecer la cadena de conexión de la base de datos.

4. **Aplicar migraciones**

  dotnet run --project Dsw2025Tpi.Api

5. **Ejecutar el proyecto**
 
   El API estará disponible en [https://localhost:7138].

## Descripción de los endpoints implementados

#1. Crear un producto
Método: POST

Ruta: /api/products

Descripción: Crea un nuevo producto con los datos enviados.

Respuesta:

201 Created si se crea correctamente.

400 Bad Request si los datos son inválidos.

#2. Obtener todos los productos

Método: GET

Ruta: /api/products

Descripción: Devuelve la lista de todos los productos.

Respuesta:

200 OK con los productos.

204 No Content si no hay productos registrados.

#3. Obtener un producto por ID

Método: GET

Ruta: /api/products/{id}

Descripción: Devuelve los datos del producto con ese ID.

Respuesta:

200 OK si se encuentra.

404 Not Found si no existe.

#4. Actualizar un producto

Método: PUT

Ruta: /api/products/{id}

Descripción: Actualiza los datos del producto con ese ID.

Respuesta:

200 OK si se actualiza.

400 Bad Request o 404 Not Found según el caso.

#5. Inhabilitar un producto

Método: PATCH

Ruta: /api/products/{id}

Descripción: Marca el producto como inactivo (IsActive = false).

Respuesta:

204 No Content si se modifica correctamente.

404 Not Found si no se encuentra.

#6. Crear una nueva orden

Método: POST

Ruta: /api/orders

Descripción: Registra una nueva orden. Verifica stock antes de crearla.

Respuesta:

201 Created si se crea correctamente.

400 Bad Request si los datos son inválidos o hay stock insuficiente.

#7. Obtener todas las órdenes

Método: GET

Ruta: /api/orders

Descripción: Devuelve una lista de órdenes, con opción de filtrar por estado o cliente.

Respuesta:

200 OK con la lista.

500 Internal Server Error si hay un fallo en el servidor.

#8. Obtener una orden por ID

Método: GET

Ruta: /api/orders/{id}

Descripción: Devuelve los detalles de una orden específica.

Respuesta:

200 OK si se encuentra.

404 Not Found si no existe.

#9. Actualizar el estado de una orden

Método: PUT

Ruta: /api/orders/{id}/status

Descripción: Cambia el estado de una orden existente. Es idempotente.

Respuesta:

200 OK si se actualiza correctamente.

400 Bad Request si el estado es inválido o no permitido.

404 Not Found si la orden no existe.

## Uso

Puedes probar los endpoints utilizando herramientas como [Postman] o [Swagger].

## Ejecutar la API con Docker

Desde la raíz del repositorio:

```bash
docker build -t dsw2025tpi-api .
docker run --rm --name dsw2025tpi-api -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  dsw2025tpi-api
```

La API escucha en `http://+:8080` dentro del contenedor. Para cambiar esa URL,
pasá `-e ASPNETCORE_URLS=http://+:PUERTO` y ajustá el segundo puerto de `-p`.
El entorno se selecciona con `ASPNETCORE_ENVIRONMENT`; Swagger está disponible
en `/swagger` tanto localmente como en la URL pública de Azure. El documento
OpenAPI se sirve en `/swagger/v1/swagger.json`.

Comprobá que la API responde con:

```bash
curl -i http://localhost:8080/healthcheck
```

La aplicación necesita SQL Server para los endpoints que usan datos. La cadena
de conexión se puede pasar al contenedor como
`ConnectionStrings__Dsw2025Tpi`; el valor `localdb` de
`appsettings.Development.json` no funciona dentro de un contenedor Linux. Si
SQL Server se ejecuta en la computadora anfitriona, usá `host.docker.internal`
como servidor en la cadena. Para un entorno distinto de `Development`, también
configurá `Jwt__key`, `Jwt__issuer` y `Jwt__Audience` mediante variables de
entorno. No guardes contraseñas ni claves reales en el Dockerfile.

## Despliegue en Azure App Service y Azure SQL Database

El workflow `.github/workflows/deploy.yml` valida los cambios en `dev` y
`main`: restaura paquetes, compila la solución desde cero y construye la
imagen Docker. En `main` sube esa imagen a GitHub Container Registry (GHCR)
y configura Azure App Service para ejecutarla. La Web App debe ser de tipo
**Container** con Linux, no de tipo Code.

### Recursos de Azure

1. Creá una Azure SQL Database con la oferta **Free** y seleccioná que se
   pause al alcanzar el límite mensual gratuito. Guardá el nombre del servidor,
   de la base y del usuario administrador SQL. Las dos migraciones de EF Core
   usan la misma base de datos.
2. Creá una Web App de tipo **Container**, con Linux y plan **Free F1**.
3. En la configuración de la Web App agregá estas variables:
   `ASPNETCORE_ENVIRONMENT=Production`, `ConnectionStrings__Dsw2025Tpi`,
   `Jwt__key`, `Jwt__issuer`, `Jwt__Audience`, `AdminUser__Email`,
   `AdminUser__Password` y `WEBSITES_PORT=8080`. La cadena debe apuntar al servidor de Azure SQL,
   tener `Encrypt=True` y usar las credenciales SQL elegidas. La aplicación
   requiere el correo y la contraseña iniciales del administrador fuera de
   Development; no uses los valores por defecto de desarrollo.
4. Permití en el firewall de Azure SQL la IP desde la que aplicarás las
   migraciones y las direcciones de salida que muestra la Web App. No hace
   falta publicar credenciales en el repositorio.

### Crear el esquema de la base

Con `ConnectionStrings__Dsw2025Tpi` configurada en tu entorno local y acceso
autorizado por el firewall, ejecutá desde la raíz del repositorio:

```bash
dotnet ef database update --context Dsw2025TpiContext --project Dsw2025Tpi.Api --startup-project Dsw2025Tpi.Api
dotnet ef database update --context AuthenticateContext --project Dsw2025Tpi.Api --startup-project Dsw2025Tpi.Api
```

Los factories de diseño permiten aplicar cada conjunto de migraciones sin
iniciar la API ni ejecutar el seeder. También se pueden generar scripts SQL
con `dotnet ef migrations script --idempotent` y el mismo `--context`.

### Conectar GitHub Actions con Azure

Creá una identidad de Microsoft Entra con una credencial federada de GitHub
para la rama `main` de este repositorio y asignale el rol **Website Contributor**
solo sobre la Web App. En los secretos de Actions del repositorio guardá
`AZURE_CLIENT_ID`, `AZURE_TENANT_ID` y `AZURE_SUBSCRIPTION_ID`. En las variables
de Actions guardá `AZURE_WEBAPP_NAME` con el nombre de la Web App y
`AZURE_WEBAPP_URL` con su dominio predeterminado completo, incluyendo `https://`
y sin barra final. Copiá la URL de **Overview** en Azure; puede incluir un
hash y la región. El workflow usa OpenID Connect y no necesita guardar una
contraseña de publicación.

El workflow sube la imagen a `ghcr.io/marchin0145/dsw2025tpi:<commit>` usando
el `GITHUB_TOKEN` de Actions. La primera imagen puede quedar privada por
defecto. Después del primer `push` a `main`, abrí el paquete en GitHub:
**Marchin0145 → Packages → dsw2025tpi → Package settings → Change visibility**,
y elegí **Public** para que App Service pueda descargarlo sin credenciales de
registro. Volvé a ejecutar el workflow desde **Actions → Re-run all jobs**.
Si la imagen debe permanecer privada, configurá credenciales de lectura de
GHCR en App Service; este flujo supone una imagen pública.

Después de subir el workflow y configurar esos valores, cada push o merge en
`main` desplegará la API. El job de despliegue consulta `/healthcheck` y
`/api/products` en la URL pública para comprobar el arranque y la conexión SQL.
