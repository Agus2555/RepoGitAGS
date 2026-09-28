CREATE DATABASE IF NOT EXISTS simulador_combate;
USE simulador_combate;

-- Orden de borrado: tablas hija primero para respetar las FK.
DROP TABLE IF EXISTS DetalleBatalla;
DROP TABLE IF EXISTS ParticipantesBatalla;
DROP TABLE IF EXISTS Batallas;
DROP TABLE IF EXISTS Personajes;

-- ─────────────────────────────────────────────────────────────────────────────
-- Personajes (Single Table Inheritance): una fila por personaje,
-- la columna TipoClase distingue la subclase (Guerrero, Mago, Arquero, Asesino).
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE Personajes (
    Id              INT AUTO_INCREMENT PRIMARY KEY,
    Nombre          VARCHAR(50)  NOT NULL,
    TipoClase       VARCHAR(20)  NOT NULL,
    Vida            INT          NOT NULL,
    VidaMaxima      INT          NOT NULL,
    Ataque          INT          NOT NULL,
    Defensa         INT          NOT NULL,
    RecursoEspecial INT          NOT NULL DEFAULT 0,
    CONSTRAINT CHK_TipoClase   CHECK (TipoClase IN ('Guerrero','Mago','Arquero','Asesino')),
    CONSTRAINT CHK_Vida        CHECK (Vida >= 0),
    CONSTRAINT CHK_VidaMaxima  CHECK (VidaMaxima > 0),
    CONSTRAINT CHK_VidaRango   CHECK (Vida <= VidaMaxima),
    CONSTRAINT CHK_Ataque      CHECK (Ataque > 0),
    CONSTRAINT CHK_Defensa     CHECK (Defensa > 0),
    CONSTRAINT CHK_Recurso     CHECK (RecursoEspecial >= 0)
);

-- ─────────────────────────────────────────────────────────────────────────────
-- Batallas: cada fila representa un enfrentamiento entre dos personajes.
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE Batallas (
    Id            INT AUTO_INCREMENT PRIMARY KEY,
    Personaje1_Id INT      NOT NULL,
    Personaje2_Id INT      NOT NULL,
    Ganador_Id    INT      NULL,
    Fecha         DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Finalizada    BOOLEAN  NOT NULL DEFAULT FALSE,
    CONSTRAINT CHK_NoBatallaConsigoMismo CHECK (Personaje1_Id <> Personaje2_Id),
    FOREIGN KEY (Personaje1_Id) REFERENCES Personajes(Id),
    FOREIGN KEY (Personaje2_Id) REFERENCES Personajes(Id),
    FOREIGN KEY (Ganador_Id)    REFERENCES Personajes(Id)
);

CREATE INDEX idx_batallas_fecha ON Batallas(Fecha);

-- ─────────────────────────────────────────────────────────────────────────────
-- ParticipantesBatalla: estado inicial y resultado final de cada combatiente.
-- Permite las estadísticas de daño sin producto cartesiano.
-- sp_RegistrarBatalla la llena al inicio; sp_FinalizarBatalla la cierra.
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE ParticipantesBatalla (
    BatallaId       INT     NOT NULL,
    PersonajeId     INT     NOT NULL,
    VidaInicial     INT     NOT NULL,
    RecursoInicial  INT     NOT NULL,
    VidaFinal       INT     NULL,       -- se actualiza al cerrar la batalla
    DanioRealizado  INT     NOT NULL DEFAULT 0,
    DanioRecibido   INT     NOT NULL DEFAULT 0,
    Resultado       ENUM('Victoria','Derrota','Empate') NULL,
    PRIMARY KEY (BatallaId, PersonajeId),
    FOREIGN KEY (BatallaId)   REFERENCES Batallas(Id) ON DELETE CASCADE,
    FOREIGN KEY (PersonajeId) REFERENCES Personajes(Id)
);

CREATE INDEX idx_participantes_personaje ON ParticipantesBatalla(PersonajeId);

-- ─────────────────────────────────────────────────────────────────────────────
-- DetalleBatalla: historial turno a turno del combate.
-- sp_RegistrarTurnoYActualizarDanio la llena y acumula daño en ParticipantesBatalla.
-- ─────────────────────────────────────────────────────────────────────────────
CREATE TABLE DetalleBatalla (
    Id             INT AUTO_INCREMENT PRIMARY KEY,
    BatallaId      INT         NOT NULL,
    Turno          INT         NOT NULL,
    AtacanteId     INT         NOT NULL,
    DefensorId     INT         NOT NULL,
    DanioRealizado INT         NOT NULL,
    Accion         VARCHAR(50) NULL,     -- nombre de la acción ejecutada
    CONSTRAINT CHK_Danio CHECK (DanioRealizado >= 0),
    FOREIGN KEY (BatallaId)  REFERENCES Batallas(Id) ON DELETE CASCADE,
    FOREIGN KEY (AtacanteId) REFERENCES Personajes(Id),
    FOREIGN KEY (DefensorId) REFERENCES Personajes(Id)
);

CREATE INDEX idx_detalle_batalla  ON DetalleBatalla(BatallaId);
CREATE INDEX idx_detalle_atacante ON DetalleBatalla(AtacanteId);
