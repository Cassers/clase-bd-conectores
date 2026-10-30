-- =====================================================================
-- SOLUCIONARIO (solo profe) · Pasos 1, 2, 3 y referencia SQL del Paso 6
-- Correr sobre una base recién reseteada con 01_base_puente.sql
-- =====================================================================

-- R1: asignaciones (tabla puente muchos a muchos)
-- R2 → 2 filas
SELECT * FROM reportes_nave WHERE documento ? 'combustible';
-- R3 → 8 filas
SELECT t.nombre, e.nombre AS escuadron
FROM tripulantes t JOIN escuadrones e ON e.id = t.escuadron_id;

-- I1
INSERT INTO escuadrones (nombre, color) VALUES ('Alejo', 'verde');
-- I2
INSERT INTO misiones (nombre, destino, dificultad) VALUES
  ('Anillo de Saturno', 'Saturno', 3),
  ('Polvo Rojo',        'Marte',   2),
  ('Noche Eterna',      'Plutón',  5);
-- I3
INSERT INTO tripulantes (nombre, email, escuadron_id)
VALUES ('Alejo', 'alejo@discamus.net', (SELECT id FROM escuadrones WHERE nombre = 'Alejo'))
RETURNING id, rango, puntos;
-- I4
INSERT INTO asignaciones (tripulante_id, mision_id, rol)
SELECT t.id, m.id, 'Cadete'
FROM tripulantes t, misiones m
WHERE t.email = 'alejo@discamus.net' AND m.nombre = 'Eco Lunar';
-- I5
INSERT INTO escuadrones (nombre, color) VALUES ('Alejo', 'negro')
ON CONFLICT (nombre) DO UPDATE SET color = EXCLUDED.color;
-- I6 (cada una falla con un constraint distinto)
-- INSERT INTO tripulantes (nombre, email) VALUES ('X', 'mateo@discamus.net');            -- UNIQUE
-- INSERT INTO tripulantes (nombre, email, escuadron_id) VALUES ('X', 'x@x.net', 99);    -- FOREIGN KEY
-- INSERT INTO tripulantes (nombre, email, rango) VALUES ('X', 'y@y.net', 'Almirante');  -- CHECK

-- P1
CALL registrar_tripulante('Compañero Uno', 'uno@discamus.net', 2);
SELECT * FROM bitacora WHERE evento = 'ALTA';
-- P2: falla y Valentina (1) sigue con 980, Daniel (8) con 90
-- CALL transferir_puntos(8, 1, 5000);
SELECT id, puntos FROM tripulantes WHERE id IN (1, 8);
-- P3
CREATE OR REPLACE PROCEDURE ascender(p_id int, p_rango text)
LANGUAGE plpgsql AS $$
BEGIN
  UPDATE tripulantes SET rango = p_rango WHERE id = p_id;
  IF NOT FOUND THEN
    RAISE EXCEPTION 'No existe el tripulante %', p_id;
  END IF;
  INSERT INTO bitacora (evento, detalle)
  VALUES ('ASCENSO', 'Tripulante ' || p_id || ' ahora es ' || p_rango);
END;
$$;
CALL ascender(5, 'Piloto');
SELECT id, nombre, rango FROM tripulantes WHERE id = 5;

-- L1 en SQL (para comparar con el ToQueryString de LINQ) → 2 filas
SELECT nombre, puntos FROM tripulantes WHERE rango = 'Recluta' AND puntos < 200;
-- L2 en SQL
SELECT rango, avg(puntos) FROM tripulantes GROUP BY rango;
