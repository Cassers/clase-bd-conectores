// Discamus Academy · Operación Puente Embrujado
// Uso:  dotnet run -- 1     (o sin número para ver el menú)

var opcion = args.FirstOrDefault();
if (opcion is null)
{
    Console.WriteLine("""
        🎃 Operación Puente Embrujado
          1) Conectar (¿plano o cifrado?)
          2) SQL injection: inseguro vs parametrizado
          3) CRUD con Npgsql
          4) Procedimientos y funciones
          5) LINQ con Entity Framework
          0) Todo en orden
        """);
    Console.Write("Elige: ");
    opcion = Console.ReadLine()?.Trim();
}

await using var fuente = Conexion.CrearFuente();

async Task Paso(string titulo, Func<Task> demo)
{
    Console.WriteLine($"\n===== {titulo} =====");
    await demo();
}

switch (opcion)
{
    case "1": await Paso("1. Conectar", () => Demo1_Conectar.Ejecutar(fuente)); break;
    case "2": await Paso("2. SQL injection", () => Demo2_Inyeccion.Ejecutar(fuente)); break;
    case "3": await Paso("3. CRUD con Npgsql", () => Demo3_Crud.Ejecutar(fuente)); break;
    case "4": await Paso("4. Procedimientos", () => Demo4_Procedimientos.Ejecutar(fuente)); break;
    case "5": await Paso("5. LINQ", () => Demo5_Linq.Ejecutar(Conexion.CadenaDeConexion())); break;
    case "0":
        await Paso("1. Conectar", () => Demo1_Conectar.Ejecutar(fuente));
        await Paso("2. SQL injection", () => Demo2_Inyeccion.Ejecutar(fuente));
        await Paso("3. CRUD con Npgsql", () => Demo3_Crud.Ejecutar(fuente));
        await Paso("4. Procedimientos", () => Demo4_Procedimientos.Ejecutar(fuente));
        await Paso("5. LINQ", () => Demo5_Linq.Ejecutar(Conexion.CadenaDeConexion()));
        break;
    default: Console.WriteLine("Opción no válida"); break;
}
