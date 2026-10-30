-- =====================================================================
-- Demo en vivo: INSERT comunes, CRUD en SQL y procedimientos
-- Ejecutar bloque por bloque en el SQL Editor de Supabase
-- (antes, correr 01_base_puente.sql)
-- =====================================================================

-- ---------------------------------------------------------------------
-- A. Relacional vs no relacional
-- ---------------------------------------------------------------------
-- Relacional: columnas fijas, se unen tablas con JOIN por llaves
SELECT t.nombre, e.nombre AS escuadron, m.nombre AS mision, a.rol
FROM asignaciones a
JOIN tripulantes t ON t.id = a.tripulante_id
JOIN escuadrones e ON e.id = t.escuadron_id
JOIN misiones m    ON m.id = a.mision_id
ORDER BY m.nombre;

-- "No relacional": cada documento trae los campos que quiera
SELECT documento FROM reportes_nave;
SELECT documento->>'nave' AS nave, documento->'alertas' AS alertas
FROM reportes_nave
WHERE documento ? 'alertas';

-- ---------------------------------------------------------------------
-- B. Los INSERT más comunes
-- ---------------------------------------------------------------------
-- 1) Una fila, nombrando columnas (SIEMPRE nombrar columnas)
INSERT INTO escuadrones (nombre, color) VALUES ('Pegaso', 'blanco');

-- 2) Varias filas en un solo INSERT
INSERT INTO misiones (nombre, destino, dificultad) VALUES
  ('Ruta de Hielo', 'Europa', 2),
  ('Faro Rojo',     'Marte',  3);

-- 3) Usar los DEFAULT: rango='Recluta', puntos=0, activo=true, creado_en=now()
INSERT INTO tripulantes (nombre, email, escuadron_id)
VALUES ('Alejo Prueba', 'alejo@discamus.net', 4);

-- 4) RETURNING: devuelve el id generado (clave para usar desde código)
INSERT INTO tripulantes (nombre, email, escuadron_id)
VALUES ('Lucía Beltrán', 'lucia@discamus.net', 2)
RETURNING id, rango, puntos;

-- 5) INSERT ... SELECT: insertar el resultado de una consulta
--    Asignar a todos los Pilotos a la misión 'Faro Rojo'
INSERT INTO asignaciones (tripulante_id, mision_id, rol)
SELECT t.id, m.id, 'Piloto'
FROM tripulantes t, misiones m
WHERE t.rango = 'Piloto' AND m.nombre = 'Faro Rojo';

-- 6) ON CONFLICT DO NOTHING: no fallar si ya existe
INSERT INTO escuadrones (nombre, color) VALUES ('Orión', 'azul')
ON CONFLICT (nombre) DO NOTHING;

-- 7) UPSERT: si existe, actualiza
INSERT INTO escuadrones (nombre, color) VALUES ('Orión', 'azul eléctrico')
ON CONFLICT (nombre) DO UPDATE SET color = EXCLUDED.color;

-- Errores típicos para mostrar (descomentar uno a uno):
-- INSERT INTO tripulantes (nombre, email) VALUES ('Repetida', 'mateo@discamus.net');      -- UNIQUE
-- INSERT INTO tripulantes (nombre, email, escuadron_id) VALUES ('X', 'x@x.net', 99);    -- FOREIGN KEY
-- INSERT INTO tripulantes (nombre, email, rango) VALUES ('Y', 'y@y.net', 'Almirante');  -- CHECK

-- ---------------------------------------------------------------------
-- C. CRUD completo en SQL
-- ---------------------------------------------------------------------
-- C = Create
INSERT INTO tripulantes (nombre, email, escuadron_id)
VALUES ('Nuevo Cadete', 'nuevo@discamus.net', 1) RETURNING id;
-- R = Read
SELECT id, nombre, rango, puntos FROM tripulantes WHERE email = 'nuevo@discamus.net';
-- U = Update (¡SIEMPRE con WHERE!)
UPDATE tripulantes SET puntos = puntos + 50, rango = 'Piloto'
WHERE email = 'nuevo@discamus.net';
-- D = Delete (¡SIEMPRE con WHERE!)
DELETE FROM tripulantes WHERE email = 'nuevo@discamus.net';

-- ---------------------------------------------------------------------
-- D. Procedimientos y funciones (ya creados en 01_base_puente.sql)
-- ---------------------------------------------------------------------
CALL registrar_tripulante('Camila Ruiz', 'CAMILA@Discamus.net', 3);
SELECT * FROM bitacora ORDER BY id DESC;

CALL transferir_puntos(1, 8, 100);           -- Valentina le pasa 100 a Daniel
SELECT id, nombre, puntos FROM tripulantes WHERE id IN (1, 8);

-- Falla y NO cambia nada (Daniel no tiene 5000 puntos)
-- CALL transferir_puntos(8, 1, 5000);

SELECT * FROM ranking_escuadron(1);
