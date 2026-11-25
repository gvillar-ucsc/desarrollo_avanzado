using BibliotecaApi.Application.Services;
using BibliotecaApi.Domain.Entities;
using BibliotecaApi.Domain.Interfaces;
using BibliotecaApi.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ILibroRepository, LibroRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IPrestamoRepository, PrestamoRepository>();

builder.Services.AddScoped<LibroService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<PrestamoService>();

var app = builder.Build();

app.UseHttpsRedirection();

// ==========================================
// LIBROS (/api/libros)
// ==========================================

app.MapGet("/api/libros", async (LibroService service) =>
{
    var libros = await service.ListarAsync();
    return Results.Ok(libros);
});

app.MapGet("/api/libros/{id:int}", async (int id, LibroService service) =>
{
    var libro = await service.ObtenerPorIdAsync(id);
    return libro is not null
        ? Results.Ok(libro)
        : Results.NotFound(new { mensaje = "El libro especificado no existe." });
});

app.MapPost("/api/libros", async (Libro libro, LibroService service) =>
{
    var (statusCode, data, error) = await service.CrearAsync(libro);
    return statusCode switch
    {
        201 => Results.Created($"/api/libros/{((Libro)data!).Id}", data),
        _ => Results.BadRequest(new { mensaje = error })
    };
});

app.MapPut("/api/libros/{id:int}", async (int id, Libro libro, LibroService service) =>
{
    var (statusCode, error) = await service.ActualizarAsync(id, libro);
    return statusCode switch
    {
        204 => Results.NoContent(),
        404 => Results.NotFound(new { mensaje = error }),
        _ => Results.BadRequest(new { mensaje = error })
    };
});

app.MapDelete("/api/libros/{id:int}", async (int id, LibroService service) =>
{
    var (statusCode, error) = await service.EliminarAsync(id);
    return statusCode switch
    {
        204 => Results.NoContent(),
        404 => Results.NotFound(new { mensaje = error }),
        409 => Results.Conflict(new { mensaje = error }),
        _ => Results.BadRequest(new { mensaje = error })
    };
});

// ==========================================
// USUARIOS (/api/usuarios)
// ==========================================

app.MapGet("/api/usuarios", async (UsuarioService service) =>
{
    var usuarios = await service.ListarAsync();
    return Results.Ok(usuarios);
});

app.MapGet("/api/usuarios/{id:int}", async (int id, UsuarioService service) =>
{
    var usuario = await service.ObtenerPorIdAsync(id);
    return usuario is not null
        ? Results.Ok(usuario)
        : Results.NotFound(new { mensaje = "El usuario especificado no existe." });
});

app.MapPost("/api/usuarios", async (Usuario usuario, UsuarioService service) =>
{
    var (statusCode, data, error) = await service.CrearAsync(usuario);
    return statusCode switch
    {
        201 => Results.Created($"/api/usuarios/{((Usuario)data!).Id}", data),
        409 => Results.Conflict(new { mensaje = error }),
        _ => Results.BadRequest(new { mensaje = error })
    };
});

// ==========================================
// PRÉSTAMOS (/api/prestamos)
// ==========================================

app.MapGet("/api/prestamos", async (PrestamoService service) =>
{
    var prestamos = await service.ListarAsync();
    return Results.Ok(prestamos);
});

app.MapPost("/api/prestamos", async (CrearPrestamoRequest request, PrestamoService service) =>
{
    var (statusCode, data, error) = await service.RegistrarPrestamoAsync(request.IdUsuario, request.IdLibro);
    return statusCode switch
    {
        201 => Results.Created($"/api/prestamos/{((Prestamo)data!).Id}", data),
        404 => Results.NotFound(new { mensaje = error }),
        409 => Results.Conflict(new { mensaje = error }),
        _ => Results.BadRequest(new { mensaje = error })
    };
});

app.MapPut("/api/prestamos/{id:int}/devolucion", async (int id, PrestamoService service) =>
{
    var (statusCode, data, error) = await service.RegistrarDevolucionAsync(id);
    return statusCode switch
    {
        200 => Results.Ok(data),
        404 => Results.NotFound(new { mensaje = error }),
        409 => Results.Conflict(new { mensaje = error }),
        _ => Results.BadRequest(new { mensaje = error })
    };
});

app.Run();

public record CrearPrestamoRequest(int IdUsuario, int IdLibro);