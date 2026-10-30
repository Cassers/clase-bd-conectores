using Npgsql;

// Paso 1 de todo conector: armar la cadena de conexión SIN escribir la clave en el código.
// Los datos salen de variables de entorno (o del archivo .env, que nunca se sube a GitHub).
static class Conexion
{
    public static string CadenaDeConexion()
    {
        CargarEnv(".env");

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = Leer("DB_HOST"),
            Port = int.Parse(Leer("DB_PORT", "5432")),
            Username = Leer("DB_USER"),
            Password = Leer("DB_PASSWORD"),
            Database = Leer("DB_NAME", "postgres"),
            // Require  = el tráfico viaja cifrado con TLS (nadie en la red puede leer claves ni datos).
            // Disable  = texto plano: solo aceptable en tu propia máquina (Docker local).
            // VerifyFull = cifrado + verifica el certificado del servidor (lo ideal en producción).
            SslMode = Enum.Parse<SslMode>(Leer("DB_SSL", "Require")),
        };
        return builder.ConnectionString;
    }

    // NpgsqlDataSource administra un "pool" de conexiones: se crea una vez y se reutiliza.
    public static NpgsqlDataSource CrearFuente() => NpgsqlDataSource.Create(CadenaDeConexion());

    static string Leer(string nombre, string? porDefecto = null) =>
        Environment.GetEnvironmentVariable(nombre) ?? porDefecto
        ?? throw new InvalidOperationException($"Falta la variable {nombre} (revisa tu archivo .env)");

    // Lector mínimo de .env: líneas CLAVE=valor; ignora comentarios.
    static void CargarEnv(string ruta)
    {
        if (!File.Exists(ruta)) return;
        foreach (var linea in File.ReadAllLines(ruta))
        {
            if (string.IsNullOrWhiteSpace(linea) || linea.TrimStart().StartsWith('#')) continue;
            var partes = linea.Split('=', 2);
            if (partes.Length == 2 && Environment.GetEnvironmentVariable(partes[0].Trim()) is null)
                Environment.SetEnvironmentVariable(partes[0].Trim(), partes[1].Trim());
        }
    }
}
