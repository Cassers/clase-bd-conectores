using Npgsql;

// Paso 2: abrir la conexión, hacer una consulta y comprobar si viaja cifrada.
static class Demo1_Conectar
{
    public static async Task Ejecutar(NpgsqlDataSource fuente)
    {
        await using var conexion = await fuente.OpenConnectionAsync();
        Console.WriteLine($"✅ Conectado a {conexion.Host}:{conexion.Port} / base '{conexion.Database}'");

        await using var cmd = new NpgsqlCommand(
            "SELECT version(), (SELECT ssl FROM pg_stat_ssl WHERE pid = pg_backend_pid())", conexion);
        await using var lector = await cmd.ExecuteReaderAsync();
        await lector.ReadAsync();

        Console.WriteLine($"Servidor: {lector.GetString(0)[..30]}...");
        var cifrada = !lector.IsDBNull(1) && lector.GetBoolean(1);
        Console.WriteLine(cifrada
            ? "🔒 Conexión CIFRADA (TLS): las claves y los datos no viajan en texto plano."
            : "⚠️  Conexión en TEXTO PLANO: aceptable solo en tu máquina, nunca contra un servidor en internet.");
    }
}
