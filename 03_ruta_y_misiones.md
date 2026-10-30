# 🎃 Discamus · Operación Puente Embrujado

**Ruta de estudio: de cero a conectar C# con una base de datos (preparación para pasantía)**

> Es noche de Halloween. La nave Discamus necesita conectar su software de control (C#) a la base de datos de la tripulación (PostgreSQL). Entre las tablas ronda el **Fantasma Inyector** 👻, que intenta colarse por los conectores mal hechos. Tu misión: aprender a construir el puente… y blindarlo.

## 🧭 ¿Por dónde empiezo? El orden importa

Todos tus temas encajan en una sola cadena. No se estudian sueltos: cada paso necesita el anterior.

```
1. ¿Qué es una BD?        →  2. SQL (CRUD + INSERT)  →  3. Procedimientos
   relacional vs no rel.        el idioma de la BD          lógica dentro de la BD
                                                                  ↓
6. LINQ (C#)              ←  5. CRUD en código        ←  4. Conector seguro
   consultas sin escribir SQL   C# hace el CRUD             C# ↔ BD, cifrado, sin inyección
                                                                  ↓
                         7. Scrum / Kanban: cómo se organiza el trabajo en un equipo real
```

| Tu lista | Dónde se ve |
|---|---|
| Qué es una BD relacional y no relacional | Paso 1 |
| "Ling es el lenguaje universal de BD" → eso es **SQL** | Paso 2 |
| Insert comunes | Paso 2 |
| Hacer un CRUD básico | Paso 2 (en SQL) y Paso 5 (en código) |
| Procedimientos en bases de datos | Paso 3 |
| Conectores de un lenguaje hacia una BD | Paso 4 |
| Conectores de forma segura · tipo plano y cifrado | Paso 4 |
| Cómo hacer un LINQ (es de **C#**, no de la BD) | Paso 6 |
| Metodologías: Kanban, SCRUM | Paso 7 |

> ⚠️ **SQL ≠ LINQ.** SQL es el idioma que entiende la base de datos (sirve en Postgres, MySQL, SQL Server…). LINQ es una herramienta **de C#** para consultar colecciones; con Entity Framework, C# traduce tu LINQ a SQL por ti.

## ⏱️ La clase (2 horas)

| Min | Paso | Qué hacemos |
|---|---|---|
| 0–10 | Prep | Supabase listo, correr `01_base_puente.sql` |
| 10–20 | 1 | Relacional vs no relacional (tablas vs documentos JSON) |
| 20–45 | 2 | CRUD en SQL + los 7 INSERT más comunes |
| 45–60 | 3 | Procedimientos vs funciones; atomicidad |
| 60–80 | 4 | Conector C#: cadena de conexión, `.env`, cifrado, **SQL injection en vivo** |
| 80–95 | 5 | CRUD en C# con Npgsql |
| 95–110 | 6 | LINQ con Entity Framework: ver el SQL que genera |
| 110–120 | 7 | Scrum vs Kanban + plan de estudio de las próximas semanas |

## 🛠️ Preparación

1. Crea un proyecto gratis en [supabase.com](https://supabase.com) → **SQL Editor** → **New query**.
2. Pega `01_base_puente.sql` y dale **Run**. Debe mostrar **8 tripulantes**.
3. Instala el [.NET 8 SDK](https://dotnet.microsoft.com/download) y comprueba con `dotnet --version`.
4. En `csharp/`, copia `.env.example` como `.env` y llénalo con los datos de Supabase → **Connect** → **Session pooler**.
5. `dotnet run` → elige `1`. Si ves 🔒 **Conexión CIFRADA**, el puente funciona.

### Mapa de la base

```
escuadrones 1───* tripulantes 1───* asignaciones *───1 misiones
codigos_acceso   (la tabla secreta que quiere el Fantasma)
bitacora         (la llenan los procedimientos)
reportes_nave    (documentos JSON: el lado "no relacional")
```

---

## Paso 1 · ¿Qué es una base de datos? 🪐

**Relacional** (PostgreSQL, MySQL, SQL Server, Oracle): los datos van en **tablas** con columnas fijas y se unen con **llaves** (primaria = identifica la fila; foránea = apunta a otra tabla). Se consultan con SQL. Úsala cuando los datos tienen estructura clara y relaciones: usuarios, pedidos, notas, pagos.

**No relacional / NoSQL** (MongoDB, Firebase, Redis, DynamoDB): guarda **documentos** (JSON), pares clave-valor o grafos. Cada registro puede traer campos distintos. Úsala cuando la estructura cambia mucho o necesitas escalar lecturas masivas: logs, catálogos variables, caché, chats.

| | Relacional | No relacional |
|---|---|---|
| Forma | Tablas, filas, columnas | Documentos, clave-valor, grafos |
| Esquema | Fijo (se define antes) | Flexible |
| Relaciones | `JOIN` por llaves | Se anidan o se duplican datos |
| Garantías | ACID (todo o nada) | Varía; prioriza escala/velocidad |

| # | Misión | Resultado esperado |
|---|---|---|
| R1 | ¿Qué tabla une a tripulantes con misiones? ¿Por qué hace falta una tabla en medio? | `asignaciones` (muchos a muchos) |
| R2 | Consulta los `reportes_nave` cuyo documento tiene la llave `combustible`. | 2 filas |
| R3 | Muestra el nombre de cada tripulante con el nombre de su escuadrón (`JOIN`). | 8 filas |

## Paso 2 · SQL: CRUD e INSERT 📡

| Letra | SQL | Ejemplo |
|---|---|---|
| **C**reate | `INSERT` | `INSERT INTO misiones (nombre, destino, dificultad) VALUES ('X', 'Marte', 2);` |
| **R**ead | `SELECT` | `SELECT nombre, puntos FROM tripulantes WHERE rango = 'Piloto';` |
| **U**pdate | `UPDATE` | `UPDATE tripulantes SET puntos = 100 WHERE id = 3;` |
| **D**elete | `DELETE` | `DELETE FROM tripulantes WHERE id = 3;` |

> 🚨 `UPDATE` y `DELETE` **sin `WHERE`** afectan **toda la tabla**. Es el error más famoso de las pasantías.

Los 7 INSERT que más vas a usar están en `02_demo_inserts_procedimientos.sql`: una fila, varias filas, usar `DEFAULT`, `RETURNING`, `INSERT … SELECT`, `ON CONFLICT DO NOTHING` y el *upsert* `ON CONFLICT DO UPDATE`.

| # | Misión | Resultado esperado |
|---|---|---|
| I1 | Inserta un escuadrón con tu nombre y tu color favorito. | 1 fila nueva |
| I2 | Inserta **3 misiones en un solo `INSERT`**. | 3 filas |
| I3 | Inscríbete como tripulante usando los valores por defecto y `RETURNING` para ver tu id, rango y puntos. | rango `Recluta`, 0 puntos |
| I4 | Asígnate a la misión **Eco Lunar** con `INSERT … SELECT`, **sin escribir ningún id a mano**. | 1 fila en `asignaciones` |
| I5 | Cambia el color de tu escuadrón con un *upsert* (`ON CONFLICT … DO UPDATE`). | color cambiado |
| I6 | Provoca a propósito los 3 errores: email repetido, escuadrón que no existe, rango inventado. Anota qué *constraint* saltó. | `UNIQUE`, `FOREIGN KEY`, `CHECK` |

## Paso 3 · Procedimientos y funciones ⚙️

- **PROCEDURE**: ejecuta acciones (insertar, actualizar, registrar). Se llama con `CALL`.
- **FUNCTION**: devuelve un valor o una tabla. Se usa dentro de un `SELECT`.
- Ventaja: la lógica queda **en la base**, igual para cualquier lenguaje que se conecte, y se ejecuta **todo o nada**.

| # | Misión | Resultado esperado |
|---|---|---|
| P1 | `CALL registrar_tripulante(...)` para un compañero y revisa `bitacora`. | 1 evento `ALTA` |
| P2 | Intenta `CALL transferir_puntos(8, 1, 5000);`. Luego revisa los puntos de 1 y 8. | Error y **nadie cambió** (980 / 90) |
| P3 | Crea un procedimiento `ascender(p_id int, p_rango text)` que cambie el rango y escriba `ASCENSO` en la bitácora. Pruébalo. | rango cambiado + 1 evento |

## Paso 4 · El conector seguro 🔐

Un **conector** (driver) es la librería que deja a un lenguaje hablar con la base: en C# con PostgreSQL es **Npgsql**; con SQL Server, `Microsoft.Data.SqlClient`; en Java, JDBC; en Python, `psycopg`.

Las 3 reglas de un conector seguro:

1. **La clave nunca va en el código.** Va en variables de entorno o en `.env`, y `.env` va en `.gitignore`.
2. **Conexión cifrada, no texto plano.** `SslMode=Require` (o `VerifyFull`) cifra con TLS todo lo que viaja por la red. En texto plano, cualquiera en la misma red puede leer claves y datos.
3. **Consultas parametrizadas, nunca SQL armado con texto.** Si pegas lo que escribe el usuario dentro del SQL, te pueden hacer **SQL injection**.

```csharp
// ❌ Inseguro: el texto del usuario se vuelve código SQL
$"SELECT * FROM codigos_acceso WHERE usuario = '{usuario}' AND clave = '{clave}'"

// ✅ Seguro: SQL y datos viajan por separado
cmd.CommandText = "SELECT * FROM codigos_acceso WHERE usuario = @usuario AND clave = @clave";
cmd.Parameters.AddWithValue("usuario", usuario);
```

| # | Misión | Resultado esperado |
|---|---|---|
| C1 | Conecta tu `.env` a Supabase y corre la demo `1`. | 🔒 Conexión CIFRADA |
| C2 | Corre la demo `2`. Explica con tus palabras por qué `' OR '1'='1' --` entró sin clave. | El `--` comenta el resto y `'1'='1'` siempre es verdad |
| C3 | Cambia `DB_SSL` a `Disable` contra Supabase y corre la demo `1`. Luego activa en Supabase → **Database → Settings → SSL Configuration** la opción *Enforce SSL on incoming connections* y vuelve a probar. | Antes: ⚠️ texto plano. Después: Supabase rechaza la conexión |
| 👻 **Jefe** | Agrega a `TripulantesRepositorio` un método `BuscarPorNombre(string texto)` que use `ILIKE` con parámetros. Pruébalo con `x' UNION SELECT usuario, clave FROM codigos_acceso --`: **no debe salir ninguna clave**. | 0 filas con el ataque |

## Paso 5 · CRUD dentro del código 💻

Abre `csharp/Demo3_Crud.cs`: cada letra del CRUD es un método que manda **SQL con parámetros** usando Npgsql.

| # | Misión | Resultado esperado |
|---|---|---|
| K1 | Crea `MisionesRepositorio` con `Crear`, `Listar` y `Eliminar`. | Funciona desde `Program.cs` |
| K2 | `Crear` debe devolver el id usando `RETURNING id`. | id > 0 |
| K3 | ¿Qué pasa si intentas `Crear` con dificultad 9? Atrapa la `PostgresException` y muestra un mensaje amable. | Mensaje propio, no crash |

## Paso 6 · LINQ 🔭

LINQ (*Language Integrated Query*) es parte de **C#**. Funciona con listas en memoria y, con **Entity Framework Core**, con la base de datos: EF traduce tu LINQ a SQL.

```csharp
var pilotos = db.Tripulantes
    .Where(t => t.Rango == "Piloto")
    .OrderByDescending(t => t.Puntos)
    .Select(t => new { t.Nombre, t.Puntos });

Console.WriteLine(pilotos.ToQueryString()); // ← muestra el SQL que se enviará
```

| LINQ | SQL |
|---|---|
| `Where` | `WHERE` |
| `OrderBy` / `OrderByDescending` | `ORDER BY` |
| `Select` | `SELECT columnas` |
| `GroupBy` | `GROUP BY` |
| `Include` | `JOIN` |
| `Count()`, `Sum()`, `Average()` | `COUNT`, `SUM`, `AVG` |
| `Add` + `SaveChanges()` | `INSERT` |

| # | Misión | Resultado esperado |
|---|---|---|
| L1 | Con LINQ, los tripulantes **Recluta con menos de 200 puntos**. | Base recién reseteada: 2 (Mariana, Daniel) + los Reclutas que tú hayas insertado |
| L2 | Promedio de puntos por rango con `GroupBy` + `Average`. | 4 grupos |
| L3 | Imprime el `ToQueryString()` de L1 y compáralo con el SQL que tú escribirías. | Casi idénticos |

## Paso 7 · Metodologías: Scrum y Kanban 🗂️

**Scrum**: trabajo en **sprints** (1–4 semanas) con un objetivo fijo.
- Roles: *Product Owner* (qué se hace), *Scrum Master* (quita bloqueos), *Equipo de desarrollo*.
- Eventos: *Sprint Planning* → *Daily* de 15 min (¿qué hice?, ¿qué haré?, ¿qué me bloquea?) → *Sprint Review* (se muestra lo hecho) → *Retrospectiva* (cómo mejorar).
- Artefactos: *Product Backlog*, *Sprint Backlog*, *Incremento*.

**Kanban**: flujo **continuo** en un tablero con columnas `Por hacer → Haciendo → Hecho`.
- Regla clave: **límite de trabajo en curso (WIP)**: no empezar más de N cosas a la vez.
- Sin sprints; se mide cuánto tarda una tarjeta en cruzar el tablero.

| | Scrum | Kanban |
|---|---|---|
| Ritmo | Sprints fijos | Flujo continuo |
| Roles | Definidos | Opcionales |
| Cambios | Al final del sprint | En cualquier momento |
| Ideal para | Producto nuevo con entregas | Soporte, mantenimiento, tickets |

En una pasantía lo más probable es que entres a un equipo con **Scrum** y un tablero en **Jira**, **Azure DevOps** o **GitHub Projects**.

| # | Misión | Resultado esperado |
|---|---|---|
| S1 | Arma un tablero Kanban (GitHub Projects o Trello) con las tareas de K1–K3. | 3 columnas, WIP ≤ 2 |
| S2 | Escribe 2 historias de usuario: *"Como ___ quiero ___ para ___"* con criterios de aceptación. | 2 historias |

---

## 🎯 Lo que te pueden preguntar en una entrevista de pasantía

1. ¿Diferencia entre base de datos relacional y no relacional? Da un ejemplo de cada una.
2. ¿Qué es una llave primaria y una llave foránea?
3. ¿Qué hace un `JOIN`? ¿Diferencia entre `INNER JOIN` y `LEFT JOIN`?
4. ¿Qué es SQL injection y cómo la evitas?
5. ¿Dónde guardas la contraseña de la base de datos en un proyecto?
6. ¿Diferencia entre un procedimiento almacenado y una función?
7. ¿Qué significa ACID / que una transacción sea atómica?
8. ¿Qué es un ORM? ¿Qué ventaja tiene Entity Framework frente a escribir SQL a mano, y qué desventaja?
9. ¿Qué es un sprint? ¿Qué se dice en una daily?
10. ¿Scrum o Kanban para un equipo de soporte? ¿Por qué?

## 📅 Plan de estudio para después de la clase (4 semanas, ~1 h al día)

| Semana | Enfoque | Meta concreta |
|---|---|---|
| 1 | SQL | Terminar Pasos 1–3 y 20 ejercicios en [sqlbolt.com](https://sqlbolt.com) |
| 2 | C# + conector | Pasos 4–5: tu propio CRUD de una tabla nueva (por ejemplo, `tareas`) |
| 3 | LINQ + EF Core | Paso 6 y rehacer el CRUD de la semana 2 con EF |
| 4 | Proyecto + metodología | Mini app "gestor de tareas" en GitHub, con tablero Kanban y un README. **Esto va al CV y a LinkedIn.** |
