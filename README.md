# Discamus · Operación Puente Cuántico

Ruta de estudio 1 a 1 (2 h) para Alejo, que se prepara para una pasantía: bases de datos → SQL/INSERT → procedimientos → conector C# seguro (Npgsql, TLS, parámetros) → CRUD en código → LINQ con EF Core → Scrum/Kanban.

- Web (artifact privado: compartir desde el menú Share): https://claude.ai/artifact/Cxf5EteA9mqBnCyFUTK5e7
- `01_base_puente.sql`: base inicial en PostgreSQL/Supabase; se puede volver a ejecutar para resetear
- `02_demo_inserts_procedimientos.sql`: demo en vivo de los 7 INSERT, el CRUD y CALL
- `03_ruta_y_misiones.md`: guía para el estudiante (orden, misiones, preguntas de entrevista, plan de 4 semanas)
- `csharp/`: consola .NET 8 con 5 demos (`dotnet run -- 0` corre todas). Configurar `.env` desde `.env.example`
- `soluciones/`: solucionario SQL y C# (solo para el profe)
- `web/template.html`: fuente de la web; `python3 build.py` genera `web/puente-cuantico.html`

## Probar sin instalar .NET ni Postgres (Docker)

```bash
docker network create puente-net
docker run -d --name puente-db --network puente-net -e POSTGRES_PASSWORD=clase123 postgres:16-alpine
docker exec -i puente-db psql -U postgres < 01_base_puente.sql
docker run --rm --network puente-net -v "$PWD/csharp":/src -w /src \
  -e DB_HOST=puente-db -e DB_USER=postgres -e DB_PASSWORD=clase123 -e DB_SSL=Disable \
  mcr.microsoft.com/dotnet/sdk:8.0 dotnet run -- 0
```

Validado el 2026-10-30: los scripts SQL, las 5 demos y el solucionario corren completos contra Postgres 16.
