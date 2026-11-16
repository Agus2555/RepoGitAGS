-- ─────────────────────────────────────────────────────────────────────────────
-- 03_datos_muestra.sql
-- Carga datos de ejemplo para probar la API. Alcanza con el usuario administrador:
--   mysql -u administrador -p < sql/03_datos_muestra.sql
--   (en PowerShell:  cmd /c "mysql -u administrador -p < sql\03_datos_muestra.sql")
--
-- Los valores son coherentes con las reglas de cada clase (C#):
--   Guerrero: vida 120, ataque 20, defensa 8, furia 0
--   Mago:     vida  80, ataque 14, defensa 4, maná 40
--   Arquero:  vida 100, ataque 16, defensa 6, flechas 10
--   Asesino:  vida  90, ataque 15, defensa 5, energía 30
-- ─────────────────────────────────────────────────────────────────────────────
USE combate_api;

-- Se vacían las tablas (hijas primero) para poder correr el script más de una vez.
DELETE FROM turnos;
DELETE FROM combates;
DELETE FROM personajes;

-- Conan (5) no participa en ningún combate: sirve para probar DELETE → 204.
INSERT INTO personajes (Id, Nombre, Tipo) VALUES
    (1, 'Thorin',  'Guerrero'),
    (2, 'Gandalf', 'Mago'),
    (3, 'Legolas', 'Arquero'),
    (4, 'Erebus',  'Asesino'),
    (5, 'Conan',   'Guerrero');

-- Combate 1: Thorin vs Gandalf, en curso con 2 turnos (le toca a Thorin).
-- Combate 2: Legolas vs Erebus, sin turnos (se puede modificar o borrar).
-- Combate 3: Erebus vs Gandalf, finalizado, ganó Erebus.
INSERT INTO combates (Id, Personaje1Id, Personaje2Id, VidaP1, VidaP2, RecursoP1, RecursoP2, Estado, GanadorId) VALUES
    (1, 1, 2, 100, 64, 10, 30, 'EnCurso',    NULL),
    (2, 3, 4, 100, 90, 10, 30, 'EnCurso',    NULL),
    (3, 4, 2,  67,  0,  0, 30, 'Finalizado', 4);

-- Cada turno guarda la vida y el recurso de ambos ANTES de jugarse.
--  Combate 1, turno 1: Thorin ataca  → 20 - 4 de defensa = 16 de daño (Gandalf 80 → 64, furia 0 → 10)
--  Combate 1, turno 2: Gandalf usa Bola de Fuego → 28 - 8 = 20 (Thorin 120 → 100, maná 40 → 30)
--  Combate 3, turno 1: Erebus ataca con energía → 45 - 4 = 41 (Gandalf 80 → 39, energía 30 → 15)
--  Combate 3, turno 2: Gandalf usa Bola de Fuego → 28 - 5 = 23 (Erebus 90 → 67, maná 40 → 30)
--  Combate 3, turno 3: Erebus ataca con energía → Gandalf 39 → 0 (energía 15 → 0). Gana Erebus.
INSERT INTO turnos (Id, CombateId, Numero, AtacanteId, DefensorId, Accion, Danio, Nota,
                    VidaP1Antes, VidaP2Antes, RecursoP1Antes, RecursoP2Antes) VALUES
    (1, 1, 1, 1, 2, 'atacar',    16, 'Primer golpe de Thorin', 120, 80,  0, 40),
    (2, 1, 2, 2, 1, 'habilidad', 20, NULL,                     120, 64, 10, 40),
    (3, 3, 1, 4, 2, 'atacar',    41, NULL,                      90, 80, 30, 40),
    (4, 3, 2, 2, 4, 'habilidad', 23, NULL,                      90, 39, 15, 40),
    (5, 3, 3, 4, 2, 'atacar',    39, 'Golpe final',             67, 39, 15, 30);
