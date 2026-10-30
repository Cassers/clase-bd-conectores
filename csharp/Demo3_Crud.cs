using Npgsql;

// Paso 4: CRUD dentro del código con Npgsql (SQL escrito a mano + parámetros).
record Tripulante(int Id, string Nombre, string Email, string Rango, int Puntos);

class TripulantesRepositorio(NpgsqlDataSource fuente)
{
    // C — Create: RETURNING id devuelve el id que generó la base.
    public async Task<int> Crear(string nombre, string email, int escuadronId)
    {
        await using var cmd = fuente.CreateCommand(
            "INSERT INTO tripulantes (nombre, email, escuadron_id) VALUES (@nombre, @email, @escuadron) RETURNING id");
        cmd.Parameters.AddWithValue("nombre", nombre);
        cmd.Parameters.AddWithValue("email", email);
        cmd.Parameters.AddWithValue("escuadron", escuadronId);
        return (int)(await cmd.ExecuteScalarAsync())!;
    }

    // R — Read (todos)
    public async Task<List<Tripulante>> Listar()
    {
        var lista = new List<Tripulante>();
        await using var cmd = fuente.CreateCommand(
            "SELECT id, nombre, email, rango, puntos FROM tripulantes ORDER BY puntos DESC");
        await using var lector = await cmd.ExecuteReaderAsync();
        while (await lector.ReadAsync())
            lista.Add(new Tripulante(lector.GetInt32(0), lector.GetString(1), lector.GetString(2),
                                     lector.GetString(3), lector.GetInt32(4)));
        return lista;
    }

    // R — Read (uno)
    public async Task<Tripulante?> BuscarPorId(int id)
    {
        await using var cmd = fuente.CreateCommand(
            "SELECT id, nombre, email, rango, puntos FROM tripulantes WHERE id = @id");
        cmd.Parameters.AddWithValue("id", id);
        await using var lector = await cmd.ExecuteReaderAsync();
        if (!await lector.ReadAsync()) return null;
        return new Tripulante(lector.GetInt32(0), lector.GetString(1), lector.GetString(2),
                              lector.GetString(3), lector.GetInt32(4));
    }

    // U — Update: devuelve cuántas filas cambió (0 = no existía).
    public async Task<int> SumarPuntos(int id, int puntos)
    {
        await using var cmd = fuente.CreateCommand(
            "UPDATE tripulantes SET puntos = puntos + @puntos WHERE id = @id");
        cmd.Parameters.AddWithValue("puntos", puntos);
        cmd.Parameters.AddWithValue("id", id);
        return await cmd.ExecuteNonQueryAsync();
    }

    // D — Delete
    public async Task<int> Eliminar(int id)
    {
        await using var cmd = fuente.CreateCommand("DELETE FROM tripulantes WHERE id = @id");
        cmd.Parameters.AddWithValue("id", id);
        return await cmd.ExecuteNonQueryAsync();
    }
}

static class Demo3_Crud
{
    public static async Task Ejecutar(NpgsqlDataSource fuente)
    {
        var repo = new TripulantesRepositorio(fuente);

        var id = await repo.Crear("Alejo Código", $"alejo{Random.Shared.Next(1000)}@discamus.net", 1);
        Console.WriteLine($"C ➜ Creado con id {id}");

        Console.WriteLine($"R ➜ {await repo.BuscarPorId(id)}");

        await repo.SumarPuntos(id, 150);
        Console.WriteLine($"U ➜ Después de sumar 150: {await repo.BuscarPorId(id)}");

        Console.WriteLine("R ➜ Top 3:");
        foreach (var t in (await repo.Listar()).Take(3))
            Console.WriteLine($"     {t.Nombre,-18} {t.Rango,-10} {t.Puntos}");

        var borradas = await repo.Eliminar(id);
        Console.WriteLine($"D ➜ Filas eliminadas: {borradas}. ¿Sigue existiendo? {(await repo.BuscarPorId(id)) is not null}");
    }
}
