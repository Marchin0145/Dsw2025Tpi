# Dsw2025Tpi

## Integrantes del grupo

-58212-Tártalo Aguirre Franco Emanuel-Franco.TartaloAguirre@alu.frt.utn.edu.ar
-57873-Campos Lucas Gonzalo-Lucas.Campos@alu.frt.utn.edu.ar
-58185-Rodriguez Hector Martin-HectorMartin.Rodriguez@alu.frt.utn.edu.ar

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
