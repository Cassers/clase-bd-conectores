using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

// Paso 6: LINQ. Es de C#, NO de la base de datos.
// Con Entity Framework Core, C# traduce tu LINQ a SQL y lo manda a Postgres.

[Table("escuadrones")]
class Escuadron
{
    [Column("id")] public int Id { get; set; }
    [Column("nombre")] public string Nombre { get; set; } = "";
    [Column("color")] public string Color { get; set; } = "";
    public List<TripulanteEf> Tripulantes { get; set; } = [];
}

[Table("tripulantes")]
class TripulanteEf
{
    [Column("id")] public int Id { get; set; }
    [Column("nombre")] public string Nombre { get; set; } = "";
    [Column("email")] public string Email { get; set; } = "";
    [Column("rango")] public string Rango { get; set; } = "Recluta";
    [Column("puntos")] public int Puntos { get; set; }
    [Column("activo")] public bool Activo { get; set; } = true;
    [Column("escuadron_id")] public int? EscuadronId { get; set; }
    public Escuadron? Escuadron { get; set; }
    [Column("creado_en"), DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreadoEn { get; set; }
}

class AcademiaContext(string cadena) : DbContext
{
    public DbSet<TripulanteEf> Tripulantes => Set<TripulanteEf>();
    public DbSet<Escuadron> Escuadrones => Set<Escuadron>();

    protected override void OnConfiguring(DbContextOptionsBuilder opciones) => opciones.UseNpgsql(cadena);
}

static class Demo5_Linq
{
    public static async Task Ejecutar(string cadena)
    {
        // 0) LINQ funciona con cualquier colección, sin base de datos
        int[] puntos = [980, 120, 650, 90, 720];
        var altos = puntos.Where(p => p > 500).OrderBy(p => p);
        Console.WriteLine($"LINQ en memoria: {string.Join(", ", altos)}");

        await using var db = new AcademiaContext(cadena);

        // 1) Sintaxis de métodos + el SQL que EF genera
        var pilotos = db.Tripulantes
            .Where(t => t.Rango == "Piloto")
            .OrderByDescending(t => t.Puntos)
            .Select(t => new { t.Nombre, t.Puntos });
        Console.WriteLine("\nLINQ:  db.Tripulantes.Where(t => t.Rango == \"Piloto\").OrderByDescending(t => t.Puntos)");
        Console.WriteLine($"SQL generado:\n{pilotos.ToQueryString()}\n");
        foreach (var p in await pilotos.ToListAsync()) Console.WriteLine($"     {p.Nombre} ({p.Puntos})");

        // 2) Sintaxis de consulta (se parece a SQL, pero es C#)
        var veteranos = from t in db.Tripulantes
                        where t.Puntos >= 500
                        orderby t.Puntos descending
                        select t.Nombre;
        Console.WriteLine($"\nVeteranos (≥500): {string.Join(", ", await veteranos.ToListAsync())}");

        // 3) JOIN automático por la relación (Include)
        var conEscuadron = await db.Tripulantes.Include(t => t.Escuadron)
            .OrderBy(t => t.Nombre).Take(3).ToListAsync();
        foreach (var t in conEscuadron) Console.WriteLine($"     {t.Nombre} → {t.Escuadron?.Nombre}");

        // 4) GroupBy: puntos totales por escuadrón
        var porEscuadron = await db.Tripulantes
            .GroupBy(t => t.Escuadron!.Nombre)
            .Select(g => new { Escuadron = g.Key, Cantidad = g.Count(), Total = g.Sum(t => t.Puntos) })
            .OrderByDescending(x => x.Total)
            .ToListAsync();
        Console.WriteLine("\nPuntos por escuadrón:");
        foreach (var e in porEscuadron) Console.WriteLine($"     {e.Escuadron,-10} {e.Cantidad} tripulantes, {e.Total} pts");

        // 5) CRUD con EF: sin escribir SQL (y con parámetros automáticos → sin inyección)
        var nuevo = new TripulanteEf { Nombre = "Alejo LINQ", Email = $"linq{Random.Shared.Next(1000)}@discamus.net", EscuadronId = 2 };
        db.Tripulantes.Add(nuevo);
        await db.SaveChangesAsync();                       // INSERT
        Console.WriteLine($"\nEF Create ➜ id {nuevo.Id}");

        nuevo.Puntos += 300;
        await db.SaveChangesAsync();                       // UPDATE
        var leido = await db.Tripulantes.FirstAsync(t => t.Id == nuevo.Id);
        Console.WriteLine($"EF Update ➜ {leido.Nombre} ahora tiene {leido.Puntos} pts");

        db.Tripulantes.Remove(leido);
        await db.SaveChangesAsync();                       // DELETE
        Console.WriteLine($"EF Delete ➜ ¿existe? {await db.Tripulantes.AnyAsync(t => t.Id == nuevo.Id)}");
    }
}
