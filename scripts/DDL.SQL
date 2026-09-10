CREATE DATABASE IF NOT EXISTS simulador_combate;
USE simulador_combate;

DROP TABLE IF EXISTS DetalleBatalla;
DROP TABLE IF EXISTS Batallas;
DROP TABLE IF EXISTS PersonajeHabilidades;
DROP TABLE IF EXISTS Habilidades;
DROP TABLE IF EXISTS Personajes;

-- Tabla principal de Personajes (Polimorfismo mapeado mediante Single Table Inheritance)
CREATE TABLE Personajes (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL,
    TipoClase VARCHAR(20) NOT NULL, -- 'Guerrero', 'Mago', 'Arquero', 'Asesino'
    Vida INT NOT NULL,
    VidaMaxima INT NOT NULL,
    Ataque INT NOT NULL,
    Defensa INT NOT NULL,
    RecursoEspecial INT NOT NULL DEFAULT 0, -- Furia, Maná, Flechas o Energía
    CONSTRAINT CHK_Vida CHECK (Vida >= 0),
    CONSTRAINT CHK_Ataque CHECK (Ataque > 0),
    CONSTRAINT CHK_Defensa CHECK (Defensa > 0)
);

-- Tabla de Habilidades
CREATE TABLE Habilidades (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL,
    Costo INT NOT NULL DEFAULT 0,
    Poder INT NOT NULL DEFAULT 0,
    CONSTRAINT CHK_Costo CHECK (Costo >= 0)
);

-- Relación N a N entre Personajes y Habilidades
CREATE TABLE PersonajeHabilidades (
    PersonajeId INT NOT NULL,
    HabilidadId INT NOT NULL,
    PRIMARY KEY (PersonajeId, HabilidadId),
    FOREIGN KEY (PersonajeId) REFERENCES Personajes(Id) ON DELETE CASCADE,
    FOREIGN KEY (HabilidadId) REFERENCES Habilidades(Id) ON DELETE CASCADE
);

-- Tabla de Batallas
CREATE TABLE Batallas (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Personaje1_Id INT NOT NULL,
    Personaje2_Id INT NOT NULL,
    Ganador_Id INT NULL,
    Fecha DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Finalizada BOOLEAN NOT NULL DEFAULT FALSE,
    FOREIGN KEY (Personaje1_Id) REFERENCES Personajes(Id),
    FOREIGN KEY (Personaje2_Id) REFERENCES Personajes(Id),
    FOREIGN KEY (Ganador_Id) REFERENCES Personajes(Id)
);

-- Registro atómico de turnos e historial de combate
CREATE TABLE DetalleBatalla (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    BatallaId INT NOT NULL,
    Turno INT NOT NULL,
    AtacanteId INT NOT NULL,
    DefensorId INT NOT NULL,
    DanioRealizado INT NOT NULL,
    HabilidadUsada VARCHAR(50) NULL,
    FOREIGN KEY (BatallaId) REFERENCES Batallas(Id) ON DELETE CASCADE,
    FOREIGN KEY (AtacanteId) REFERENCES Personajes(Id),
    FOREIGN KEY (DefensorId) REFERENCES Personajes(Id),
    CONSTRAINT CHK_Danio CHECK (DanioRealizado >= 0)
);