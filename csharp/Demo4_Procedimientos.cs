using Npgsql;

// Paso 5: llamar procedimientos (CALL) y funciones (SELECT) desde C#.
static class Demo4_Procedimientos
{
    public static async Task Ejecutar(NpgsqlDataSource fuente)
    {
        // PROCEDURE: hace algo, no devuelve filas
        await using (var cmd = fuente.CreateCommand("CALL registrar_tripulante(@nombre, @email, @escuadron)"))
        {
            cmd.Parameters.AddWithValue("nombre", "Camila Ruiz");
            cmd.Parameters.AddWithValue("email", $"camila{Random.Shared.Next(1000)}@discamus.net");
            cmd.Parameters.AddWithValue("escuadron", 3);
            await cmd.ExecuteNonQueryAsync();
            Console.WriteLine("CALL registrar_tripulante ➜ ok (revisa la tabla bitacora)");
        }

        // PROCEDURE que falla: la base deshace todo y C# recibe el error
        try
        {
            await using var cmd = fuente.CreateCommand("CALL transferir_puntos(@origen, @destino, @cantidad)");
            cmd.Parameters.AddWithValue("origen", 8);
            cmd.Parameters.AddWithValue("destino", 1);
            cmd.Parameters.AddWithValue("cantidad", 5000);
            await cmd.ExecuteNonQueryAsync();
        }
        catch (PostgresException ex)
        {
            Console.WriteLine($"CALL transferir_puntos ➜ rechazado por la base: {ex.MessageText}");
        }

        // FUNCTION: devuelve una tabla, se lee como un SELECT normal
        Console.WriteLine("SELECT * FROM ranking_escuadron(1):");
        await using (var cmd = fuente.CreateCommand("SELECT puesto, nombre, puntos FROM ranking_escuadron(@escuadron)"))
        {
            cmd.Parameters.AddWithValue("escuadron", 1);
            await using var lector = await cmd.ExecuteReaderAsync();
            while (await lector.ReadAsync())
                Console.WriteLine($"     #{lector.GetInt64(0)} {lector.GetString(1),-18} {lector.GetInt32(2)}");
        }
    }
}
