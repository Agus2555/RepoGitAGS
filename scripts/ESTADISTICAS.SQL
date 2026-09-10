USE simulador_combate;

DELIMITER //

-- Cantidad de batallas por tipo de personaje
DROP PROCEDURE IF EXISTS sp_CantidadBatallasPorTipoPersonaje //
CREATE PROCEDURE sp_CantidadBatallasPorTipoPersonaje()
BEGIN
    SELECT 
        p.TipoClase,
        COUNT(b.Id) AS TotalBatallas
    FROM Personajes p
    INNER JOIN Batallas b ON p.Id = b.Personaje1_Id OR p.Id = b.Personaje2_Id
    GROUP BY p.TipoClase;
END //

-- Victorias y derrotas por personaje
DROP PROCEDURE IF EXISTS sp_RankingVictoriasYDerrotas //
CREATE PROCEDURE sp_RankingVictoriasYDerrotas()
BEGIN
    SELECT 
        p.Id,
        p.Nombre,
        p.TipoClase,
        SUM(CASE WHEN b.Ganador_Id = p.Id THEN 1 ELSE 0 END) AS Victorias,
        SUM(CASE WHEN b.Ganador_Id IS NOT NULL AND b.Ganador_Id != p.Id THEN 1 ELSE 0 END) AS Derrotas
    FROM Personajes p
    LEFT JOIN Batallas b ON (p.Id = b.Personaje1_Id OR p.Id = b.Personaje2_Id) AND b.Finalizada = TRUE
    GROUP BY p.Id, p.Nombre, p.TipoClase
    ORDER BY Victorias DESC;
END //

-- Daño promedio por clase de personaje y por combate
DROP PROCEDURE IF EXISTS sp_DanioPromedioPorClase //
CREATE PROCEDURE sp_DanioPromedioPorClase()
BEGIN
    SELECT 
        p.TipoClase,
        AVG(db.DanioRealizado) AS DanioPromedioPorTurno
    FROM DetalleBatalla db
    INNER JOIN Personajes p ON db.AtacanteId = p.Id
    GROUP BY p.TipoClase;
END //

-- Daño total realizado y recibido por personaje en un período
DROP PROCEDURE IF EXISTS sp_DanioPorPersonajeEnPeriodo //
CREATE PROCEDURE sp_DanioPorPersonajeEnPeriodo(
    IN p_FechaInicio DATETIME,
    IN p_FechaFin DATETIME
)
BEGIN
    SELECT 
        p.Id,
        p.Nombre,
        COALESCE(SUM(db_atq.DanioRealizado), 0) AS DanioTotalRealizado,
        COALESCE(SUM(db_def.DanioRealizado), 0) AS DanioTotalRecibido
    FROM Personajes p
    LEFT JOIN DetalleBatalla db_atq ON p.Id = db_atq.AtacanteId
    LEFT JOIN Batallas b_atq ON db_atq.BatallaId = b_atq.Id AND b_atq.Fecha BETWEEN p_FechaInicio AND p_FechaFin
    LEFT JOIN DetalleBatalla db_def ON p.Id = db_def.DefensorId
    LEFT JOIN Batallas b_def ON db_def.BatallaId = b_def.Id AND b_def.Fecha BETWEEN p_FechaInicio AND p_FechaFin
    GROUP BY p.Id, p.Nombre;
END //

-- Batallas realizadas por período
DROP PROCEDURE IF EXISTS sp_BatallasPorPeriodo //
CREATE PROCEDURE sp_BatallasPorPeriodo(
    IN p_FechaInicio DATETIME,
    IN p_FechaFin DATETIME
)
BEGIN
    SELECT 
        b.Id,
        p1.Nombre AS Personaje1,
        p2.Nombre AS Personaje2,
        pg.Nombre AS Ganador,
        b.Fecha,
        b.Finalizada
    FROM Batallas b
    INNER JOIN Personajes p1 ON b.Personaje1_Id = p1.Id
    INNER JOIN Personajes p2 ON b.Personaje2_Id = p2.Id
    LEFT JOIN Personajes pg ON b.Ganador_Id = pg.Id
    WHERE b.Fecha BETWEEN p_FechaInicio AND p_FechaFin;
END //

DELIMITER ;