// SOLUCIONARIO (solo profe) · Jefe del Paso 4, Paso 5 (K1–K3) y Paso 6 (L1–L3)
// Para probarlo: copiar este archivo dentro de csharp/ y llamar Soluciones.Ejecutar(...) desde Program.cs
using Microsoft.EntityFrameworkCore;
using Npgsql;

static class TripulantesBusqueda
{
    // 👻 Jefe: búsqueda parcial con ILIKE y parámetro. El % se agrega al VALOR, no al SQL.
    public static async Task<List<string>> BuscarPorNombre(NpgsqlDataSource fuente, string texto)
    {
        var nombres = new List<string>();
        await using var cmd = fuente.CreateCommand("SELECT nombre, email FROM tripulantes WHERE nombre ILIKE @patron");
        cmd.Parameters.AddWithValue("patron", $"%{texto}%");
        await using var lector = await cmd.ExecuteReaderAsync();
        while (await lector.ReadAsync()) nombres.Add($"{lector.GetString(0)} <{lector.GetString(1)}>");
        return nombres;
    }
}

record Mision(int Id, string Nombre, string Destino, int Dificultad);

class MisionesRepositorio(NpgsqlDataSource fuente)
{
    // K1 + K2 + K3
    public async Task<int?> Crear(string nombre, string destino, int dificultad)
    {
        try
        {
            await using var cmd = fuente.CreateCommand(
                "INSERT INTO misiones (nombre, destino, dificultad) VALUES (@nombre, @destino, @dificultad) RETURNING id");
            cmd.Parameters.AddWithValue("nombre", nombre);
            cmd.Parameters.AddWithValue("destino", destino);
            cmd.Parameters.AddWithValue("dificultad", dificultad);
            return (int)(await cmd.ExecuteScalarAsync())!;
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.CheckViolation)
        {
            Console.WriteLine($"   ⚠️ La dificultad debe estar entre 1 y 5 (recibí {dificultad}).");
            return null;
        }
    }

    public async Task<List<Mision>> Listar()
    {
        var lista = new List<Mision>();
        await using var cmd = fuente.CreateCommand("SELECT id, nombre, destino, dificultad FROM misiones ORDER BY id");
        await using var lector = await cmd.ExecuteReaderAsync();
        while (await lector.ReadAsync())
            lista.Add(new Mision(lector.GetInt32(0), lector.GetString(1), lector.GetString(2), lector.GetInt32(3)));
        return lista;
    }

    public async Task<int> Eliminar(int id)
    {
        await using var cmd = fuente.CreateCommand("DELETE FROM misiones WHERE id = @id");
        cmd.Parameters.AddWithValue("id", id);
        return await cmd.ExecuteNonQueryAsync();
    }
}

static class Soluciones
{
    public static async Task Ejecutar(NpgsqlDataSource fuente, string cadena)
    {
        Console.WriteLine("Jefe ➜ búsqueda normal 'ma': " + string.Join(", ", await TripulantesBusqueda.BuscarPorNombre(fuente, "ma")));
        var ataque = await TripulantesBusqueda.BuscarPorNombre(fuente, "x' UNION SELECT usuario, clave FROM codigos_acceso --");
        Console.WriteLine($"Jefe ➜ con el ataque UNION: {ataque.Count} filas (claves robadas: ninguna)");

        var repo = new MisionesRepositorio(fuente);
        var id = await repo.Crear("Prueba K1", "Venus", 2);
        Console.WriteLine($"K1/K2 ➜ creada con id {id}; total misiones: {(await repo.Listar()).Count}");
        await repo.Crear("Imposible", "Sol", 9);
        Console.WriteLine($"K1 ➜ eliminadas: {await repo.Eliminar(id!.Value)}");

        await using var db = new AcademiaContext(cadena);
        var l1 = db.Tripulantes.Where(t => t.Rango == "Recluta" && t.Puntos < 200).Select(t => t.Nombre);
        Console.WriteLine($"L1 ➜ {string.Join(", ", await l1.ToListAsync())}");
        Console.WriteLine($"L3 ➜ SQL generado:\n{l1.ToQueryString()}");

        var l2 = await db.Tripulantes.GroupBy(t => t.Rango)
            .Select(g => new { Rango = g.Key, Promedio = g.Average(t => t.Puntos) })
            .ToListAsync();
        foreach (var g in l2) Console.WriteLine($"L2 ➜ {g.Rango,-10} {g.Promedio:F0}");
    }
}
