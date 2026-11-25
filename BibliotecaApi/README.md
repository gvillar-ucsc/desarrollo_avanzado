## Requisitos
* .NET SDK (8.0 o superior)
* PostgreSQL con la base de datos `db_biblioteca_api` creada
* Visual Studio Code con la extensión **REST Client** instalada para ejecutar las pruebas

## Configuración y ejecución
1. **Configurar contraseña:** Primero hay que ingresar al archivo `appsettings.json` y dirigirse a `DefaultConnection`, en donde solo hay que modificar `Password` con la contraseña personal de PostgreSQL.
2. **Levantar la API:** Después hay que abrir una terminal en la carpeta del proyecto (donde está el archivo `.csproj`) y ejecutar el comando `dotnet run`.
3. En la consola aparecerá la dirección local donde quedó corriendo el servidor (por ejemplo, `http://localhost:5255`).

## Cómo usar y probar la API
1. Con la API corriendo en la terminal, dirigirse al archivo `BibliotecaApi.http` dentro de Visual Studio Code.
2. Dentro del archivo verás todas las consultas separadas por bloques (`GET`, `POST`, `PUT`, `DELETE`). Justo encima de cada consulta aparece el botón **Send Request**; solo debes hacer clic ahí para enviar la petición y ver la respuesta de la API a la derecha.
3. En las consultas `POST` y `PUT` verás un bloque de texto en formato JSON con datos de ejemplo. Puedes modificar directamente esos valores en el JSON (como el título, autor, nombre, correo o los IDs) para ingresar los datos que quieras antes de hacer clic en **Send Request**.
4. El archivo incluye tanto las pruebas exitosas como los casos de error controlados (campos vacíos, IDs inexistentes, correos duplicados, libros no disponibles y devoluciones repetidas).

## Endpoints incluidos
* **Libros:** `GET /api/libros`, `GET /api/libros/{id}`, `POST /api/libros`, `PUT /api/libros/{id}`, `DELETE /api/libros/{id}`
* **Usuarios:** `GET /api/usuarios`, `GET /api/usuarios/{id}`, `POST /api/usuarios`
* **Préstamos:** `GET /api/prestamos`, `POST /api/prestamos`, `PUT /api/prestamos/{id}/devolucion`