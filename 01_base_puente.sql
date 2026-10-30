-- =====================================================================
-- Discamus Academy · Operación Puente Embrujado (Halloween)
-- Base de datos inicial (PostgreSQL / Supabase)
-- Se puede re-ejecutar: borra todo y lo deja como nuevo.
-- =====================================================================

DROP TABLE IF EXISTS bitacora, asignaciones, reportes_nave, codigos_acceso, misiones, tripulantes, escuadrones CASCADE;
DROP PROCEDURE IF EXISTS registrar_tripulante(text, text, int);
DROP PROCEDURE IF EXISTS transferir_puntos(int, int, int);
DROP FUNCTION IF EXISTS ranking_escuadron(int);

-- ---------------------------------------------------------------------
-- 1. Tablas RELACIONALES: filas y columnas fijas, unidas por llaves
-- ---------------------------------------------------------------------
CREATE TABLE escuadrones (
  id      int  GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  nombre  text NOT NULL UNIQUE,
  color   text NOT NULL
);

CREATE TABLE tripulantes (
  id            int         GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  nombre        text        NOT NULL,
  email         text        NOT NULL UNIQUE,
  rango         text        NOT NULL DEFAULT 'Recluta'
                            CHECK (rango IN ('Recluta', 'Piloto', 'Ingeniero', 'Capitán')),
  puntos        int         NOT NULL DEFAULT 0 CHECK (puntos >= 0),
  escuadron_id  int         REFERENCES escuadrones(id),
  activo        boolean     NOT NULL DEFAULT true,
  creado_en     timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE misiones (
  id          int  GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  nombre      text NOT NULL,
  destino     text NOT NULL,
  dificultad  int  NOT NULL CHECK (dificultad BETWEEN 1 AND 5)
);

-- Tabla puente: relación muchos a muchos tripulantes <-> misiones
CREATE TABLE asignaciones (
  tripulante_id int  REFERENCES tripulantes(id) ON DELETE CASCADE,
  mision_id     int  REFERENCES misiones(id) ON DELETE CASCADE,
  rol           text NOT NULL,
  PRIMARY KEY (tripulante_id, mision_id)
);

-- Tabla "secreta" que el Fantasma Inyector quiere robar
CREATE TABLE codigos_acceso (
  id        int  GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  usuario   text NOT NULL UNIQUE,
  clave     text NOT NULL,          -- en la vida real: NUNCA en texto plano, siempre un hash (bcrypt/argon2)
  nivel     text NOT NULL
);

-- Bitácora que llenan los procedimientos
CREATE TABLE bitacora (
  id      int         GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  evento  text        NOT NULL,
  detalle text,
  fecha   timestamptz NOT NULL DEFAULT now()
);

-- ---------------------------------------------------------------------
-- 2. Tabla estilo NO RELACIONAL: un documento JSON por fila (como MongoDB)
--    Cada reporte puede traer campos distintos, sin esquema fijo.
-- ---------------------------------------------------------------------
CREATE TABLE reportes_nave (
  id         int         GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  documento  jsonb       NOT NULL,
  fecha      timestamptz NOT NULL DEFAULT now()
);

-- ---------------------------------------------------------------------
-- 3. Datos iniciales
-- ---------------------------------------------------------------------
INSERT INTO escuadrones (nombre, color) VALUES
  ('Orión',     'azul'),
  ('Andrómeda', 'violeta'),
  ('Fénix',     'naranja');

INSERT INTO tripulantes (nombre, email, rango, puntos, escuadron_id) VALUES
  ('Valentina Ríos',  'valentina@discamus.net', 'Capitán',   980, 1),
  ('Mateo Cárdenas',  'mateo@discamus.net',     'Piloto',    720, 1),
  ('Sofía Herrera',   'sofia@discamus.net',     'Ingeniero', 650, 2),
  ('Samuel Ortiz',    'samuel@discamus.net',    'Piloto',    540, 3),
  ('Isabella Mora',   'isabella@discamus.net',  'Recluta',   310, 2),
  ('Tomás Vargas',    'tomas@discamus.net',     'Ingeniero', 480, 3),
  ('Mariana López',   'mariana@discamus.net',   'Recluta',   120, 1),
  ('Daniel Pineda',   'daniel@discamus.net',    'Recluta',    90, 3);

INSERT INTO misiones (nombre, destino, dificultad) VALUES
  ('Puente Cuántico',   'Estación Kepler',  4),
  ('Lluvia de Cometas', 'Cinturón de Kuiper', 3),
  ('Eco Lunar',         'Luna',             1),
  ('Sombra de Titán',   'Titán',            5);

INSERT INTO asignaciones (tripulante_id, mision_id, rol) VALUES
  (1, 1, 'Comandante'), (2, 1, 'Piloto'), (3, 1, 'Ingeniera'),
  (4, 2, 'Piloto'),     (6, 2, 'Ingeniero'),
  (5, 3, 'Navegante'),  (7, 3, 'Apoyo'),
  (1, 4, 'Comandante'), (2, 4, 'Piloto');

INSERT INTO codigos_acceso (usuario, clave, nivel) VALUES
  ('valentina', 'Orion#2126', 'comando'),
  ('mateo',     'Cometa77',   'piloto'),
  ('sofia',     'Reactor!9',  'ingenieria');

INSERT INTO reportes_nave (documento) VALUES
  ('{"nave": "Discamus I", "combustible": 82, "alertas": []}'),
  ('{"nave": "Discamus II", "combustible": 15, "alertas": ["motor 2", "oxígeno bajo"], "capitan": "Valentina"}'),
  ('{"nave": "Fénix", "sensores": {"temperatura": -40, "radiacion": "alta"}}');

-- ---------------------------------------------------------------------
-- 4. Procedimientos y funciones
-- ---------------------------------------------------------------------

-- PROCEDURE: hace algo (no devuelve tabla). Se llama con CALL.
CREATE PROCEDURE registrar_tripulante(p_nombre text, p_email text, p_escuadron int)
LANGUAGE plpgsql AS $$
BEGIN
  INSERT INTO tripulantes (nombre, email, escuadron_id)
  VALUES (p_nombre, lower(p_email), p_escuadron);

  INSERT INTO bitacora (evento, detalle)
  VALUES ('ALTA', p_nombre || ' se unió a la tripulación');
END;
$$;

-- PROCEDURE con validación: o se hace todo, o no se hace nada.
CREATE PROCEDURE transferir_puntos(p_origen int, p_destino int, p_cantidad int)
LANGUAGE plpgsql AS $$
BEGIN
  IF p_cantidad <= 0 THEN
    RAISE EXCEPTION 'La cantidad debe ser positiva';
  END IF;

  UPDATE tripulantes SET puntos = puntos + p_cantidad WHERE id = p_destino;

  -- El destino YA recibió los puntos... pero si el origen no tiene suficientes,
  -- el error deshace TODO el procedimiento (atomicidad): nadie gana puntos de la nada.
  IF (SELECT puntos FROM tripulantes WHERE id = p_origen) < p_cantidad THEN
    RAISE EXCEPTION 'Puntos insuficientes: el tripulante % no tiene % puntos', p_origen, p_cantidad;
  END IF;

  UPDATE tripulantes SET puntos = puntos - p_cantidad WHERE id = p_origen;

  INSERT INTO bitacora (evento, detalle)
  VALUES ('TRANSFERENCIA', p_cantidad || ' puntos de ' || p_origen || ' a ' || p_destino);
END;
$$;

-- FUNCTION: devuelve un resultado. Se usa dentro de un SELECT.
CREATE FUNCTION ranking_escuadron(p_escuadron int)
RETURNS TABLE (puesto bigint, nombre text, rango text, puntos int)
LANGUAGE sql AS $$
  SELECT row_number() OVER (ORDER BY t.puntos DESC), t.nombre, t.rango, t.puntos
  FROM tripulantes t
  WHERE t.escuadron_id = p_escuadron AND t.activo
  ORDER BY t.puntos DESC;
$$;

-- Comprobación rápida: debe mostrar 8 tripulantes
SELECT id, nombre, rango, puntos FROM tripulantes ORDER BY id;
