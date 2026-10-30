using Npgsql;

// Paso 3: el error más peligroso de un conector — armar el SQL pegando texto (SQL injection).
static class Demo2_Inyeccion
{
    // ❌ INSEGURO: lo que escribe el usuario se vuelve parte del SQL.
    static async Task LoginInseguro(NpgsqlDataSource fuente, string usuario, string clave)
    {
        var sql = $"SELECT usuario, nivel FROM codigos_acceso WHERE usuario = '{usuario}' AND clave = '{clave}'";
        Console.WriteLine($"   SQL enviado: {sql}");

        await using var cmd = fuente.CreateCommand(sql);
        await using var lector = await cmd.ExecuteReaderAsync();
        var entro = false;
        while (await lector.ReadAsync())
        {
            entro = true;
            Console.WriteLine($"   🚪 Acceso concedido a {lector.GetString(0)} (nivel {lector.GetString(1)})");
        }
        if (!entro) Console.WriteLine("   ⛔ Acceso denegado");
    }

    // ✅ SEGURO: consulta parametrizada. El SQL y los datos viajan por separado;
    // lo que escriba el usuario JAMÁS se interpreta como código SQL.
    static async Task LoginSeguro(NpgsqlDataSource fuente, string usuario, string clave)
    {
        await using var cmd = fuente.CreateCommand(
            "SELECT usuario, nivel FROM codigos_acceso WHERE usuario = @usuario AND clave = @clave");
        cmd.Parameters.AddWithValue("usuario", usuario);
        cmd.Parameters.AddWithValue("clave", clave);
        Console.WriteLine($"   SQL enviado: {cmd.CommandText}   (+ parámetros aparte)");

        await using var lector = await cmd.ExecuteReaderAsync();
        if (await lector.ReadAsync())
            Console.WriteLine($"   🚪 Acceso concedido a {lector.GetString(0)} (nivel {lector.GetString(1)})");
        else
            Console.WriteLine("   ⛔ Acceso denegado");
    }

    public static async Task Ejecutar(NpgsqlDataSource fuente)
    {
        const string ataque = "' OR '1'='1' --";

        Console.WriteLine("1) Login normal con clave correcta (inseguro):");
        await LoginInseguro(fuente, "mateo", "Cometa77");

        Console.WriteLine("\n2) El Fantasma Inyector escribe como usuario:  " + ataque);
        await LoginInseguro(fuente, ataque, "no-sé-la-clave");

        Console.WriteLine("\n3) Mismo ataque contra la versión con parámetros:");
        await LoginSeguro(fuente, ataque, "no-sé-la-clave");

        Console.WriteLine("\n4) Login normal (seguro) sigue funcionando:");
        await LoginSeguro(fuente, "mateo", "Cometa77");
    }
}
