using BibliotecaApi.Application.Services;
using BibliotecaApi.Domain.Entities;
using BibliotecaApi.Domain.Interfaces;
using BibliotecaApi.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. CONEXIÓN DE CAPAS (Inyección de Dependencias)
// Conecta la interfaz (Domain) con su implementación SQL (Infrastructure)
builder.Services.AddScoped<ILibroRepository, LibroRepository>();

// Registra el servicio de lógica de negocio (Application)
builder.Services.AddScoped<LibroService>();

var app = builder.Build();

// 2. ENDPOINTS DE LIBROS (Capa de Presentación)

// GET /api/libros -> Listar todos los libros
app.MapGet("/api/libros", async (LibroService service) =>
{
    var libros = await service.ListarAsync();
    return Results.Ok(libros);
});

// GET /api/libros/{id} -> Obtener un libro por ID
app.MapGet("/api/libros/{id:int}", async (int id, LibroService service) =>
{
    var libro = await service.ObtenerPorIdAsync(id);
    return libro is not null
        ? Results.Ok(libro)
        : Results.NotFound(new { mensaje = "El libro especificado no existe." });
});

// POST /api/libros -> Crear un libro
app.MapPost("/api/libros", async (Libro libro, LibroService service) =>
{
    var (statusCode, data, error) = await service.CrearAsync(libro);

    if (statusCode == 400)
        return Results.BadRequest(new { mensaje = error });

    var creado = (Libro)data!;
    return Results.Created($"/api/libros/{creado.Id}", creado);
});

// PUT /api/libros/{id} -> Modificar un libro
app.MapPut("/api/libros/{id:int}", async (int id, Libro libro, LibroService service) =>
{
    var (statusCode, error) = await service.ActualizarAsync(id, libro);

    return statusCode switch
    {
        400 => Results.BadRequest(new { mensaje = error }),
        404 => Results.NotFound(new { mensaje = error }),
        _ => Results.NoContent()
    };
});

// DELETE /api/libros/{id} -> Eliminar un libro
app.MapDelete("/api/libros/{id:int}", async (int id, LibroService service) =>
{
    var (statusCode, error) = await service.EliminarAsync(id);

    return statusCode switch
    {
        404 => Results.NotFound(new { mensaje = error }),
        409 => Results.Conflict(new { mensaje = error }),
        _ => Results.NoContent()
    };
});

app.Run();