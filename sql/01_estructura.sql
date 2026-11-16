-- ─────────────────────────────────────────────────────────────────────────────
-- 01_estructura.sql
-- Crea la base combate_api y sus tres tablas: personajes, combates y turnos.
-- Se ejecuta con root (hace falta permiso para crear la base):
--   mysql -u root -p < sql/01_estructura.sql
--   (en PowerShell:  cmd /c "mysql -u root -p < sql\01_estructura.sql")
-- OJO: si las tablas ya existían las borra (se pierden los datos).
-- ─────────────────────────────────────────────────────────────────────────────
CREATE DATABASE IF NOT EXISTS combate_api CHARACTER SET utf8mb4;

USE combate_api;

-- Se borran en orden inverso por las claves foráneas (primero las tablas hijas).
DROP TABLE IF EXISTS turnos;
DROP TABLE IF EXISTS combates;
DROP TABLE IF EXISTS personajes;

-- ─────────────────────────────────────────────────────────────────────────────
-- personajes: solo nombre y tipo.
-- Las estadísticas (vida, ataque, defensa y recurso) son reglas del dominio,
-- salen de la clase de cada tipo en C# (Guerrero, Mago, Arquero, Asesino).
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE personajes (
    Id     INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL,
    Tipo   VARCHAR(20) NOT NULL,
    CONSTRAINT CHK_Personajes_Tipo CHECK (Tipo IN ('Guerrero', 'Mago', 'Arquero', 'Asesino'))
);

-- ─────────────────────────────────────────────────────────────────────────────
-- combates: los dos participantes y el estado ACTUAL de la pelea
-- (vida y recurso de cada uno dentro de este combate, estado y ganador).
-- Así un mismo personaje puede pelear en varios combates sin mezclar vidas.
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE combates (
    Id           INT AUTO_INCREMENT PRIMARY KEY,
    Personaje1Id INT      NOT NULL,
    Personaje2Id INT      NOT NULL,
    VidaP1       INT      NOT NULL,
    VidaP2       INT      NOT NULL,
    RecursoP1    INT      NOT NULL,
    RecursoP2    INT      NOT NULL,
    Estado       ENUM('EnCurso', 'Finalizado') NOT NULL DEFAULT 'EnCurso',
    GanadorId    INT      NULL,
    Fecha        DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT CHK_Combates_Distintos CHECK (Personaje1Id <> Personaje2Id),
    CONSTRAINT CHK_Combates_Vidas     CHECK (VidaP1 >= 0 AND VidaP2 >= 0),
    CONSTRAINT CHK_Combates_Recursos  CHECK (RecursoP1 >= 0 AND RecursoP2 >= 0),
    CONSTRAINT FK_Combates_P1      FOREIGN KEY (Personaje1Id) REFERENCES personajes(Id),
    CONSTRAINT FK_Combates_P2      FOREIGN KEY (Personaje2Id) REFERENCES personajes(Id),
    CONSTRAINT FK_Combates_Ganador FOREIGN KEY (GanadorId)    REFERENCES personajes(Id)
);

-- ─────────────────────────────────────────────────────────────────────────────
-- turnos: historial del combate.
-- Nota: lo único editable del turno.
-- VidaP1Antes ... RecursoP2Antes: la "foto" de ambos participantes ANTES del
-- turno; es lo que se usa para deshacer el último turno.
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE turnos (
    Id             INT AUTO_INCREMENT PRIMARY KEY,
    CombateId      INT          NOT NULL,
    Numero         INT          NOT NULL,
    AtacanteId     INT          NOT NULL,
    DefensorId     INT          NOT NULL,
    Accion         ENUM('atacar', 'habilidad') NOT NULL,
    Danio          INT          NOT NULL,
    Nota           VARCHAR(200) NULL,
    VidaP1Antes    INT          NOT NULL,
    VidaP2Antes    INT          NOT NULL,
    RecursoP1Antes INT          NOT NULL,
    RecursoP2Antes INT          NOT NULL,
    Fecha          DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    -- No puede haber dos "turno 3" en el mismo combate.
    CONSTRAINT UQ_Turnos_CombateNumero UNIQUE (CombateId, Numero),
    CONSTRAINT CHK_Turnos_Danio CHECK (Danio >= 0),
    -- Sin ON DELETE CASCADE: un combate con turnos no se puede borrar.
    CONSTRAINT FK_Turnos_Combate  FOREIGN KEY (CombateId)  REFERENCES combates(Id),
    CONSTRAINT FK_Turnos_Atacante FOREIGN KEY (AtacanteId) REFERENCES personajes(Id),
    CONSTRAINT FK_Turnos_Defensor FOREIGN KEY (DefensorId) REFERENCES personajes(Id)
);
